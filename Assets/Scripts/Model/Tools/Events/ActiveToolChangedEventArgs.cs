using System;

namespace GeoViewer.Model.Tools.Events
{
    /// <summary>
    /// Arguments for the active tool changed event.
    /// The event is raised after the active has changed.
    /// </summary>
    public class ActiveToolChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The tool which was active before the event triggered.
        /// </summary>
        public ToolID? DisabledTool { get; }

        /// <summary>
        /// The tool whose activation triggered the event.
        /// </summary>
        public ToolID? ActiveTool { get; }

        /// <summary>
        /// Create new event arguments which contain the activated tool.
        /// </summary>
        /// <param name="disabledTool">The previously active tool.</param>
        /// <param name="activeTool">The freshly activated tool.</param>
        public ActiveToolChangedEventArgs(ToolID? disabledTool, ToolID? activeTool)
        {
            DisabledTool = disabledTool;
            ActiveTool = activeTool;
        }
    }
}