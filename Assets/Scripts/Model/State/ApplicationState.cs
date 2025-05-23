using System;
using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Commands;
using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Files;
using GeoViewer.Controller.Input;
using GeoViewer.Model.State.Events;
using GeoViewer.Model.Tools.Mode;
using GeoViewer.View.FilePicking;
using GeoViewer.View.Rendering;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GeoViewer.Model.State
{
    /// <summary>
    /// The class contains information about the different states of the application.
    /// </summary>
    public sealed class ApplicationState
    {
        #region Singleton

        private static ApplicationState? _instance;

        /// <summary>
        /// The singleton instance of this ApplicationState.
        /// </summary>
        public static ApplicationState Instance => _instance ??= new ApplicationState();

        private ApplicationState()
        {
            Settings = ConfigLoader.GetSettingsFromConfig();
            LayerManager = new LayerManager(Settings.DataLayers);
            MapRenderer = new MapRenderer(LayerManager, Settings);
            ReloadGraphicSettings();
            foreach (var folder in QuickLinks.SpecialFolders)
            {
                var path = Environment.GetFolderPath(folder);
                FileBrowser.AddQuickLink(folder.ToString(), path);
            }
        }

        #endregion Singleton

        #region Rotation Center Visibility

        private bool _rotationCenterVisible = true;

        /// <summary>
        /// Indicates whether the rotation center is visible or invisible.
        /// </summary>
        public bool RotationCenterVisible
        {
            get => _rotationCenterVisible;
            set
            {
                if (value == _rotationCenterVisible)
                {
                    return;
                }

                _rotationCenterVisible = value;
                OnRotationCenterVisibilityChangedEvent(new RotationCenterVisibilityChangedEventArgs(value));
            }
        }

        /// <summary>
        /// Call this method to raise a rotation center visibility changed event,
        /// indicating that the rotation center changed its visibility.
        /// </summary>
        /// <param name="args">The arguments which should be passed to the event.</param>
        private void OnRotationCenterVisibilityChangedEvent(RotationCenterVisibilityChangedEventArgs args)
        {
            RotationCenterVisibilityChangedEvent?.Invoke(this, args);
        }

        /// <summary>
        /// Switches <see cref="RotationCenterVisible"/> from true to false and vice versa.
        /// </summary>
        public void SwitchRotationCenterVisibility()
        {
            RotationCenterVisible = !RotationCenterVisible;
        }

        /// <summary>
        /// An event which is raised when the rotation center changes its visibility.
        /// </summary>
        public event EventHandler<RotationCenterVisibilityChangedEventArgs>? RotationCenterVisibilityChangedEvent;

        #endregion Rotation Center Visibility

        #region Tools

        private ToolMode? _mode;

        /// <summary>
        /// The tool mode containing the features reserved by the active tool.
        /// If no tool is active, returns a <see cref="ToolMode"/> which doesn't reserve any features.
        /// </summary>
        public ToolMode ToolMode
        {
            get => _mode ?? new ToolMode();
            set => _mode = value;
        }

        public readonly HashSet<SceneObject> SceneObjects = new();

        /// <summary>
        /// An enumerable containing all currently selected objects.
        /// The enumerable automatically filters destroyed objects.
        /// </summary>
        public IEnumerable<SceneObject> SelectedObjects
        {
            get { return SceneObjects.Where(obj => obj.IsSelected); }
        }

        /// <summary>
        /// Adds an object to the set of selected objects.
        /// This also removes any already destroyed selected objects.
        /// </summary>
        /// <param name="sceneObject">the object which is selected</param>
        public void AddSceneObject(SceneObject sceneObject)
        {
            SceneObjects.Add(sceneObject);
            sceneObject.OnDestroy += RemoveSelectedObject;
        }

        /// <summary>
        /// Removes an object from the set of selected objects.
        /// </summary>
        /// <param name="sceneObject">the object which is deselected</param>
        public void RemoveSelectedObject(SceneObject sceneObject)
        {
            SceneObjects.Remove(sceneObject);
        }

        /// <summary>
        /// Resets the <see cref="ToolMode"/> to a default tool mode which doesn't contain any values.
        /// </summary>
        public void ClearToolMode()
        {
            _mode = null;
        }

        #endregion Tools

        #region References

        /// <summary>
        /// The current layer manager of the application
        /// </summary>
        public LayerManager LayerManager { get; }

        /// <summary>
        /// The current map renderer of the application
        /// </summary>
        public MapRenderer MapRenderer { get; }

        private Camera? _camera;

        /// <summary>
        /// A reference to the active camera.
        /// If null, no camera is rendering at the moment.
        /// </summary>
        public Camera? Camera
        {
            get => _camera;
            set
            {
                if (value == _camera) return;
                _camera = value;
                OnCameraChanged?.Invoke();
            }
        }

        private GameObject? _rotationCenter;

        /// <summary>
        /// A reference to the rotation center game object.
        /// </summary>
        public GameObject? RotationCenter
        {
            get => _rotationCenter;
            set
            {
                if (value == _rotationCenter) return;
                _rotationCenter = value;
                OnRotationCenterChanged?.Invoke();
            }
        }

        /// <summary>
        /// User settings loaded from the json file stored in the application's data directory.
        /// </summary>
        public ApplicationSettings Settings { get; }

        /// <summary>
        /// A reference to the input handling script
        /// </summary>
        public Inputs Inputs { get; } = new(new InputManager());

        /// <summary>
        /// A reference to the active command handler
        /// </summary>
        public CommandHandler CommandHandler { get; } = new();

        #endregion References

        #region Change Events

        public static event Action? OnRotationCenterChanged;
        public static event Action? OnCameraChanged;

        #endregion Change Events

        #region Graphic Settings

        private Volume? _postProcessVolume;

        private void ReloadGraphicSettings()
        {
            var renderAsset = Resources.Load<UniversalRendererData>("HighFidelity_Fog");
            AdjustFog(renderAsset);

            QualitySettings.vSyncCount = Settings.EnableVSync ? 1 : 0;
            if (!Settings.EnableVSync)
                Application.targetFrameRate = Settings.TargetFrameRate;

            if (_postProcessVolume == null)
            {
                _postProcessVolume = Object.Instantiate(Resources.Load<Volume>("Global Volume"));
            }

            _postProcessVolume.enabled = Settings.EnablePostProcessing;
        }

        private static readonly int FadeStartEnd = Shader.PropertyToID("_Fade_Start_End");
        private static readonly int Fov = Shader.PropertyToID("_Fov");

        private void AdjustFog(UniversalRendererData renderAsset)
        {
            var rendererFeature =
                (FullScreenPassRendererFeature)renderAsset.rendererFeatures.Find(x => x.name == "FullscreenFog");
            rendererFeature.SetActive(Settings.EnableDistanceFog);
            var maxDistance = MapRenderer.TargetCamDistance * Settings.MapSizeMultiplier + MapRenderer.TargetCamDistance / 2;
            rendererFeature.passMaterial.SetVector(FadeStartEnd,
                new Vector4(maxDistance * 2 / 3, maxDistance));

            var horizontalFov =
                Camera.VerticalToHorizontalFieldOfView(Settings.CameraFov, (float)Screen.width / Screen.height);
            rendererFeature.passMaterial.SetFloat(Fov, horizontalFov);
        }

        #endregion Graphic Settings
    }
}