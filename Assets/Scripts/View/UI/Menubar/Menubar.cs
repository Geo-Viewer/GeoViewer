using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Menubar
{
    /// <summary>
    /// The menu bar is displayed at the top of the screen.
    /// It consists of a list of menus, each of which can contain one or more clickable entries.
    /// </summary>
    public class Menubar : UIElement
    {
        private VisualElement _menubar;
        private readonly List<MenuItem> _items = new();
        private bool _started;
        private readonly SortedList<int, (string, IEnumerable<MenuEntry>)> _queuedEntries = new();

        private void Start()
        {
            var root = GetRoot();
            _menubar = root.Q("menubar");
            _started = true;
            AddQueuedMenus(root);
        }

        /// <summary>
        /// Adds all menus which other parts of the application tried to add before this component was ready.
        /// </summary>
        /// <param name="root">the element at which the menus are added</param>
        private void AddQueuedMenus(VisualElement root)
        {
            foreach (var (_, (name, entries)) in _queuedEntries.Reverse())
            {
                var item = new MenuItem(name, entries);

                var button = new MenuButton(this, item);
                _menubar.Add(button);

                root.Add(item);
                _items.Add(item);
            }
        }

        /// <summary>
        /// The method adds a new menu to the menubar with the given priority and name containing the given entries.
        /// If the method is called after the application is started it throws an exception.
        /// The priority declares where the menu should be placed in the menubar.
        /// </summary>
        /// <param name="name">the name of the menu</param>
        /// <param name="entries">the entries of the new menu</param>
        /// <param name="priority">the priority of the menu</param>
        /// <exception cref="InvalidOperationException">an exception which is thrown when the method is called after the
        /// application started</exception>
        public void AddMenu(string name, IEnumerable<MenuEntry> entries, int priority)
        {
            if (_started)
            {
                throw new InvalidOperationException(" You may not add any new menus after " +
                                                    "the application has started");
            }

            _queuedEntries.Add(priority, (name, entries));
        }

        /// <summary>
        /// This method closes all menus of the menubar.
        /// </summary>
        public void CloseAllMenus()
        {
            foreach (var item in _items)
            {
                item.Close();
            }
        }

        #region Singleton

        /// <summary>
        /// The only instance of the menubar.
        /// </summary>
        public static Menubar Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        #endregion Singleton
    }
}