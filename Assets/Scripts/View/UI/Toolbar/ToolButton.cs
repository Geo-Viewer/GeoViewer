using GeoViewer.Controller.Tools;
using GeoViewer.Model.Tools;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.Toolbar
{
    /// <summary>
    /// The class represents a button which activates a tool.
    /// </summary>
    public class ToolButton : Button
    {
        /// <summary>
        /// The constructor creates a new button which activates a tool.
        /// </summary>
        /// <param name="id">The id of the referenced tool</param>
        public ToolButton(ToolID id)
        {
            var data = id.Tool.Data;
            var image = new VisualElement();
            image.Add(new Image
            {
                vectorImage = data.Icon,
                tintColor = data.Color
            });
            image.AddToClassList("toolbar-item");
            Add(image);
            clicked += () => ToolManager.Instance.Registry.TrySetActiveTool(id);
        }
    }
}