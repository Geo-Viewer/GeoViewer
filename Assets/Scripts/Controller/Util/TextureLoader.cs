using UnityEngine;

namespace GeoViewer.Controller.Util
{
    public static class TextureLoader
    {
        public static Texture2D GetTextureFromData(byte[] data, FilterMode filterMode, string name)
        {
            var texture = new Texture2D(1, 1);
            texture.LoadImage(data);

            var mmTexture = new Texture2D(texture.width, texture.height, texture.format, true)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = filterMode
            };
            mmTexture.SetPixelData(texture.GetRawTextureData<byte>(), 0);
            mmTexture.Apply(true, true);

            return mmTexture;
        }
    }
}