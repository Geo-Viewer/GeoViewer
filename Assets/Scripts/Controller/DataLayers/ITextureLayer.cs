using GeoViewer.Model.DataLayers.Settings;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// An interface for a texture layer
    /// </summary>
    public interface ITextureLayer : IDataLayer, IDataRequest<Texture2D>
    {
        /// <summary>
        /// Returns the segmentation settings of this texture layer.
        /// </summary>
        /// <returns>The segmentation settings of this texture layer</returns>
        public SegmentationSettings SegmentationSettings { get; }
    }
}