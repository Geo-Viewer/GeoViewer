using GeoViewer.Controller.Tools.BuiltinTools;
using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.Controller.Commands
{
    /// <summary>
    /// A command for deselecting an object.
    /// </summary>
    public class DeselectObject : ICommand
    {
        private GameObject _object;

        /// <summary>
        /// Creates a new <see cref="DeselectObject"/> command.
        /// </summary>
        /// <param name="o">The object to deselect.</param>
        public DeselectObject(GameObject o)
        {
            _object = o;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            _object.layer = LayerMask.NameToLayer(SelectionTool.SelectableLayer);
            ApplicationState.Instance.RemoveSelectedObject(_object);
        }

        /// <inheritdoc/>
        public void Undo()
        {
            _object.layer = LayerMask.NameToLayer(SelectionTool.SelectedLayer);
            ApplicationState.Instance.AddSelectedObject(_object);
        }
    }
}