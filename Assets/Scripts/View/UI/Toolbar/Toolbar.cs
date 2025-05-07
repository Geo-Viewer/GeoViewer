using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Tools;
using GeoViewer.Model.Tools;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Toolbar
{
    /// <summary>
    ///     The toolbar contains the most important tools.
    ///     It displays which tool is active and can be used to change the current tool.
    /// </summary>
    public class Toolbar : UIElement
    {
        private VisualElement _toolbar;
        private readonly Dictionary<ToolID, ToolButton> _listToolButtons = new();
        [SerializeField] private int toolCount = 5;

        private void Start()
        {
            var root = GetRoot();
            _toolbar = root.Q("toolbar");
            DisplayTools();
            SetActiveTool(ToolManager.Instance.Registry.ActiveTool);
            ToolManager.Instance.Registry.ActiveToolChangedEvent += (sender, args) => SetActiveTool(args.ActiveTool);
        }

        private void SetActiveTool(ToolID activeTool)
        {
            foreach (var id in _listToolButtons.Keys)
            {
                _listToolButtons[id].RemoveFromClassList("activated-button");
                _listToolButtons[id].AddToClassList("deactivated-button");
            }

            if (activeTool != null && _listToolButtons.ContainsKey(activeTool))
            {
                _listToolButtons[activeTool].RemoveFromClassList("deactivated-button");
                _listToolButtons[activeTool].AddToClassList("activated-button");
            }
        }

        /// <summary>
        /// Get the highest priority tools from the tool registry and display them.
        /// </summary>
        private void DisplayTools()
        {
            foreach (var toolID in ToolManager.Instance.Registry.GetSortedTools().Take(toolCount))
            {
                var button = new ToolButton(toolID);
                button.AddToClassList("deactivated-button");
                button.AddToClassList("deactivated-button");
                button.AddToClassList("toolbar-item");
                _listToolButtons.Add(toolID, button);
                _toolbar.Add(button);
            }
        }
    }
}