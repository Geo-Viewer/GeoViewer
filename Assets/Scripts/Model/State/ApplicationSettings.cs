using System.Collections.Generic;
using GeoViewer.Model.DataLayers.Settings;

namespace GeoViewer.Model.State
{
    /// <summary>
    /// A data class storing all settings declared in the config file
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// The settings version.
        /// </summary>
        public const int SettingsVersion = 3;

        /// <summary>
        /// If this doesn't match the settings version, we create a backup of it and reset the config.
        /// </summary>
        public int ConfigVersion { get; set; } = SettingsVersion;

        /// <summary>
        /// The factor for multiplying the resolution of the terrain 
        /// </summary>
        public int ResolutionMultiplier { get; set; } = 1;

        /// <summary>
        /// The factor to multiply the camera distance with, to get the radius of the request
        /// </summary>
        public float RequestRadiusMultiplier { get; set; } = 12f;

        /// <summary>
        /// A list of all data layers
        /// </summary>
        public List<DataLayerSettings> DataLayers { get; set; } = new() { new OsmTextureLayerSettings() };
    }
}