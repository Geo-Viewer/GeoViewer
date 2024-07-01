using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A Texture Layer for rendering a basic texture
    /// </summary>
    public class BaseTextureLayer : DataLayer<BaseTextureLayerSettings, Texture2D>, ITextureLayer
    {
        private Texture2D? _baseTexture;

        public SegmentationSettings SegmentationSettings => Settings.SegmentationSettings;

        /// <summary>
        /// Creates a new Instance of the <see cref="BaseTextureLayer"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="BaseTextureLayer"/></param>
        public BaseTextureLayer(BaseTextureLayerSettings settings) : base(settings)
        {
        }

        /// <inheritdoc/>
        public override void RenderData(Texture2D data, TileGameObject tileGameObject, MapRenderer mapRenderer)
        {
            tileGameObject.SetTexture(data, Priority);
        }

        /// <inheritdoc/>
        protected override Task<Texture2D> RequestDataInternal((TileId tileId, GlobeArea globeArea) request,
            CancellationToken token)
        {
            if (_baseTexture == null)
            {
                InitializeBaseTexture();
            }

            return Task.FromResult(_baseTexture)!;
        }

        /// <summary>
        /// Loads the base texture from the file system.
        /// </summary>
        private void InitializeBaseTexture()
        {
            _baseTexture = new Texture2D(1, 1)
            {
                name = "TileBaseTexture",
                filterMode = Settings.FilterMode
            };
            _baseTexture.LoadImage(File.ReadAllBytes(Settings.TexturePath));
        }
    }
}