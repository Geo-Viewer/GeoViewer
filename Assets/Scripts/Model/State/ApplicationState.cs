using System;
using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Commands;
using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Files;
using GeoViewer.Controller.Input;
using GeoViewer.Model.State.Events;
using GeoViewer.Model.Tools.Mode;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Model.State
{
    /// <summary>
    /// The class contains information about the different states of the application.
    /// </summary>
    public class ApplicationState
    {
        #region Singleton

        private static ApplicationState? _instance;

        /// <summary>
        /// The singleton instance of this ApplicationState.
        /// </summary>
        public static ApplicationState Instance => _instance ??= new ApplicationState();

        /// <summary>
        /// The current layer manager of the application
        /// </summary>
        public LayerManager LayerManager { get; private set; }

        /// <summary>
        /// The current map renderer of the application
        /// </summary>
        public MapRenderer MapRenderer { get; private set; }

        private bool _rotationCenterVisible = true;

        private ApplicationState()
        {
            Settings = ConfigLoader.GetSettingsFromConfig();
            LayerManager = new LayerManager(Settings.DataLayers);
            MapRenderer = new MapRenderer(LayerManager);
        }

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
        protected virtual void OnRotationCenterVisibilityChangedEvent(RotationCenterVisibilityChangedEventArgs args)
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
        /// A reference to the rotation center game object.
        /// </summary>
        public GameObject? RotationCenter { get; set; }

        /// <summary>
        /// An event which is raised when the rotation center changes its visibility.
        /// </summary>
        public event EventHandler<RotationCenterVisibilityChangedEventArgs>? RotationCenterVisibilityChangedEvent;

        #endregion RotationCenter

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

        private readonly HashSet<GameObject> _selectedObjects = new();

        /// <summary>
        /// An enumerable containing all currently selected objects.
        /// The enumerable automatically filters destroyed objects.
        /// </summary>
        public IEnumerable<GameObject> SelectedObjects
        {
            get { return _selectedObjects.Where(obj => obj != null); }
        }

        /// <summary>
        /// Adds an object to the set of selected objects.
        /// This also removes any already destroyed selected objects.
        /// </summary>
        /// <param name="gameObject">the object which is selected</param>
        public void AddSelectedObject(GameObject gameObject)
        {
            _selectedObjects.Add(gameObject);

            foreach (var obj in _selectedObjects.Where(obj => obj == null).ToArray())
            {
                _selectedObjects.Remove(obj);
            }
        }

        /// <summary>
        /// Removes an object from the set of selected objects.
        /// </summary>
        /// <param name="gameObject">the object which is deselected</param>
        public void RemoveSelectedObject(GameObject gameObject)
        {
            _selectedObjects.Remove(gameObject);
        }

        /// <summary>
        /// Resets the <see cref="ToolMode"/> to a default tool mode which doesn't contain any values.
        /// </summary>
        public void ClearToolMode()
        {
            _mode = null;
        }

        #endregion Tools

        /// <summary>
        /// A reference to the active camera.
        /// If null, no camera is rendering at the moment.
        /// </summary>
        public Camera? Camera { get; set; }

        /// <summary>
        /// User settings loaded from the json file stored in the application's data directory.
        /// </summary>
        public ApplicationSettings Settings { get; }

        public Inputs Inputs { get; } = new(new InputManager());

        public CommandHandler CommandHandler { get; } = new();
    }
}