using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeoViewer.Controller.Commands
{
    /// <summary>
    /// A command for transforming selected objects.
    /// </summary>
    public class TransformSelected : ICommand
    {
        private readonly (Transform transform, Vector3 positionDelta, Quaternion rotationDelta, Vector3
            scaleDelta)[] _deltas;

        /// <summary>
        /// Creates a new instance of the <see cref="TransformSelected"/> class.
        /// </summary>
        /// <param name="deltas">The deltas of the Transformation.</param>
        public TransformSelected(
            (Transform transform, Vector3 positionDelta, Quaternion rotationDelta, Vector3 scaleDelta)[]
                deltas)
        {
            _deltas = deltas;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TransformSelected"/> class.
        /// </summary>
        /// <param name="transforms">The transforms to modify</param>
        /// <param name="positionDelta">The position delta to apply to all objects</param>
        public TransformSelected(IEnumerable<Transform> transforms, Vector3 positionDelta) : this(
            transforms.Select((x) => (x, positionDelta, Quaternion.identity, Vector3.zero)).ToArray())
        {
        }

        /// <inheritdoc/>
        public void Execute()
        {
            foreach (var (transform, positionDelta, rotationDelta, scaleDelta) in _deltas)
            {
                transform.position += positionDelta;
                transform.rotation *= rotationDelta;
                transform.localScale += scaleDelta;
            }
        }

        /// <inheritdoc/>
        public void Undo()
        {
            foreach (var (transform, positionDelta, rotationDelta, scaleDelta) in _deltas)
            {
                transform.position -= positionDelta;
                transform.rotation *= Quaternion.Inverse(rotationDelta);
                transform.localScale -= scaleDelta;
            }
        }
    }
}