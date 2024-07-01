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
    /// A tool which rotates selected objects around the y-axis.
    /// The shift key can be pressed to allow for fine-grained adjustments.
    /// </summary>
    public class RotationTool : Tool
    {
        private bool _rotating;

        /// <summary>
        /// the direction from the selection center to the cursor at the start of an operation
        /// </summary>
        private float2 _cursorStartDirection = float2.zero;

        private readonly Dictionary<GameObject, ObjectData> _startData = new();

        /// <summary>
        /// Create a new tool for rotating selected objects.
        /// </summary>
        /// <param name="inputs">An <see cref="Inputs"/> instance which is used to retrieve user input.</param>
        public RotationTool(Inputs inputs) : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override ToolMode Mode { get; } = new ToolMode.Builder()
            .WithFeature(ApplicationFeature.HoldPrimary)
            .WithFeature(ApplicationFeature.HoldShift)
            .Build();

        /// <inheritdoc/>
        public override ToolData Data { get; } = new(
            Resources.Load<VectorImage>("Tools/Rotate"),
            "Rotate objects",
            "Rotate selected objects by dragging the mouse",
            Color.white,
            3000
        );

        /// <inheritdoc/>
        protected override void OnActivate()
        {
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            if (!Inputs.PrimaryHeld || !Inputs.MousePosition.HasValue || Camera == null)
            {
                StopRotation();
                return;
            }

            if (_rotating)
            {
                UpdateRotation();
                return;
            }

            if (!EventSystem.current.IsPointerOverGameObject())
            {
                StartRotation();
            }
        }

        private void UpdateRotation()
        {
            var currentPosition = (float2)Inputs.MousePosition!;
            currentPosition.x /= Camera!.pixelWidth;
            currentPosition.y /= Camera.pixelHeight;

            var currentDirection = currentPosition - GetCenterViewCoordinates(Camera);

            var radians = math.atan2(
                _cursorStartDirection.y * currentDirection.x - _cursorStartDirection.x * currentDirection.y,
                math.dot(_cursorStartDirection, currentDirection)
            );

            var rotation = quaternion.RotateY(radians);

            foreach (var selected in ApplicationState.Instance.SelectedObjects)
            {
                var data = _startData[selected];
                selected.transform.localRotation = math.mul(data.Rotation, rotation);
                selected.transform.position = SelectionCenter + math.rotate(rotation, data.OffsetFromCenter);
            }
        }

        private void StopRotation()
        {
            if (!_rotating)
            {
                return;
            }

            _rotating = false;

            ApplicationState.Instance.CommandHandler.AddWithoutExecute(new TransformSelected(
                ApplicationState.Instance.SelectedObjects.Select(
                    (x) => (x.transform,
                        x.transform.position - (Vector3)(SelectionCenter + _startData[x].OffsetFromCenter),
                        x.transform.rotation * Quaternion.Inverse(_startData[x].Rotation),
                        Vector3.zero)).ToArray()));
        }

        private void StartRotation()
        {
            _rotating = true;

            // set start direction
            // view space mouse position and camera nullability are checked in caller
            Debug.Assert(Inputs.ViewSpaceMousePosition != null, "Inputs.ViewSpaceMousePosition != null");
            _cursorStartDirection = Inputs.ViewSpaceMousePosition.Value - GetCenterViewCoordinates(Camera!);

            // set up cached data which will be used to display the current state based on the offset until the operation is completed.
            _startData.Clear();
            foreach (var selected in ApplicationState.Instance.SelectedObjects)
            {
                var transform = selected.transform;
                _startData[selected] = new ObjectData(
                    transform.localRotation,
                    (float3)transform.position - SelectionCenter
                );
            }
        }

        private record ObjectData(Quaternion Rotation, float3 OffsetFromCenter)
        {
            public Quaternion Rotation { get; } = Rotation;
            public float3 OffsetFromCenter { get; } = OffsetFromCenter;
        }
    }
}