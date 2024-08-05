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
        public const int SettingsVersion = 5;

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
        public float MapSizeMultiplier { get; set; } = 3f;

        /// <summary>
        /// The minimal size of the displayed map in metres
        /// </summary>
        public float MinMapSize { get; set; } = 500f;

        /// <summary>
        /// Whether Frustum Culling for map tiles should be used
        /// </summary>
        public bool EnableTileCulling { get; set; } = true;

        /// <summary>
        /// The strength of the frustum culling
        /// </summary>
        public float CullingAngle { get; set; } = 90f;

        /// <summary>
        /// Fov of the main camera
        /// </summary>
        public float CameraFov { get; set; } = 60;

        #region Graphics

        /// <summary>
        /// Whether distance fog should be used to hide map edge
        /// </summary>
        public bool EnableDistanceFog { get; set; } = true;

        /// <summary>
        /// Whether Post Processing should be enabled
        /// </summary>
        public bool EnablePostProcessing { get; set; } = true;

        /// <summary>
        /// Whether VSync should be enabled
        /// </summary>
        public bool EnableVSync { get; set; } = true;

        /// <summary>
        /// The framerate target for the Application
        /// </summary>
        public int TargetFrameRate { get; set; } = 120;

        #endregion Graphics

        /// <summary>
        /// A list of all data layers
        /// </summary>
        public List<DataLayerSettings> DataLayers { get; set; } = new() { new OsmTextureLayerSettings() };
    }
}