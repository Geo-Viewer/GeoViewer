using System;
using GeoViewer.Model.State;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GeoViewer.Controller.Input
{
    /// <summary>
    /// Contains pressed user inputs.
    /// The class bundles multiple inputs into vectors making it easy to use them in arithmetic operations.
    /// </summary>
#if UNITY_EDITOR
    public class Inputs : InputManager.IEditorActions
#else
    public class Inputs : InputManager.IReleaseActions
#endif
    {
        /// <summary>
        /// An input manager to read user inputs from.
        /// </summary>
        public InputManager Manager { get; }

        /// <summary>
        /// The mouse movement relative to the last frame.
        /// The x-axis represents left/right movement.
        /// The y-axis represents up/down movement.
        /// </summary>
        public Vector2 MouseDelta { get; private set; }

        /// <summary>
        /// The current keyboard movement inputs.
        /// These are by default bound to WASD as well as Q and E.
        /// The x-axis represents left/right movement and is bound to A/D.
        /// The y-axis represents up/down movement and is bound to E/Q.
        /// The z-axis represents forward/backward movement and is bound to W/S.
        /// </summary>
        public Vector3 KeyBoardMovement { get; private set; }

        /// <summary>
        /// True if the left mouse button is currently pressed.
        /// </summary>
        public bool PrimaryHeld { get; private set; }

        /// <summary>
        /// Gets called if the left mouse button was pressed.
        /// </summary>
        public Action? PrimaryClicked;

        /// <summary>
        /// True if the left alt key is currently pressed.
        /// </summary>
        public bool AltHeld { get; private set; }

        /// <summary>
        /// True if the left shift key is currently pressed.
        /// </summary>
        public bool ShiftHeld { get; private set; }

        /// <summary>
        /// True if the right mouse button is currently pressed.
        /// </summary>
        public bool SecondaryHeld { get; private set; }

        /// <summary>
        /// Gets called if the right mouse button was pressed.
        /// </summary>
        public Action? SecondaryClicked;

        /// <summary>
        /// The current scroll wheel input.
        /// </summary>
        public float ScrollWheel { get; private set; }

        /// <summary>
        /// The current window space position of the mouse.
        /// </summary>
        public Vector2? MousePosition => Mouse.current?.position.ReadValue();

        /// <summary>
        /// The current view space position of the mouse.
        /// </summary>
        public float2? ViewSpaceMousePosition
        {
            get
            {
                if (MousePosition.HasValue)
                {
                    return (float2)MousePosition.Value / new float2(Screen.width, Screen.height);
                }

                return null;
            }
        }

        /// <summary>
        /// Creates a new Inputs instance.
        /// </summary>
        /// <param name="manager">The input manager to read user inputs from.</param>
        public Inputs(InputManager manager)
        {
            Manager = manager;
            Manager.Enable();
#if UNITY_EDITOR
            Manager.Editor.SetCallbacks(this);
#else
            Manager.Release.SetCallbacks(this);
#endif
        }

        /// <summary>
        /// Updates the value of MouseDelta.
        /// Called if the mouseMove-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnMouseMove(InputAction.CallbackContext context)
        {
            MouseDelta = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Updates the value of AltHeld.
        /// Called if the holdAlt-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnHoldAlt(InputAction.CallbackContext context)
        {
            AltHeld = context.started || context.performed;
        }

        /// <summary>
        /// Updates the value of ShiftHeld.
        /// Called if the holdShift-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnHoldShift(InputAction.CallbackContext context)
        {
            ShiftHeld = context.started || context.performed;
        }

        /// <summary>
        /// Updates the value of PrimaryHeld.
        /// Called if the holdLeft-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnHoldLeft(InputAction.CallbackContext context)
        {
            PrimaryHeld = context.performed && !EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Updates the value of SecondaryHeld.
        /// Called if the holdRight-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnHoldRight(InputAction.CallbackContext context)
        {
            SecondaryHeld = context.performed && !EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Updates the value of KeyboardMovement.
        /// Called if the keyboardMove-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnKeyboardMove(InputAction.CallbackContext context)
        {
            KeyBoardMovement = context.ReadValue<Vector3>();
        }

        /// <summary>
        /// Updates the value of ScrollWheel.
        /// Called if the mouseZoom-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnZoom(InputAction.CallbackContext context)
        {
            ScrollWheel = context.ReadValue<float>();
        }

        /// <summary>
        /// Called if the clickLeft-action changes its status.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnClickLeft(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PrimaryClicked?.Invoke();
            }
        }

        /// <summary>
        /// Called if the clickRight-action changes its value.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnClickRight(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SecondaryClicked?.Invoke();
            }
        }

        /// <summary>
        /// Called if the Undo-action changes its value.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnUndo(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                ApplicationState.Instance.CommandHandler.Undo();
            }
        }

        /// <summary>
        /// Called if the Redo-action changes its value.
        /// </summary>
        /// <param name="context">Information provided to action callbacks about what triggered an action.</param>
        public void OnRedo(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                ApplicationState.Instance.CommandHandler.Redo();
            }
        }
    }
}