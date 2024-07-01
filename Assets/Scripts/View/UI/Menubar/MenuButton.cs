using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Menubar
{
    /// <summary>
    /// A button in the menu bar. When clicked it toggles an associated menu with entries.
    /// </summary>
    public class MenuButton : Button
    {
        private readonly Menubar _parent;
        private readonly MenuItem _item;

        /// <summary>
        /// Creates a new menu button which is located in the given menu bar.
        /// </summary>
        /// <param name="parent">the bar where the button is displayed</param>
        /// <param name="item">The <see cref="MenuItem"/> which should be toggled once the button is pressed</param>
        public MenuButton(Menubar parent, MenuItem item)
        {
            _parent = parent;
            _item = item;
            text = item.Name;
            clicked += ToggleMenu;
            //style
            AddToClassList("menu-button");
        }

        /// <summary>
        /// Opens the menu this button is associated with and closes all other menus to prevent overlap
        /// if the menu isn't already opened.
        /// If the menu is already opened this method closes all menus.
        /// </summary>
        private void ToggleMenu()
        {
            if (_item.visible)
            {
                _parent.CloseAllMenus();
            }
            else
            {
                _parent.CloseAllMenus();
                _item.SetPosition(layout.position.x, layout.height);
                _item.Open();
            }
        }
    }
}