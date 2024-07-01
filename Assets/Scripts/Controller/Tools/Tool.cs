using GeoViewer.Controller.Input;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Mode;
using Unity.Mathematics;
using UnityEngine;

namespace GeoViewer.Controller.Tools
{
    /// <summary>
    /// An abstract class containing methods needed for implementing the functionality of a tool.
    /// A tool provides a way for users to interact with loaded objects and the environment.
    /// </summary>
    public abstract class Tool
    {
        /// <summary>
        /// The center point of all selected objects.
        /// To save on computation time, this property is not updated automatically,
        /// but can be updated manually with <see cref="ComputeCenter"/>
        /// </summary>
        protected float3 SelectionCenter { get; private set; }

        /// <summary>
        /// A tool mode containing all <see cref="ApplicationFeature"/>s reserved by this tool.
        /// </summary>
        public abstract ToolMode Mode { get; }

        /// <summary>
        /// An <see cref="Inputs"/> instance which can be used by active tools to retrieve user input.
        /// </summary>
        protected Inputs Inputs { get; }

        /// <summary>
        /// The camera which renders the viewport.
        /// </summary>
        protected Camera? Camera => ApplicationState.Instance.Camera;

        /// <summary>
        /// Data associated with a tool.
        /// Contains mostly data which is needed to display tool buttons and menu entries.
        /// </summary>
        public abstract ToolData Data { get; }

        /// <summary>
        /// Constructs a new Tool with the given <see cref="Inputs"/> instance.
        /// </summary>
        /// <param name="inputs">An Inputs instance which can be used by the tool to retrieve user input.</param>
        protected Tool(Inputs inputs)
        {
            Inputs = inputs;
        }

        /// <summary>
        /// Activates the tool and sets the <see cref="ToolMode"/> in the <see cref="ApplicationState"/>.
        /// The previously active tool must be disabled before the new tool can be enabled.
        /// </summary>
        public void Activate()
        {
            // Set the tool mode to reserve features
            ApplicationState.Instance.ToolMode = Mode;

            // Update the center of all selected objects for tools to use
            ComputeCenter();

            OnActivate();
        }

        /// <summary>
        /// Disables the tool and unsets the <see cref="ToolMode"/> in the <see cref="ApplicationState"/>.
        /// </summary>
        public void Disable()
        {
            ApplicationState.Instance.ClearToolMode();
            OnDisable();
        }

        /// <summary>
        /// Called when a tool is activated.
        /// This method may only be called on disabled tools.
        /// </summary>
        protected abstract void OnActivate();

        /// <summary>
        /// Called when a tool is disabled.
        /// This method may only be called on active tools.
        /// </summary>
        protected abstract void OnDisable();

        /// <summary>
        /// Called each frame when the tool is active.
        /// Contains the logic for interacting with the environment.
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// Computes the average position of all selected objects
        /// </summary>
        protected void ComputeCenter()
        {
            var sum = float3.zero;
            var count = 0;
            foreach (var vector in ApplicationState.Instance.SelectedObjects)
            {
                // if the game object has a mesh, we calculate the center using the mesh bounds instead.
                // This is a workaround because many of the provided scans have a weird origin.
                if (vector.TryGetComponent(out MeshFilter mesh))
                {
                    sum += (float3)mesh.mesh.bounds.center;
                }

                sum += (float3)vector.transform.position;
                count++;
            }

            if (count > 1)
            {
                sum /= count;
            }

            SelectionCenter = sum;
        }

        /// <summary>
        /// Returns the view coordinates of the selection center.
        /// </summary>
        /// <param name="camera">The camera to which the coordinates should be relative</param>
        protected float2 GetCenterViewCoordinates(Camera camera)
        {
            var point = camera.WorldToViewportPoint(SelectionCenter);
            return new float2(point.x, point.y);
        }
    }
}