using System.Collections.Generic;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Tools.BuiltinTools;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Events;
using GeoViewer.View.UI.Menubar;
using UnityEngine;

namespace GeoViewer.Controller.Tools
{
    /// <summary>
    /// The tool manager manages a set of tools, up to one of which can be active at a certain time.
    /// </summary>
    public class ToolManager : MonoBehaviour
    {
        // Value is initialized in Awake()
        /// <summary>
        /// An inputs instance which is passed to tools created by this manager.
        /// The tools can use it to retrieve user input.
        /// </summary>
        private Inputs _inputs = null!;

        // Value is initialized in Awake()
        /// <summary>
        /// The default tool.
        /// This tool is activated when the application starts.
        /// </summary>
        public ToolID DefaultTool { get; private set; } = null!;

        /// <summary>
        /// The tool registry used to register tools which are managed by this manager.
        /// </summary>
        public ToolRegistry Registry { get; } = new();

        /// <summary>
        /// Resets the active tool to the default one as returned by <see cref="DefaultTool"/>
        /// </summary>
        public void ResetActiveTool()
        {
            Registry.TrySetActiveTool(DefaultTool);
        }

        private void Awake()
        {
            SetupSingleton();

            _inputs = ApplicationState.Instance.Inputs;

            RegisterTools();
        }

        private void Start()
        {
            AddToolsToMenubar();

            ResetActiveTool();
        }

        private void Update()
        {
            Registry.ActiveTool?.Tool.OnUpdate();
        }

        private void OnEnable()
        {
            Registry.ActiveToolChangedEvent += OnActiveToolChanged;
        }

        private void OnDisable()
        {
            Registry.ActiveToolChangedEvent -= OnActiveToolChanged;
        }

        /// <summary>
        /// Registers the built-in tools and sets the default tool.
        /// If this method is overriden, one should consider calling this base method to register the built-in tools.
        /// If this isn't done, the default tool <b>must</b> be set by any class which overrides this method.
        /// </summary>
        protected virtual void RegisterTools()
        {
            var factory = new BuiltinToolFactory();
            DefaultTool = Registry.RegisterTool(factory.SelectionTool(_inputs));
            Registry.RegisterTool(factory.MovementTool(_inputs));
            Registry.RegisterTool(factory.RotationTool(_inputs));
            Registry.RegisterTool(factory.ScaleTool(_inputs));
            Registry.RegisterTool(factory.DistanceTool(_inputs));
        }

        /// <summary>
        /// Event handler which activates a new tool when it is assigned active in the tool registry of this manager.
        /// </summary>
        /// <param name="sender">The instance which raised the event</param>
        /// <param name="args">The arguments passed to the event handler</param>
        private void OnActiveToolChanged(object sender, ActiveToolChangedEventArgs args)
        {
            args.DisabledTool?.Tool.Disable();
            args.ActiveTool?.Tool.Activate();
        }

        private void AddToolsToMenubar()
        {
            var toolButtons = new List<MenuEntry>();
            foreach (var tool in Registry.GetTools())
            {
                toolButtons.Add(new MenuEntry(() => Registry.TrySetActiveTool(tool), tool.Tool.Data.Name));
            }

            Menubar.Instance.AddMenu("Tools", toolButtons, 2000);
        }

        #region Singleton

        // value is initialized in Awake()
        /// <summary>
        /// The singleton instance of this <see cref="ToolManager"/>.
        /// </summary>
        public static ToolManager Instance { get; private set; } = null!;

        private void SetupSingleton()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        #endregion Singleton
    }
}