using GeoViewer.Controller.Tools.BuiltinTools;
using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.Controller.Commands
{
    /// <summary>
    /// A command for selecting an object.
    /// </summary>
    public class SelectObject : ICommand
    {
        private SceneObject _object;
        private GameObject _visual;

        /// <summary>
        /// Creates a new <see cref="SelectObject"/> command.
        /// </summary>
        /// <param name="o">The object to select.</param>
        public SelectObject(SceneObject o, GameObject visual)
        {
            _object = o;
            _visual = visual;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            _visual.layer = LayerMask.NameToLayer(SelectionTool.SelectedLayer);
            _object.IsSelected = true;

            if (_object.IsUserMovable && _object.AttachmentMode == AttachmentMode.RelativeToSurface)
                _object.AttachmentMode = AttachmentMode.Absolute;
        }

        /// <inheritdoc/>
        public void Undo()
        {
            _object.gameObject.layer = LayerMask.NameToLayer(SelectionTool.SelectableLayer);
            _object.IsSelected = false;
        }
    }
}