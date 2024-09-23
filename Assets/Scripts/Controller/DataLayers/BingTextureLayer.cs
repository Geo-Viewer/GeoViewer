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
    /// A texture layer for requesting and rendering textures based on Bing Format (QuadKey)
    /// </summary>
    public class BingTextureLayer : DataLayer<BingTextureLayerSettings, Texture2D>, ITextureLayer
    {
        public const string QuadKeyIdentifier = "quadkey";

        public SegmentationSettings SegmentationSettings => _settings.SegmentationSettings;

        /// <summary>
        /// Stores a client for requesting the data.
        /// </summary>
        private readonly HttpClient _client = HttpClientFactory.CreateOsmClient();

        /// <summary>
        /// Creates a new instance of the <see cref="OsmTextureLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="OsmTextureLayer"/></param>
        public BingTextureLayer(BingTextureLayerSettings settings) : base(settings)
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
                QuadKeyIdentifier => request.tileId.ToQuadKey(),
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