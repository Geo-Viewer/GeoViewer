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
        private SceneObject _object;
        private GameObject _visual;

        /// <summary>
        /// Creates a new <see cref="DeselectObject"/> command.
        /// </summary>
        /// <param name="o">The object to deselect.</param>
        public DeselectObject(SceneObject o, GameObject visual)
        {
            _object = o;
            _visual = visual;
        }

        /// <inheritdoc/>
        public void Execute()
        {
            _visual.layer = LayerMask.NameToLayer(SelectionTool.SelectableLayer);
            _object.IsSelected = false;
        }

        /// <inheritdoc/>
        public void Undo()
        {
            _visual.layer = LayerMask.NameToLayer(SelectionTool.SelectedLayer);
            _object.IsSelected = true;
        }
    }
}