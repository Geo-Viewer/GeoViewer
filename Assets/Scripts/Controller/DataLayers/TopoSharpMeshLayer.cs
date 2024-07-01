using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Controller.Map;
using GeoViewer.Controller.Networking;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A Mesh Layer for creating a mesh based on the toposharp api
    /// </summary>
    public class TopoSharpMeshLayer : DataLayer<TopoSharpMeshLayerSettings, IReadOnlyList<GlobePoint>>, IMeshLayer
    {
        public const string MinLatIdentifier = "{minlat}";
        public const string MaxLatIdentifier = "{maxlat}";
        public const string MinLonIdentifier = "{minlon}";
        public const string MaxLonIdentifier = "{maxlon}";
        public const string ResolutionIdentifier = "{resolution}";

        private readonly HttpClient _client;

        /// <summary>
        /// Creates a new Instance of the <see cref="TopoSharpMeshLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="TopoSharpMeshLayer"/></param>
        public TopoSharpMeshLayer(TopoSharpMeshLayerSettings settings) : base(settings)
        {
            _client = HttpClientFactory.CreateClient(new Uri(Settings.Url));
        }

        /// <inheritdoc/>
        public override void RenderData(IReadOnlyList<GlobePoint> data, TileGameObject tileGameObject,
            MapRenderer mapRenderer)
        {
            var midpoint =
                mapRenderer.ApplicationPositionToWorldPosition(
                    mapRenderer.GlobePointToApplicationPosition(GlobePoint.MidPoint(data[0], data[^1])));
            midpoint.y = 0;
            var vertices = data.Select(point =>
                mapRenderer.ApplicationPositionToWorldPosition(mapRenderer.GlobePointToApplicationPosition(point)) -
                midpoint).ToArray();
            tileGameObject.SetMesh(MeshBuilder.BuildMesh(vertices), Priority);
 
            foreach (var neighbour in mapRenderer.GetRenderedNeighbours(tileGameObject))
            {
                tileGameObject.AdjustVertexHeight(neighbour.tileGameObject, neighbour.direction,
                    (uint)Settings.MeshResolution);
            }
        }

        /// <inheritdoc/>
        protected override async Task<IReadOnlyList<GlobePoint>> RequestDataInternal(
            (TileId tileId, GlobeArea globeArea) request, CancellationToken token)
        {
            var globePoints = request.globeArea.GetPointGrid(Settings.MeshResolution);

            var uri = Settings.Url.Replace(MinLatIdentifier,
                    request.globeArea.BoundsLat.Min.ToString(CultureInfo.InvariantCulture))
                .Replace(MaxLatIdentifier, request.globeArea.BoundsLat.Max.ToString(CultureInfo.InvariantCulture))
                .Replace(MinLonIdentifier, request.globeArea.BoundsLon.Min.ToString(CultureInfo.InvariantCulture))
                .Replace(MaxLonIdentifier, request.globeArea.BoundsLon.Max.ToString(CultureInfo.InvariantCulture))
                .Replace(ResolutionIdentifier, Settings.MeshResolution.ToString(CultureInfo.InvariantCulture));

            //send web request
            //using string.empty for the requestUri as the base address is set in client
            var response =
                await _client.GetAsync(uri, token);
            response.EnsureSuccessStatusCode();

            //convert json response to object
            var text = await response.Content.ReadAsStringAsync();
            var result = JsonUtility.FromJson<OtdStrippedResponse>(text);

            //overwrite globePoint elevation
            for (var i = 0; i < result.results.Count; i++)
            {
                globePoints[i].Altitude = result.results[i].elevation;
            }

            return globePoints;
        }

        #region JsonClasses

        //these classes are needed to convert objects to a json request and back
        //therefore they cannot follow naming conventions

        [Serializable]
        private class OtdStrippedResponse
        {
            public List<OtdStrippedResponsePoint> results;
        }

        [Serializable]
        private class OtdStrippedResponsePoint
        {
            public double elevation;
        }

        #endregion JsonClasses
    }
}