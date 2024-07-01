using GeoViewer.Controller.ObjLoading;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI
{
    /// <summary>
    /// A class managing the loading Screen displayed when loading a model.
    /// </summary>
    public class LoadingScreen : UIElement
    {
        private VisualElement _instance;
        private VisualElement _bar;

        private void Awake()
        {
            _instance = GetRoot().Q("Background");
            _bar = _instance.Q("BarFill");
            Close();
        }

        /// <summary>
        /// Opens the loading Screen.
        /// </summary>
        public void Open()
        {
            _instance.visible = true;
            _bar.style.width = new StyleLength(new Length(0, LengthUnit.Percent));
        }

        /// <summary>
        /// Closes the loading screen.
        /// </summary>
        public void Close()
        {
            _instance.visible = false;
        }

        /// <summary>
        /// Updates the style. This cannot be done via event callbacks, because it has to be executed on the main thread
        /// </summary>
        private void Update()
        {
            _bar.style.width = new StyleLength(new Length(ObjLoader.Progress * 100, LengthUnit.Percent));
        }
    }
}