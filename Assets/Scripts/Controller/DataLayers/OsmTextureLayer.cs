using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    /// A texture layer for requesting and rendering textures based on Osm-format
    /// </summary>
    public class OsmTextureLayer : DataLayer<OsmTextureLayerSettings, Texture2D>, ITextureLayer
    {
        public const string ZoomIdentifier = "zoom";
        public const string XCordIdentifier = "x";
        public const string YCordIdentifier = "y";

        public SegmentationSettings SegmentationSettings => _settings.SegmentationSettings;

        /// <summary>
        /// Stores a client for requesting the data.
        /// </summary>
        private readonly HttpClient _client = HttpClientFactory.CreateOsmClient();

        /// <summary>
        /// Creates a new instance of the <see cref="OsmTextureLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="OsmTextureLayer"/></param>
        public OsmTextureLayer(OsmTextureLayerSettings settings) : base(settings)
        {
        }

        /// <inheritdoc/>
        protected override void RenderDataInternal(Texture2D data, TileGameObject tileGameObject, MapRenderer mapRenderer)
        {
            tileGameObject.SetTexture(data, _settings.Priority);
        }

        /// <inheritdoc/>
        protected override async Task<Texture2D> RequestDataInternal((TileId tileId, GlobeArea globeArea) request,
            CancellationToken token)
        {
            var url = StringFormatter.FormatString(_settings.Url, tag => tag.ToString().ToLower() switch
            {
                ZoomIdentifier => request.tileId.Zoom.ToString(),
                XCordIdentifier => request.tileId.Coordinates.x,
                YCordIdentifier => request.tileId.Coordinates.y,
                _ => null
            });

            var response = await _client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();

            var texture = new Texture2D(1, 1)
            {
                name = request.tileId.ToString(),
                wrapMode = TextureWrapMode.Clamp,
                filterMode = _settings.FilterMode
            };
            texture.LoadImage(await response.Content.ReadAsByteArrayAsync());

            return texture;
        }
    }
}