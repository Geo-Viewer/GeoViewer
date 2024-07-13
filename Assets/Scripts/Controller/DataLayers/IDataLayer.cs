using System;
using GeoViewer.Model.DataLayers.Settings;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// An interface containing basic behaviours for all data layers
    /// </summary>
    public interface IDataLayer
    {
        /// <summary>
        /// Returns whether this data layer is active
        /// </summary>
        /// <returns><c>true</c> if this data layer is active, <c>false</c> otherwise</returns>
        public bool Active { get; }

        /// <summary>
        /// The base settings of this layer
        /// </summary>
        public DataLayerSettings Settings { get; }

        /// <summary>
        /// Clears the cache of this data layer
        /// </summary>
        public void ClearCache();

        /// <summary>
        /// Sets the active state of this data layer
        /// </summary>
        /// <param name="active">Whether the layer should be active</param>
        public void SetActive(bool active);

        /// <summary>
        /// Called, when the layer changes it's active value
        /// </summary>
        public event Action<IDataLayer> ActiveChanged;

        /// <summary>
        /// The Analytics for this DataLayer
        /// </summary>
        public DataLayerAnalytics Analytics { get; }
    }
}