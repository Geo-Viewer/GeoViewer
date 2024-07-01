using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.InformationBox
{
    /// <summary>
    /// The information box displays relevant information to the user based on the current context.
    /// </summary>
    public class InformationBox : UIElement
    {
        private VisualElement _informationbox;
        private int _count;
        private readonly Dictionary<int, VisualElement> _content = new();

        private void Awake()
        {
            var root = GetRoot();
            _informationbox = root.Q("informationbox");
            _count = 0;
        }

        /// <summary>
        /// Adds a new element to the information box and displays it.
        /// </summary>
        /// <param name="element">The element which is added.</param>
        /// <returns>A unique identifier which can be used to access or remove the element from the box.</returns>
        public int AddElement(VisualElement element)
        {
            _informationbox.Add(element);
            element.visible = true;
            _content.Add(_count + 1, element);
            _count++;
            return _count;
        }

        /// <summary>
        /// Returns the element in this box with the given id, or <c>null</c> if there isn't any.
        /// </summary>
        /// <param name="id">The unique identifier of the object which should be returned.</param>
        /// <returns>the element in this box with the given id, or <c>null</c> if there isn't any.</returns>
        public VisualElement? GetElement(int id)
        {
            if (id <= _count)
            {
                return _content[id];
            }

            return null;
        }

        /// <summary>
        /// Removes the element with the given id from this box.
        /// </summary>
        /// <param name="id">The unique identifier of the object which should be removed.</param>
        /// <returns><c>true</c> if an element with the given id was found and removed from the box, <c>false</c> otherwise.</returns>
        public bool RemoveElement(int id)
        {
            if (_count >= id && _informationbox.Contains(_content[id]))
            {
                _informationbox.Remove(_content[id]);
                _content.Remove(id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the visibility of the information box.
        /// </summary>
        /// <param name="visible">True to make the information box visible, false to hide it</param>
        public void SetVisible(bool visible = true)
        {
            _informationbox.visible = visible;
            foreach (var child in _informationbox.Children())
            {
                child.visible = visible;
            }
        }
    }
}