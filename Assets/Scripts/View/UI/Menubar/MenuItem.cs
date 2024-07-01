using System.Collections.Generic;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Menubar
{
    /// <summary>
    /// Collection of multiple menu entries which is opened when a menu button is pressed.
    /// </summary>
    public class MenuItem : VisualElement
    {
        /// <summary>
        /// This property contains the name of the menu.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The property is true if the menu is visible, otherwise false.
        /// </summary>
        public bool IsOpen => visible;

        /// <summary>
        /// The constructor creates a new instance which has the given name displays the given entries.
        /// </summary>
        /// <param name="name">the name of the menu</param>
        /// <param name="entries">the entries which should be displayed</param>
        public MenuItem(string name, IEnumerable<MenuEntry> entries)
        {
            Name = name;
            visible = false;
            BuildMenu(entries);
            // Set to absolute position
            style.position = Position.Absolute;
            //style 
            AddToClassList("menu-box");
        }

        /// <summary>
        /// The method sets his own position to the given coordinates.
        /// </summary>
        /// <param name="x">the position on the x-axis at which the menu should be displayed</param>
        /// <param name="y">the position on the y-axis at which the menu should be displayed</param>
        public void SetPosition(float x, float y)
        {
            style.left = x;
            style.top = y;
        }

        /// <summary>
        /// By calling the method the menu becomes invisible.
        /// </summary>
        public void Close()
        {
            visible = false;
        }

        /// <summary>
        /// By calling the method the menu becomes visible.
        /// </summary>
        public void Open()
        {
            visible = true;
        }

        private void BuildMenu(IEnumerable<MenuEntry> entries)
        {
            foreach (var entry in entries)
            {
                Add(entry);
            }
        }
    }
}