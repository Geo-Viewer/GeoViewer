using GeoViewer.Controller.Commands;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Util;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Mode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace GeoViewer.Controller.Tools.BuiltinTools
{
    /// <summary>
    /// A tool which selects objects when clicking on them.
    /// </summary>
    public class SelectionTool : Tool
    {
        /// <summary>
        /// The name of the layer containing selectable objects.
        /// </summary>
        public const string SelectableLayer = "Selectable";

        /// <summary>
        /// The name of the layer containing selected objects (should be outlined).
        /// </summary>
        public const string SelectedLayer = "Selected";

        private int _selectableLayerId;
        private int _selectedLayerId;

        /// <summary>
        /// Create a new tool for selecting and deselecting objects.
        /// </summary>
        /// <param name="inputs">An <see cref="Inputs"/> instance which is used to retrieve user input.</param>
        public SelectionTool(Inputs inputs) : base(inputs)
        {
        }

        /// <inheritdoc/>
        public override ToolMode Mode { get; } = new ToolMode.Builder()
            .WithFeature(ApplicationFeature.ClickPrimary)
            .Build();

        /// <inheritdoc/>
        public override ToolData Data { get; } = new(
            Resources.Load<VectorImage>("Tools/Select"),
            "Select objects",
            "Select objects by clicking on them",
            Color.white,
            5000
        );

        /// <inheritdoc/>
        protected override void OnActivate()
        {
            Inputs.PrimaryClicked += TrySelect;

            _selectedLayerId = LayerMask.NameToLayer(SelectedLayer);
            _selectableLayerId = LayerMask.NameToLayer(SelectableLayer);
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            Inputs.PrimaryClicked -= TrySelect;
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
        }

        /// <summary>
        /// Tries to select an Object by sending a raycast from the camera and finding a hit object on the selectable layer.
        /// </summary>
        private void TrySelect()
        {
            // If the click was aborted or didn't finish, we don't select anything.
            if (EventSystem.current.IsPointerOverGameObject()
                || !Inputs.MousePosition.HasValue
                || Camera == null)
            {
                return;
            }

            if (RaycastUtil.GetCursorRaycastHit(_selectableLayerId, out var hit))
            {
                // if the hit object isn't selected, we select it
                if (hit.transform.parent.TryGetComponent(out SceneObject sceneObject))
                    ApplicationState.Instance.CommandHandler.Execute(new SelectObject(sceneObject, hit.transform.gameObject));
            }
            else if (RaycastUtil.GetCursorRaycastHit(_selectedLayerId, out hit))
            {
                // if the hit object is selected, we deselect it
                if (hit.transform.parent.TryGetComponent(out SceneObject sceneObject))
                    ApplicationState.Instance.CommandHandler.Execute(new DeselectObject(sceneObject, hit.transform.gameObject));
            }
        }
    }
}