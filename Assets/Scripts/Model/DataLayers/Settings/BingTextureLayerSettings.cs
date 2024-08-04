using System;
using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class BingTextureLayerSettings : TextureLayerSettings
    {
        public override string Type { get; } = "BingTexture";

        /// <summary>
        /// The url to the tile server. Has to contain {quadkey}
        /// </summary>
        public string Url { get; set; } = "https://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?mapArea={quadkey}&key=";

        public BingTextureLayerSettings()
        {
            RequestsPerSecond = 20;
        }

        public override IDataLayer CreateDataLayer()
        {
            return new BingTextureLayer(this);
        }

        public override bool Validate()
        {
            return base.Validate()
                   && Url.Contains(BingTextureLayer.QuadKeyIdentifier, StringComparison.OrdinalIgnoreCase);
        }
    }
}