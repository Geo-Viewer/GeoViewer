using System.IO;
using GeoViewer.Controller.DataLayers;
using UnityEngine.Device;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class BaseTextureLayerSettings : TextureLayerSettings
    {
        public override string Type { get; } = "BaseTexture";

        /// <summary>
        /// The path to the texture file
        /// </summary>
        public string TexturePath { get; set; } =
            Path.Combine(Application.dataPath, "Data", "Textures", "TileBaseTexture.png");

        public BaseTextureLayerSettings()
        {
            CacheSize = 0;
        }

        public override IDataLayer CreateDataLayer()
        {
            return new BaseTextureLayer(this);
        }

        public override bool Validate()
        {
            return base.Validate() && File.Exists(TexturePath);
        }
    }
}