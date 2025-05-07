using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Controller.Map;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A Mesh Layer for creating a basic flat mesh, with no height data
    /// </summary>
    public class BaseMeshLayer : DataLayer<BaseMeshLayerSettings, IReadOnlyList<GlobePoint>>, IMeshLayer
    {
        /// <summary>
        /// Creates a new Instance of the <see cref="BaseMeshLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="BaseMeshLayer"/></param>
        public BaseMeshLayer(BaseMeshLayerSettings settings) : base(settings)
        {
        }

        /// <inheritdoc/>
        protected override void RenderDataInternal(IReadOnlyList<GlobePoint> data, TileGameObject tileGameObject,
            MapRenderer mapRenderer)
        {
            var midpoint =
                mapRenderer.ApplicationPositionToWorldPosition(
                    mapRenderer.GlobePointToApplicationPosition(GlobePoint.MidPoint(data[0], data[^1])));
            var vertices = data.Select(point =>
                mapRenderer.ApplicationPositionToWorldPosition(mapRenderer.GlobePointToApplicationPosition(point)) -
                midpoint).ToArray();
            tileGameObject.SetMesh(MeshBuilder.BuildMesh(vertices), _settings.Priority);
        }

        /// <inheritdoc/>
        protected override Task<IReadOnlyList<GlobePoint>> RequestDataInternal(
            (TileId tileId, GlobeArea globeArea) request, CancellationToken token)
        {
            return Task.FromResult(request.globeArea.GetPointGrid(_settings.MeshResolution));
        }
    }
}