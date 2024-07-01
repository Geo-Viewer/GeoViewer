using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI
{
    /// <summary>
    /// The welcome screen is displayed upon starting the application.
    /// It contains useful links as well as build version information.
    /// </summary>
    public class WelcomeScreen : UIElement
    {
        private VisualElement _instance;
        private VisualElement _background;
        private Button _close;
        private VisualElement _sections;
        private Label _build;
        private Label _version;
        private const string versionDisplay = "Version: ";
        private const string buildDisplay = "Build: ";
        private List<VisualElement> _listSections = new();

        private void Awake()
        {
            _instance = GetRoot().Q("background");
            _close = _instance.Q("Close") as Button;
            _sections = _instance.Q("Sections");
            _build = _instance.Q("Build") as Label;
            _version = _instance.Q("Version") as Label;
        }

        private void Start()
        {
            //close button for welcome-screen
            _close!.clicked += Close;

            //set up welcome-screen
            AddSection("Links");
            AddSection("Credits");
            AddEntry(0, "Manual", OpenManual);
            AddEntry(1, "Openstreetmap.org", () => OpenLink("https://www.openstreetmap.org/copyright"));
            AddEntry(1, "Unity URP Outlines", () => OpenLink("https://github.com/Robinseibold/Unity-URP-Outlines"));
            AddEntry(1, "Runtime OBJ Importer",
                () => OpenLink("https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547"));
            AddEntry(1, "Standalone File Browser",
                () => OpenLink("https://github.com/gkngkc/UnityStandaloneFileBrowser"));
            SetVersionInformation(versionDisplay + Application.version, buildDisplay + Application.buildGUID);
        }

        /// <summary>
        /// Adds a new section to the welcome screen.
        /// </summary>
        /// <param name="name">The title to be displayed at the top of the section.</param>
        /// <returns>A unique identifier used to add elements to the created section.</returns>
        public int AddSection(string name)
        {
            //create section elements
            var section = new VisualElement();
            var label = new Label(name);

            //styling
            section.AddToClassList("section-container");
            label.AddToClassList("section-heading");
            label.AddToClassList("section-item");
            section.Add(label);
            _sections.Add(section);
            _listSections.Add(section);
            return _listSections.Count;
        }

        /// <summary>
        /// Adds a new entry to the section with the given identifier.
        /// An entry is a clickable link which executes an action when clicked.
        /// </summary>
        /// <param name="section">The identifier of the section where the entry should be added.</param>
        /// <param name="text">The text the entry should display.</param>
        /// <param name="onClick">The action which should be executed when the user clicks on the entry.</param>
        public void AddEntry(int section, string text, Action onClick)
        {
            var entry = new Button(onClick)
            {
                text = text
            };
            entry.AddToClassList("welcome-button");
            _listSections[section].Add(entry);
        }

        private void OpenManual()
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(Application.dataPath, "Manual.pdf"))
                {
                    UseShellExecute = true
                }
            }.Start();
        }

        private void OpenLink(string link)
        {
            Application.OpenURL(link);
        }

        /// <summary>
        /// Sets the version information displayed in the lower right corner of the welcome screen.
        /// </summary>
        /// <param name="version">The application version</param>
        /// <param name="build">The build guid, used to uniquely identify builds.</param>
        private void SetVersionInformation(string version, string build)
        {
            _version.text = version;
            _build.text = build;
        }

        /// <summary>
        /// Opens the welcome screen.
        /// </summary>
        public void Open()
        {
            _instance.visible = true;
        }

        /// <summary>
        /// Closes the welcome screen.
        /// </summary>
        public void Close()
        {
            _instance.visible = false;
        }
    }
}