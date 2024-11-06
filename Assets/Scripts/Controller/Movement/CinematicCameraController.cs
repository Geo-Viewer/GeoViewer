using System;
using GeoViewer.Model.State;
using GeoViewer.View.UI.InformationBox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace GeoViewer.Controller.Movement
{
    [RequireComponent(typeof(CameraController))]
    public class CinematicCameraController : MonoBehaviour
    {
        [SerializeField] Key toggleCameraControl = Key.T;
        [SerializeField] Key toggleZoom = Key.Z;
        [SerializeField] Key toggleRotation = Key.R;

        [SerializeField] private Key increaseZoomSpeed = Key.I;
        [SerializeField] private Key decreaseZoomSpeed = Key.U;
        [SerializeField] private float ZoomStepAmount = 0.2f;

        [SerializeField] private Key increaseHorizontalRotationSpeed = Key.E;
        [SerializeField] private Key decreaseHorizontalRotationSpeed = Key.W;
        [SerializeField] private Key increaseVerticalRotationSpeed = Key.D;
        [SerializeField] private Key decreaseVerticalRotationSpeed = Key.S;
        [SerializeField] private float RotationStepAmount = 0.2f;

        [SerializeField] private Key disableCurrentTextureLayer = Key.N;
        [SerializeField] private Key reactivateAllLayers = Key.B;
        [SerializeField] private Key callMapUpdate = Key.M;
        [SerializeField] private Key toggleUI = Key.V;


        private CameraController _controller;
        private CameraInputs _inputs = new();
        private bool _controlEnabled = false;


        private void Start()
        {
            _controller = GetComponent<CameraController>();
            _inputs.PrimaryHeld = true;
        }

        private void Update()
        {
            if (Keyboard.current[toggleCameraControl].wasPressedThisFrame)
            {
                _controlEnabled = !_controlEnabled;
            }

            if (Keyboard.current[toggleRotation].wasPressedThisFrame)
            {
                _inputs.MouseDelta = Vector2.zero;
            }

            if (Keyboard.current[toggleZoom].wasPressedThisFrame)
            {
                _inputs.ScrollWheel = 0;
            }

            if (Keyboard.current[increaseZoomSpeed].wasPressedThisFrame)
            {
                _inputs.ScrollWheel += ZoomStepAmount;
            }

            if (Keyboard.current[decreaseZoomSpeed].wasPressedThisFrame)
            {
                _inputs.ScrollWheel -= ZoomStepAmount;
            }

            if (Keyboard.current[increaseHorizontalRotationSpeed].wasPressedThisFrame)
            {
                _inputs.MouseDelta -= new Vector2(RotationStepAmount, 0);
            }

            if (Keyboard.current[decreaseHorizontalRotationSpeed].wasPressedThisFrame)
            {
                _inputs.MouseDelta += new Vector2(RotationStepAmount, 0);
            }

            if (Keyboard.current[increaseVerticalRotationSpeed].wasPressedThisFrame)
            {
                _inputs.MouseDelta += new Vector2(0, RotationStepAmount);
            }

            if (Keyboard.current[decreaseVerticalRotationSpeed].wasPressedThisFrame)
            {
                _inputs.MouseDelta -= new Vector2(0, RotationStepAmount);
            }

            if (Keyboard.current[disableCurrentTextureLayer].wasPressedThisFrame)
            {
                ApplicationState.Instance.LayerManager._textureLayers.Current.SetActive(false);
            }

            if (Keyboard.current[reactivateAllLayers].wasPressedThisFrame)
            {
                foreach (var layer in ApplicationState.Instance.LayerManager._textureLayers.GetAllAddedLayers())
                {
                    layer.SetActive(true);
                }
            }

            if (Keyboard.current[callMapUpdate].wasPressedThisFrame)
            {
                ApplicationState.Instance.MapRenderer.UpdateMap();
            }
            
            if (Keyboard.current[toggleUI].wasPressedThisFrame)
            {
                var ui = GameObject.Find("UI");
                if (ui.TryGetComponent(out UIDocument doc))
                {
                    doc.rootVisualElement.visible = !doc.rootVisualElement.visible;
                }

                if (ui.TryGetComponent(out InformationBox ib))
                {
                    ib.SetVisible(doc.rootVisualElement.visible);
                }
            }
        }

        private void LateUpdate()
        {
            if (!_controlEnabled) return;
            _inputs.MouseDelta *= Time.deltaTime;
            _inputs.ScrollWheel *= Time.deltaTime;
            _controller.UpdateCam(Time.deltaTime, _inputs);
            _inputs.MouseDelta /= Time.deltaTime;
            _inputs.ScrollWheel /= Time.deltaTime;
        }
    }
}