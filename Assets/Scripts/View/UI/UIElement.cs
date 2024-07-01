using GeoViewer.Controller.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI
{
    /// <summary>
    /// Represents a UI element which can send and retrieve data from the controller.
    /// </summary>
    public abstract class UIElement : MonoBehaviour
    {
        /// <summary>
        /// Returns the root element of the UI document attached to the GameObject.
        /// </summary>
        /// <returns>The root visual element of the UI document.</returns>
        protected VisualElement GetRoot()
        {
            return GetComponent<UIDocument>().rootVisualElement;
        }

        /// <summary>
        /// Returns the UI controller responsible for handling ui events.
        /// </summary>
        protected UIController GetController()
        {
            return GetComponent<UIController>();
        }
    }
}