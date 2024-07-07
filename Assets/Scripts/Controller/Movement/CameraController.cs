using System;
using GeoViewer.Controller.Input;
using GeoViewer.Model.Globe;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools.Mode;
using UnityEngine;

namespace GeoViewer.Controller.Movement
{
    /// <summary>
    /// The camera controller handles the movement of the camera.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        // initialized in the start method
        private Inputs _inputs = null!;

        private const float ZoomSpeedModifier = 0.0008f;

        private const float MoveSpeedModifier = 0.8f;

        private const float MouseSpeedModifier = 0.002f;

        private const float RotationSpeedModifier = 0.1f;

        private const float MinPitchX = -90f;

        private const float MaxPitchX = 90f;

        private const float RotationZ = 0f;

        private float _rotationX;

        private float _rotationY;

        private void Update()
        {
            // if the rotation center isn't set, we can't move the camera
            var rotationCenter = ApplicationState.Instance.RotationCenter;
            if (rotationCenter is null)
            {
                return;
            }

            var rotationCenterPosition = rotationCenter.transform.position;

            var distanceToRotationCenter = Vector3.Distance(transform.position,
                rotationCenterPosition);

            if (ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldPrimary) && _inputs.PrimaryHeld)
            {
                HandleRotation();
            }

            HandleMovement(rotationCenter.transform, distanceToRotationCenter);
            HandleZoom();
        }

        private void Start()
        {
            ApplicationState.Instance.Camera = GetComponent<Camera>();

            _inputs = ApplicationState.Instance.Inputs;

            // set the initial camera transform
            var cameraTransform = transform;

            var localEulerAngles = cameraTransform.localEulerAngles;
            _rotationX = localEulerAngles.x;
            _rotationY = localEulerAngles.y;
        }

        private void OnDestroy()
        {
            ApplicationState.Instance.Camera = null;
        }

        private void HandleRotation()
        {
            var mouseMovementX = _inputs.MouseDelta.x * RotationSpeedModifier;
            var mouseMovementY = _inputs.MouseDelta.y * RotationSpeedModifier;

            _rotationX -= mouseMovementY;
            _rotationY += mouseMovementX;

            _rotationX = Mathf.Clamp(_rotationX, MinPitchX, MaxPitchX);

            ApplicationState.Instance.RotationCenter!.transform.localEulerAngles =
                new Vector3(_rotationX, _rotationY, RotationZ);
        }

        private void HandleMovement(Transform rotationCenter, float distance)
        {
            var movementInput = Vector3.zero;

            if (ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldSecondary) && _inputs.SecondaryHeld)
            {
                movementInput = -_inputs.MouseDelta * MouseSpeedModifier;
            }
            else if (ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldPrimary) &&
                     _inputs.PrimaryHeld)
            {
                movementInput = _inputs.KeyBoardMovement * Time.deltaTime;
            }

            var movement = distance * MoveSpeedModifier * movementInput;
            rotationCenter.Translate(movement, transform);
        }

        private void HandleZoom()
        {
            var zoomAmount = -_inputs.ScrollWheel * ZoomSpeedModifier;

            ApplicationState.Instance.RotationCenter!.transform.localScale *= Math.Clamp(1 + zoomAmount, 0.01f, 2);
        }

        /// <summary>
        /// Resets the position of the rotation center back to (0, 0, 0).
        /// This does not change the camera's rotation or zoom.
        /// </summary>
        public void SetPosition(Vector3 position, bool resetZoom = true)
        {
            // we can't reset the position without the rotation center
            if (ApplicationState.Instance.RotationCenter == null)
            {
                return;
            }
            ApplicationState.Instance.RotationCenter.transform.position = position;

            if (resetZoom)
            {
                ApplicationState.Instance.RotationCenter.transform.localScale = Vector3.one;
            }
        }
    }
}