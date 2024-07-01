using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.Commands;
using GeoViewer.Controller.Input;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Mode;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Debug = System.Diagnostics.Debug;

namespace GeoViewer.Controller.Tools.BuiltinTools
{
    /// <summary>
    /// A tool which scales selected objects.
    /// The shift key can be pressed to allow for fine-grained adjustments.
    /// </summary>
    public class ScaleTool : Tool
    {
        private const float ScalingStrength = 4f;

        private readonly Dictionary<GameObject, ObjectData> _startData = new();

        /// <summary>
        /// The relative distance from the cursor to the center of the screen when the user initiates a scaling process
        /// </summary>
        private float _cursorStartDistance;

        private bool _scaling;

        /// <summary>
        /// Create a new tool for scaling selected objects.
        /// </summary>
        /// <param name="inputs">An <see cref="Inputs"/> instance which is used to retrieve user input.</param>
        public ScaleTool(Inputs inputs) : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override ToolMode Mode { get; } = new ToolMode.Builder()
            .WithFeature(ApplicationFeature.HoldPrimary)
            .WithFeature(ApplicationFeature.HoldShift)
            .Build();

        /// <inheritdoc/>
        public override ToolData Data { get; } = new(
            Resources.Load<VectorImage>("Tools/Scale"),
            "Scale objects",
            "Scale selected objects by dragging the mouse",
            Color.white,
            2000
        );

        /// <inheritdoc/>
        protected override void OnActivate()
        {
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            StopScaling();
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            if (!Inputs.PrimaryHeld || !Inputs.ViewSpaceMousePosition.HasValue)
            {
                StopScaling();
                return;
            }

            if (_scaling)
            {
                UpdateScaling();
                return;
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                StartScaling();
            }
        }

        private void UpdateScaling()
        {
            var offsetLength = DistanceToSelectionCenter(Inputs.ViewSpaceMousePosition!.Value);

            // if the user presses shift, the strength is cut in half
            var strength = ScalingStrength;
            if (Inputs.ShiftHeld)
            {
                strength /= 2;
            }

            // To calculate how much the selection is scaled,
            // we take the distance the cursor was moved relative to the center of the screen.
            // We multiply that by the scaling strength and add 1 so that the initial scale isn't zero.
            // We also need to ensure that the scale isn't negative.
            var scalingFactor = math.max(strength * (offsetLength - _cursorStartDistance) + 1, 0);

            foreach (var selected in ApplicationState.Instance.SelectedObjects)
            {
                var data = _startData[selected];

                selected.transform.localScale = scalingFactor * data.Scale;
                // move the objects away from the origin to simulate that the whole selection is scaled equally
                selected.transform.position = data.Position + (scalingFactor - 1) * data.SelectionCenterOffset;
            }
        }

        private void StopScaling()
        {
            if (!_scaling)
            {
                return;
            }

            _scaling = false;

            ApplicationState.Instance.CommandHandler.AddWithoutExecute(new TransformSelected(
                ApplicationState.Instance.SelectedObjects.Select((x) => (x.transform,
                    x.transform.position - (Vector3)_startData[x].Position,
                    Quaternion.identity,
                    x.transform.localScale - (Vector3)_startData[x].Scale)).ToArray()));
        }

        private void StartScaling()
        {
            _scaling = true;

            // Calculate the distance from the cursor to the selection center in view space
            // we checked for null in update()
            Debug.Assert(Inputs.ViewSpaceMousePosition != null, "Inputs.ViewSpaceMousePosition != null");
            _cursorStartDistance = DistanceToSelectionCenter(Inputs.ViewSpaceMousePosition.Value);

            // set up cached data which will be used to display the current state based on the offset until the operation is completed.
            _startData.Clear();
            foreach (var selected in ApplicationState.Instance.SelectedObjects)
            {
                var transform = selected.transform;
                var position = transform.position;
                _startData[selected] = new ObjectData(
                    transform.localScale,
                    position,
                    (float3)position - SelectionCenter
                );
            }
        }

        /// <summary>
        /// Calculates the distance of the given relative mouse position to the selection center.
        /// </summary>
        /// <param name="position">
        /// A camera view position.
        /// The lower left corner is located at (0,0), the upper right at (1,1).
        /// </param>
        /// <returns>The distance to the selection center.</returns>
        private float DistanceToSelectionCenter(float2 position)
        {
            // camera nullability is checked in caller
            return math.length(GetCenterViewCoordinates(Camera!) - position);
        }

        /// <summary>
        /// Caches data of selected objects at the start of an operation
        /// </summary>
        /// <param name="Scale">The scale of the object relative to its parent</param>
        /// <param name="Position">the position of the object in world space</param>
        /// <param name="SelectionCenterOffset">the offset of the object from the selection center</param>
        private record ObjectData(float3 Scale, float3 Position, float3 SelectionCenterOffset)
        {
            public float3 Scale { get; } = Scale;
            public float3 Position { get; } = Position;

            /// <summary>
            /// SelectionCenter - Position.
            /// </summary>
            public float3 SelectionCenterOffset { get; } = SelectionCenterOffset;
        }
    }
}