using UnityEngine;
using UnityEngine.UIElements;

namespace GeoViewer.Model.Tools
{
    /// <summary>
    /// The class contains the displayed data of the tools.
    /// </summary>
    public class ToolData
    {
        /// <summary>
        /// The icon which is displayed in the tool bar.
        /// </summary>
        public VectorImage Icon { get; }

        /// <summary>
        /// The name of the tool.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A short description of the tool's usage.
        /// </summary>
        public string ShortDescription { get; }

        /// <summary>
        /// The color of the tool in the tool bar.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// The priority of this view element.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Create new tool data with the given values
        /// </summary>
        /// <param name="icon">The icon which is displayed in the tool bar.</param>
        /// <param name="name">The name of the tool.</param>
        /// <param name="shortDescription">A short description of what the tool does.</param>
        /// <param name="color">The colour of the tool.</param>
        /// <param name="priority">The priority of the tool.</param>
        public ToolData(VectorImage icon, string name, string shortDescription, Color color, int priority)
        {
            Icon = icon;
            Name = name;
            ShortDescription = shortDescription;
            Color = color;
            Priority = priority;
        }
    }
}