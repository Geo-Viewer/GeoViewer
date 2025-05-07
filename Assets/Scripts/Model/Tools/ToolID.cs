using GeoViewer.Controller.Tools;

namespace GeoViewer.Model.Tools
{
    /// <summary>
    /// An identifier which is used to connect a tool to the registry where it is registered.
    /// </summary>
    public record ToolID
    {
        /// <summary>
        /// The unique id of a tool.
        /// This is primarily used by the registry and can be ignored.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// The actual tool this tool id references.
        /// </summary>
        public Tool Tool { get; }

        /// <summary>
        /// The registry where the tool is registered.
        /// </summary>
        public ToolRegistry Registry { get; }

        /// <summary>
        /// Create a new tool id which references the given tool as registered under the given internal id in
        /// the registry.
        /// </summary>
        /// <param name="tool">The tool referenced by the id.</param>
        /// <param name="id">The internal id used by the registry.</param>
        /// <param name="registry">The registry where the tool is registered.</param>
        public ToolID(Tool tool, int id, ToolRegistry registry)
        {
            ID = id;
            Tool = tool;
            Registry = registry;
        }
    }
}