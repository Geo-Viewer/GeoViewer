using System;
using GeoViewer.Controller.Input;
using GeoViewer.Model.Globe;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools.Mode;
using SimpleFileBrowser;
using UnityEngine;

namespace GeoViewer.Controller.Movement
{
    /// <summary>
    /// The camera controller handles the movement of the camera.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public bool UserControlled { get; set; } = true;
        private GameObject? RotationCenter => ApplicationState.Instance.RotationCenter;

        private const float ZoomSpeedModifier = 0.0008f;

        private const float MoveSpeedModifier = 0.8f;

        private const float MouseSpeedModifier = 0.002f;

        private const float RotationSpeedModifier = 0.1f;

        private const float MinPitchX = -90f;

        private const float MaxPitchX = 90f;

        private const float RotationZ = 0f;

        private float _rotationX;

        private float _rotationY;

        private readonly CameraInputs _inputs = new ();

        private void Awake()
        {
            ApplicationState.Instance.Camera = GetComponent<Camera>();
            ApplicationState.Instance.Camera.fieldOfView = ApplicationState.Instance.Settings.CameraFov;
        }

        private void Update()
        {
            if (FileBrowser.IsOpen || !UserControlled) return;
            _inputs.SetInputs(ApplicationState.Instance.Inputs);
            UpdateCam(Time.deltaTime, _inputs);
        }

        public void UpdateCam(float delta, CameraInputs inputs)
        {
            // if the rotation center isn't set, we can't move the camera
            if (RotationCenter is null) return;

            HandleRotation(inputs.PrimaryHeld, inputs.MouseDelta);
            HandleMovement(delta, inputs.PrimaryHeld, inputs.SecondaryHeld, inputs.MouseDelta, inputs.KeyboardMovement);
            HandleZoom(inputs.ScrollWheel);
        }

        private void Start()
        {
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

        private void HandleRotation(bool primaryHeld, Vector2 mouseDelta)
        {
            if (!ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldPrimary) || !primaryHeld) return;

            var mouseMovementX = mouseDelta.x * RotationSpeedModifier;
            var mouseMovementY = mouseDelta.y * RotationSpeedModifier;

            _rotationX -= mouseMovementY;
            _rotationY += mouseMovementX;

            _rotationX = Mathf.Clamp(_rotationX, MinPitchX, MaxPitchX);

            ApplicationState.Instance.RotationCenter!.transform.localEulerAngles =
                new Vector3(_rotationX, _rotationY, RotationZ);
        }

        private void HandleMovement(float delta, bool primaryHeld, bool secondaryHeld, Vector2 mouseDelta, Vector3 keyboardMovement)
        {
            var distanceToRotationCenter = Vector3.Distance(transform.position,
                RotationCenter!.transform.position);

            var movementInput = Vector3.zero;

            if (ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldSecondary) && secondaryHeld)
            {
                movementInput = -mouseDelta * MouseSpeedModifier;
            }
            else if (ApplicationState.Instance.ToolMode.CanAppUse(ApplicationFeature.HoldPrimary) &&
                     primaryHeld)
            {
                movementInput = keyboardMovement * delta;
            }

            var movement = distanceToRotationCenter * MoveSpeedModifier * movementInput;
            RotationCenter!.transform.Translate(movement, transform);
        }

        private void HandleZoom(float scrollWheel)
        {
            var zoomAmount = -scrollWheel * ZoomSpeedModifier;

            RotationCenter!.transform.localScale *= Math.Clamp(1 + zoomAmount, 0.01f, 2);
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

    public class CameraInputs
    {
        public bool PrimaryHeld { get; set; }
        public bool SecondaryHeld { get; set; }
        public Vector2 MouseDelta { get; set; }
        public Vector3 KeyboardMovement { get; set; }
        public float ScrollWheel { get; set; }

        public void SetInputs(Inputs inputs)
        {
            PrimaryHeld = inputs.PrimaryHeld;
            SecondaryHeld = inputs.SecondaryHeld;
            MouseDelta = inputs.MouseDelta;
            KeyboardMovement = inputs.KeyBoardMovement;
            ScrollWheel = inputs.ScrollWheel;
        }
    }
}