using System;
using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class OsmTextureLayerSettings : TextureLayerSettings
    {
        public override string Type { get; } = "OsmTexture";

        /// <summary>
        /// The url to the tile server. Has to contain {zoom}, {x} and {y}
        /// </summary>
        public string Url { get; set; } = "https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";

        public OsmTextureLayerSettings()
        {
            RequestsPerSecond = 20;
        }

        public override IDataLayer CreateDataLayer()
        {
            return new OsmTextureLayer(this);
        }

        public override bool Validate()
        {
            return base.Validate()
                   && Url.Contains(OsmTextureLayer.ZoomIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(OsmTextureLayer.XCordIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(OsmTextureLayer.YCordIdentifier, StringComparison.OrdinalIgnoreCase);
        }
    }
}