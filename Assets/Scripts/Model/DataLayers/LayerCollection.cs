using System;
using System.Collections.Generic;
using System.Linq;
using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers
{
    /// <summary>
    /// A class handling a collection of data layers of the same type
    /// </summary>
    /// <typeparam name="T">The type of contained data layers</typeparam>
    public class LayerCollection<T> where T : IDataLayer
    {
        private List<T> _layers = new();

        /// <summary>
        /// Called when the current layer of this collection changes
        /// </summary>
        public event Action CurrentLayerChanged;

        private T _current;

        /// <summary>
        /// The highest priority layer. Base layer if no other layer is active
        /// </summary>
        public T Current
        {
            get => _current;
            private set
            {
                if (value.Equals(_current)) return;
                _current = value;
                CurrentLayerChanged?.Invoke();
            }
        }

        private T _baseLayer;

        /// <summary>
        /// Creates a new layer collection
        /// </summary>
        /// <param name="baseLayer">The base layer of this collection</param>
        /// <param name="layers">list of additional layers</param>
        public LayerCollection(T baseLayer, params T[] layers)
        {
            _baseLayer = baseLayer;
            _current = baseLayer;
            foreach (var layer in layers)
            {
                Add(layer);
            }

            Current = GetHighestPriorityLayer();
        }

        /// <summary>
        /// Adds a new layer to the collection
        /// </summary>
        /// <param name="layer">The layer to add</param>
        public void Add(T layer)
        {
            _layers.Add(layer);
            layer.ActiveChanged += OnLayerActiveChanged;
            Current = GetHighestPriorityLayer();
        }

        /// <summary>
        /// Removes a layer from the collection
        /// </summary>
        /// <param name="layer">The layer to remove</param>
        public void Remove(T layer)
        {
            _layers.Remove(layer);
            layer.ActiveChanged -= OnLayerActiveChanged;
            if (layer.Equals(Current))
                Current = GetHighestPriorityLayer();
        }

        private T GetHighestPriorityLayer()
        {
            var currentPriority = int.MinValue;
            var currentHighest = _baseLayer;
            foreach (var layer in _layers.Where(layer => layer.Active))
            {
                if (layer.Settings.Priority > currentPriority)
                {
                    currentPriority = layer.Settings.Priority;
                    currentHighest = layer;
                }
            }
            return currentHighest;
        }

        /// <summary>
        /// Gets all added layers of this collection
        /// </summary>
        /// <returns>An enumerable of all added layers</returns>
        public IEnumerable<T> GetAllAddedLayers()
        {
            foreach (var layer in _layers)
            {
                yield return layer;
            }
        }

        private void OnLayerActiveChanged(IDataLayer layer)
        {
            Current = GetHighestPriorityLayer();
        }
    }
}