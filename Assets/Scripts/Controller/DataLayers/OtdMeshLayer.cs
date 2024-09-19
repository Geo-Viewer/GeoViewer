using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Controller.Map;
using GeoViewer.Controller.Networking;
using GeoViewer.Controller.Util;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A Mesh Layer for creating a mesh based on the opentopodata/google api
    /// </summary>
    public class OtdMeshLayer : DataLayer<OtdMeshLayerSettings, IReadOnlyList<GlobePoint>>, IMeshLayer
    {
        /// <summary>
        /// Stores a client for requesting data.
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Creates a new Instance of the <see cref="OtdMeshLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="OtdMeshLayer"/></param>
        public OtdMeshLayer(OtdMeshLayerSettings settings) : base(settings)
        {
            _client = HttpClientFactory.CreateClient(new Uri(_settings.Url));
        }

        /// <inheritdoc/>
        protected override void RenderDataInternal(IReadOnlyList<GlobePoint> data, TileGameObject tileGameObject,
            MapRenderer mapRenderer)
        {
            RenderHeightMesh(data, tileGameObject, mapRenderer, Settings.Priority);
        }

        /// <summary>
        /// Builds the tile mesh from the given GlobePoints and sets the mesh accordingly on the given TileGameObject
        /// </summary>
        /// <param name="points">The points to generate the Mesh from</param>
        /// <param name="tileGameObject">The TileGameObject to set the mesh to</param>
        /// <param name="mapRenderer">The MapRenderer the tile belongs to</param>
        /// <param name="priority">The Priority of the Mesh</param>
        public static void RenderHeightMesh(IReadOnlyList<GlobePoint> points, TileGameObject tileGameObject,
            MapRenderer mapRenderer, int priority)
        {
            var midpoint = mapRenderer.ViewProjection.GlobePointToPosition(GlobePoint.MidPoint(points[0], points[^1]));
            midpoint.y = 0;
            var vertices = points.Select(point =>
                (mapRenderer.ViewProjection.GlobePointToPosition(point) - midpoint).ToVector3()).ToArray();
            tileGameObject.SetMesh(MeshBuilder.BuildMesh(vertices), priority);
        }

        /// <inheritdoc/>
        protected override async Task<IReadOnlyList<GlobePoint>> RequestDataInternal(
            (TileId tileId, GlobeArea globeArea) request, CancellationToken token)
        {
            var globePoints = request.globeArea.GetPointGrid(_settings.MeshResolution);

            //create web request
            var json = JsonUtility.ToJson(new OtdRequest
            {
                locations = GetLocationString(globePoints),
                interpolation = _settings.Interpolation.ToString().ToLower()
            });

            //send web request
            //using string.empty for the requestUri as the base address is set in client
            var response =
                await _client.PostAsync(string.Empty, new StringContent(json, Encoding.UTF8, "application/json"),
                    token);
            response.EnsureSuccessStatusCode();

            //convert json response to object
            var text = await response.Content.ReadAsStringAsync();
            var result = JsonUtility.FromJson<OtdResponse>(text);

            //overwrite globePoint elevation
            for (var i = 0; i < result.results.Count; i++)
            {
                globePoints[i].Altitude = result.results[i].elevation;
            }

            return globePoints;
        }

        /// <summary>
        /// Generates a string of the format lat,lon|lat,lon|... for requesting <see cref="GlobePoint"/>s.
        /// </summary>
        /// <param name="globePoints"><see cref="GlobePoint"/>s to generate the string from</param>
        /// <returns>The list of <see cref="GlobePoint"/>s represented as a string</returns>
        private string GetLocationString(IReadOnlyList<GlobePoint> globePoints)
        {
            var result = new StringBuilder();
            for (var i = 0; i < globePoints.Count; i++)
            {
                result.Append(GetLocationString(globePoints[i]));
                if (i < globePoints.Count - 1)
                {
                    result.Append('|');
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Generates the location string for a single <see cref="GlobePoint"/>.
        /// </summary>
        /// <param name="globePoint"><see cref="GlobePoint"/> to get the location string for</param>
        /// <returns><see cref="GlobePoint"/> location represented as a string</returns>
        private string GetLocationString(GlobePoint globePoint)
        {
            StringBuilder builder = new();
            builder.Append(globePoint.Latitude.ToString(CultureInfo.InvariantCulture))
                .Append(',')
                .Append(globePoint.Longitude.ToString(CultureInfo.InvariantCulture));

            return builder.ToString();
        }

        #region JsonClasses

        //these classes are needed to convert objects to a json request and back
        //therefore they cannot follow naming conventions

        [Serializable]
        private record OtdRequest
        {
            public string locations;
            public string interpolation;
        }

        [Serializable]
        private class OtdResponse
        {
            public List<OtdResponsePoint> results;
            public string status;
        }

        [Serializable]
        private class OtdResponsePoint
        {
            public string dataset;
            public double elevation;
            public OtdResponseLocation location;
        }

        [Serializable]
        private class OtdResponseLocation
        {
            public double lat;
            public double lng;
        }

        #endregion JsonClasses
    }

    public enum OtdInterpolation
    {
        Nearest,
        Linear,
        Cubic
    }
}