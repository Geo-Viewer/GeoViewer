using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Util;
using GeoViewer.Model.State;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Color = UnityEngine.Color;

namespace GeoViewer.View.UI
{
    /// <summary>
    /// The map settings are displayed in the lower right corner of the application.
    /// They provide settings to change how the map is displayed.
    /// </summary>
    public class MapSettings : UIElement
    {
        private Button _3dButton;
        private Button _centerButton;
        private Button _gizmoButton;
        private VisualElement _buttonImage;
        private readonly StyleColor _black = new Color(0.106f, 0.106f, 0.106f, 1f);
        private readonly StyleColor _grey = new Color(0.6f, 0.6f, 0.6f, 1);

        private void OnEnable()
        {
            var root = GetRoot();
            _centerButton = root.Q("Center-Button") as Button;
            _gizmoButton = root.Q("Gizmo-Button") as Button;
            _3dButton = root.Q("3D-Button") as Button;
            _buttonImage = root.Q("3D-Icon");

            //actions which take place when one of the buttons is pressed
            _centerButton!.clicked += () => GetController().ResetCamera();
            _gizmoButton!.clicked += () => GetController().ToggleRotationCenter();
            _3dButton!.clicked += () => GetController().ToggleRenderHeightData();
            ApplicationState.Instance.LayerManager.CurrentLayerChanged += (_) => SetCorrect3DButtonMode(ApplicationState.Instance.LayerManager.GetLayersActive(typeof(IMeshLayer)));

            ApplicationState.Instance.RotationCenterVisibilityChangedEvent +=
                (sender, args) => SetCorrectGizmoMode(args.RotationCenterVisible);
        }

        private void SetCorrectGizmoMode(bool argsRotationCenterVisible)
        {
            if (argsRotationCenterVisible)
            {
                if (_gizmoButton.ClassListContains("deactivated-button"))
                {
                    _gizmoButton.RemoveFromClassList("deactivated-button");
                }

                _gizmoButton.AddToClassList("activated-map-button");
            }
            else
            {
                if (_gizmoButton.ClassListContains("activated-button"))
                {
                    _gizmoButton.RemoveFromClassList("activated-button");
                }

                _gizmoButton.AddToClassList("deactivated-button");
            }
        }

        private void SetCorrect3DButtonMode(bool renderHeightData)
        {
            if (renderHeightData)
            {
                if (_3dButton.ClassListContains("deactivated-button"))
                {
                    _3dButton.RemoveFromClassList("deactivated-button");
                }

                _3dButton.AddToClassList("activated-button");
                _buttonImage.style.unityBackgroundImageTintColor = _black;
            }
            else
            {
                if (_3dButton.ClassListContains("activated-button"))
                {
                    _3dButton.RemoveFromClassList("activated-button");
                }

                _3dButton.AddToClassList("deactivated-button");
                _buttonImage.style.unityBackgroundImageTintColor = _grey;
            }
        }
    }
}