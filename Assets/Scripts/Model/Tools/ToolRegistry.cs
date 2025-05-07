using System;
using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Tools;
using GeoViewer.Model.Tools.Events;

namespace GeoViewer.Model.Tools
{
    /// <summary>
    /// The tool registry is used to collect all available <see cref="Tool"/>s.
    /// </summary>
    public class ToolRegistry
    {
        private readonly List<ToolID> _ids = new();
        private readonly HashSet<ToolID> _idSet = new();
        private readonly HashSet<Tool> _toolsSet = new();

        private ToolID? _activeTool;

        /// <summary>
        /// Contains the currently active <see cref="Tool"/>.
        /// This property may only be assigned tools which are registered in this tool registry.
        /// Otherwise an <see cref="ArgumentException"/> is thrown.
        /// When the assigned tool is different from the currently active tool, an <see cref="ActiveToolChangedEvent"/> is raised.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when the property is assigned a tool which isn't registered in this tool registry.
        /// </exception>
        public ToolID? ActiveTool
        {
            get => _activeTool;
            private set
            {
                // the nullability suppression is needed here as the hashset doesn't have proper nullability annotations yet and relies
                // on incompatible jetbrains annotations
                if (!_idSet.Contains(value!))
                {
                    throw new ArgumentException("The given tool id does not belong to this registry.");
                }

                // we only raise the event if the new tool is different from the old one.
                if (_activeTool?.ID != value?.ID)
                {
                    ActiveToolChangedEvent?.Invoke(this, new ActiveToolChangedEventArgs(_activeTool, value));
                    _activeTool = value;
                }
            }
        }

        /// <summary>
        /// An event which is raised when the active tool is changed to a different one.
        /// </summary>
        public event EventHandler<ActiveToolChangedEventArgs>? ActiveToolChangedEvent;

        /// <summary>
        /// Registers the given <see cref="Tool"/> .
        /// The registration fails if the tool is registered in the registry already.
        /// </summary>
        /// <param name="tool">The tool which is to be registered.</param>
        /// <exception cref="ArgumentException">Thrown if the tool is registered already.</exception>
        /// <returns>The id of the registered tool.</returns>
        public ToolID RegisterTool(Tool tool)
        {
            // Check fail condition
            if (!_toolsSet.Add(tool))
            {
                throw new ArgumentException($"The given tool '{tool.Data.Name}' is registered already.");
            }

            var id = new ToolID(tool, _ids.Count, this);

            _ids.Add(id);
            _idSet.Add(id);
            return id;
        }

        /// <summary>
        /// Tries to set the active tool to the one with the given id.
        /// This fails if no tool was registered with that id.
        /// </summary>
        /// <param name="id">The unique identifier of the tool.</param>
        /// <returns>true if the tool was activated successfully, false otherwise.</returns>
        public bool TrySetActiveTool(ToolID? id)
        {
            if (id is null || !_idSet.Contains(id))
            {
                return false;
            }

            ActiveTool = id;
            return true;
        }

        /// <summary>
        /// Returns a list of all registered tools' IDs.
        /// </summary>
        public IEnumerable<ToolID> GetTools()
        {
            return _ids;
        }

        /// <summary>
        /// Returns a list of all registered tools' IDs, sorted by their priority in descending order.
        /// </summary>
        public IEnumerable<ToolID> GetSortedTools()
        {
            return _ids.OrderBy(id => -id.Tool.Data.Priority);
        }
    }
}