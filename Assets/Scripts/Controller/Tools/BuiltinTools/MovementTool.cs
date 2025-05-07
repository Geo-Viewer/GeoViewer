using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Commands;
using GeoViewer.Controller.Input;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Mode;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = System.Diagnostics.Debug;

namespace GeoViewer.Controller.Tools.BuiltinTools
{
    /// <summary>
    /// A tool which moves selected objects around.
    /// The shift key can be pressed to allow for fine-grained adjustments.
    /// If the alt key is pressed, the objects are moved vertically, otherwise they are moved horizontally.
    /// </summary>
    public class MovementTool : Tool
    {
        private readonly Dictionary<SceneObject, float3> _startPositions = new();
        private float2 _cursorStartPosition = float2.zero;

        /// <summary>
        /// Indicates that the user is moving objects
        /// </summary>
        private bool _moving;

        /// <summary>
        /// Create a new tool for moving selected objects.
        /// </summary>
        /// <param name="inputs">An <see cref="Inputs"/> instance which is used to retrieve user input.</param>
        public MovementTool(Inputs inputs) : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override ToolMode Mode { get; } = new ToolMode.Builder()
            .WithFeature(ApplicationFeature.HoldPrimary)
            .WithFeature(ApplicationFeature.HoldAlt)
            .WithFeature(ApplicationFeature.HoldShift)
            .Build();

        /// <inheritdoc/>
        public override ToolData Data { get; } = new(
            Resources.Load<VectorImage>("Tools/Move"),
            "Move objects",
            "Move selected objects by dragging the mouse",
            Color.white,
            4000
        );

        /// <inheritdoc/>
        protected override void OnActivate()
        {
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            FinishMoving();
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            if (!Inputs.PrimaryHeld || !Inputs.ViewSpaceMousePosition.HasValue || Camera == null)
            {
                FinishMoving();
                return;
            }

            if (!_moving)
            {
                StartMoving();
            }

            // get the current relative cursor position
            var cursorOffset = Inputs.ViewSpaceMousePosition.Value - _cursorStartPosition;

            var viewportMovement = GetCenterViewCoordinates(Camera) + new float2(cursorOffset.x, cursorOffset.y);
            var ray = Camera.ViewportPointToRay((Vector2)viewportMovement);

            // calculate how much the selected objects should be moved
            var offset = Inputs.AltHeld ? CalculateVerticalOffset(ray) : CalculateHorizontalOffset(ray);

            // if shift is held, we cut the moved distance in half for fine grained movements.
            if (Inputs.ShiftHeld)
            {
                offset /= 2;
            }

            foreach (var (gameObject, pos) in _startPositions)
            {
                gameObject.transform.position = pos + offset;
            }
        }

        /// <summary>
        /// Calculates how much the selection should be raised vertically if the alt key is pressed
        /// </summary>
        /// <param name="ray">A ray which points from the camera to the position where the selection center should be moved.</param>
        private float3 CalculateVerticalOffset(Ray ray)
        {
            var origin = (float3)ray.origin;
            var direction = (float3)ray.direction;

            // Camera isn't null as the update method checks that for us
            var plane = new Plane(Camera!.transform.forward, SelectionCenter);
            plane.Raycast(ray, out var t);

            var newPosition = origin + t * direction;
            return new float3(SelectionCenter.x, newPosition.y, SelectionCenter.z) - SelectionCenter;
        }

        /// <summary>
        /// Calculates how much the selection should be raised horizontally
        /// </summary>
        /// <param name="ray">A ray which points from the camera to the position where the selection center should be moved.</param>
        private float3 CalculateHorizontalOffset(Ray ray)
        {
            float3 origin = ray.origin;
            float3 direction = ray.direction;
            if (direction.y == 0)
            {
                direction.y -= 0.1f;
            }

            var t = (SelectionCenter.y - origin.y) / direction.y;

            var newPosition = origin + t * direction;

            return new float3(newPosition.x, SelectionCenter.y, newPosition.z) - SelectionCenter;
        }

        private void FinishMoving()
        {
            if (!_moving)
            {
                return;
            }

            _moving = false;

            if (_startPositions.Count == 0)
            {
                return;
            }

            // recompute the center as we changed the position of the objects
            ComputeCenter();

            //add command
            var (key, value) = _startPositions.First();
            ApplicationState.Instance.CommandHandler.AddWithoutExecute(
                new TransformSelected(ApplicationState.Instance.SelectedObjects.Select((x) => x.transform),
                    key.transform.position - (Vector3)value));
        }

        private void StartMoving()
        {
            _moving = true;

            // set start cursor position

            // we know this because we checked it in update()
            Debug.Assert(Inputs.ViewSpaceMousePosition != null, "Inputs.ViesSpaceMousePosition != null");
            _cursorStartPosition = Inputs.ViewSpaceMousePosition.Value;

            // set up cached data which will be used to display the current state based on the offset until the operation is completed.
            _startPositions.Clear();
            foreach (var selected in ApplicationState.Instance.SelectedObjects)
            {
                _startPositions[selected] = selected.transform.position;
            }
        }
    }
}