using System;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Menubar
{
    /// <summary>
    /// This class represents a button in a menu from the menubar.
    /// </summary>
    public class MenuEntry : Button
    {
        /// <summary>
        /// This constructor creates a new entry which is a button. Its name is displayed on the button.
        /// </summary>
        /// <param name="clickEvent">the action which is called when the button is clicked</param>
        /// <param name="name">the name of the entry</param>
        public MenuEntry(Action clickEvent, string name)
        {
            text = name;
            //style
            AddToClassList("menu-button");
            clicked += () =>
            {
                clickEvent.Invoke();
                Menubar.Instance.CloseAllMenus();
            };
        }
    }
}