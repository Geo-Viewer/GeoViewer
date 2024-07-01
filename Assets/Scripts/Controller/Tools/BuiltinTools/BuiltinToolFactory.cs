using GeoViewer.Controller.Input;

namespace GeoViewer.Controller.Tools.BuiltinTools
{
    /// <summary>
    /// A factory class for creating the tools which are shipped with the application.
    /// </summary>
    public class BuiltinToolFactory
    {
        /// <summary>
        /// Create a selection tool.
        /// The tool is able to select interact-able objects.
        /// </summary>
        /// <param name="inputs">The inputs instance which is used to create the tool.</param>
        /// <returns>A new selection tool instance.</returns>
        public virtual Tool SelectionTool(Inputs inputs)
        {
            return new SelectionTool(inputs);
        }

        /// <summary>
        /// Create a movement tool.
        /// The tool is able to move selected objects.
        /// </summary>
        /// <param name="inputs">The inputs instance which is used to create the tool.</param>
        /// <returns>A new movement tool instance.</returns>
        public virtual Tool MovementTool(Inputs inputs)
        {
            return new MovementTool(inputs);
        }

        /// <summary>
        /// Create a rotation tool.
        /// The tool is able to rotate selected objects.
        /// </summary>
        /// <param name="inputs">The inputs instance which is used to create the tool.</param>
        /// <returns>A new rotation tool instance.</returns>
        public virtual Tool RotationTool(Inputs inputs)
        {
            return new RotationTool(inputs);
        }

        /// <summary>
        /// Create a scaling tool.
        /// The tool is able to scale selected objects.
        /// </summary>
        /// <param name="inputs">The inputs instance which is used to create the tool.</param>
        /// <returns>A new scale tool instance.</returns>
        public virtual Tool ScaleTool(Inputs inputs)
        {
            return new ScaleTool(inputs);
        }

        /// <summary>
        /// Create a tool for measuring distances.
        /// The tool is able to measure real-world distances in the scene.
        /// </summary>
        /// <param name="inputs">The inputs instance which is used to create the tool.</param>
        /// <returns>A new distance tool instance.</returns>
        public virtual Tool DistanceTool(Inputs inputs)
        {
            return new DistanceTool(inputs);
        }
    }
}