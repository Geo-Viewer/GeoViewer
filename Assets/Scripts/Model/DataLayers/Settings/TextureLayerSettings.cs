using GeoViewer.Controller.Map.Projection;
using GeoViewer.Controller.Util;
using UnityEngine;

namespace GeoViewer.Model.DataLayers.Settings
{
    public abstract class TextureLayerSettings : DataLayerSettings
    {
        /// <summary>
        /// The filter mode to use for the texture
        /// </summary>
        public FilterMode FilterMode { get; set; } = FilterMode.Point;

        /// <summary>
        /// The Segmentation settings of this layer
        /// </summary>
        public SegmentationSettings SegmentationSettings { get; set; } = new();

        public override bool Validate()
        {
            return base.Validate() && SegmentationSettings.Validate();
        }
    }

    public class SegmentationSettings
    {
        public ITileProjection Projection { get; set; } = new WebMercatorProjection();
        public Bounds<int> ZoomBounds { get; set; } = new(0, 19);
        public float ResolutionMultiplier { get; set; } = 1;

        public bool Validate()
        {
            return ZoomBounds.Min >= 0 && ResolutionMultiplier > 0;
        }
    }
}