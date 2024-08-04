using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using GeoViewer.Model.DataLayers;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Grid;
using GeoViewer.Model.State;
using GeoViewer.View.Rendering;
using UnityEngine;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// A class managing a collection of <see cref="IDataLayer"/>s. It handles sending requests to all layers as well letting
    /// them render their data.
    /// </summary>
    public class LayerManager
    {
        private readonly LayerCollection<ITextureLayer> _textureLayers =
            new(new BaseTextureLayer(new BaseTextureLayerSettings() { Priority = 0 }));

        private readonly LayerCollection<IMeshLayer> _meshLayers =
            new(new BaseMeshLayer(new BaseMeshLayerSettings() { Priority = 0 }));

        /// <summary>
        /// The segmentation settings of the current active texture layer
        /// </summary>
        public SegmentationSettings CurrentSegmentationSettings => _textureLayers.Current.SegmentationSettings;

        public event Action<IDataLayer>? CurrentLayerChanged;

        public LayerManager(List<DataLayerSettings> dataLayerSettings)
        {
            foreach (var layerSettings in dataLayerSettings)
            {
                if (!layerSettings.Validate())
                {
                    Debug.LogWarning($"Settings for layer {layerSettings.Name} are not valid. Skipping layer.");
                    continue;
                }

                var layer = layerSettings.CreateDataLayer();
                if (layer is ITextureLayer textureLayer)
                {
                    _textureLayers.Add(textureLayer);
                }

                if (layer is IMeshLayer meshLayer)
                {
                    _meshLayers.Add(meshLayer);
                }
            }
            _textureLayers.CurrentLayerChanged += () => OnCurrentLayerChanged(_textureLayers.Current);
            _meshLayers.CurrentLayerChanged += () => OnCurrentLayerChanged(_meshLayers.Current);
        }

        /// <summary>
        /// Creates a new <see cref="TileRequest"/> for the given <paramref name="tile"/>
        /// </summary>
        /// <param name="tile">The tile to create a request for</param>
        /// <returns>A new <see cref="TileRequest"/> for the given <paramref name="tile"/></returns>
        public TileRequest GetTileRequest(TileId tile)
        {
            var request = (tile, CurrentSegmentationSettings.Projection.TileToGlobeArea(tile));

            return new TileRequest(request, _textureLayers.Current, _meshLayers.Current);
        }

        /// <summary>
        /// Creates a new <see cref="TileRequest"/> for the given <paramref name="tile"/>. Generates it's render requests.
        /// </summary>
        /// <param name="tile">The tile to create a request for</param>
        /// <param name="tileGameObject">The <see cref="TileGameObject"/> to render onto</param>
        /// <param name="mapRenderer">The <see cref="MapRenderer"/> to render for</param>
        /// <returns>A new <see cref="TileRequest"/> for the given <paramref name="tile"/></returns>
        public TileRequest GetTileRequest(TileId tile, TileGameObject tileGameObject, MapRenderer mapRenderer)
        {
            var request = (tile, CurrentSegmentationSettings.Projection.TileToGlobeArea(tile));

            return new TileRequest(request, _textureLayers.Current, _meshLayers.Current, tileGameObject, mapRenderer);
        }

        /// <summary>
        /// Clears the cache of all layers
        /// </summary>
        public void ClearLayers()
        {
            foreach (var layer in _textureLayers.GetAllAddedLayers())
            {
                layer.ClearCache();
            }

            foreach (var layer in _meshLayers.GetAllAddedLayers())
            {
                layer.ClearCache();
            }
        }

        /// <summary>
        /// Sets the active state of all layers with the given <paramref name="layerType"/>
        /// </summary>
        /// <param name="layerType">The layer type to set the active state for</param>
        /// <param name="active">The new active state</param>
        /// <exception cref="ArgumentException">thrown if the type does not match an expected layer type</exception>
        public void SetLayersActive(Type layerType, bool active)
        {
            if (layerType == typeof(ITextureLayer))
            {
                foreach (var layer in _textureLayers.GetAllAddedLayers())
                {
                    layer.SetActive(active);
                }
            }
            else if (layerType == typeof(IMeshLayer))
            {
                foreach (var layer in _meshLayers.GetAllAddedLayers())
                {
                    layer.SetActive(active);
                }
            }
            else
            {
                throw new ArgumentException("Unknown layer type");
            }
        }

        /// <summary>
        /// Checks whether a layer with the given <paramref name="layerType"/> are active
        /// </summary>
        /// <param name="layerType">the layer type to check</param>
        /// <returns><c>true</c>, if one layer with the given <paramref name="layerType"/> is active</returns>
        /// <exception cref="ArgumentException">thrown if the type does not match an expected layer type</exception>
        public bool GetLayersActive(Type layerType)
        {
            if (layerType == typeof(ITextureLayer))
            {
                return _textureLayers.GetAllAddedLayers().Any(layer => layer.Active);
            }

            if (layerType == typeof(IMeshLayer))
            {
                return _meshLayers.GetAllAddedLayers().Any(layer => layer.Active);
            }

            throw new ArgumentException("Unknown layer type");
        }

        public void PrintStatistics()
        {
            Debug.Log($"{_textureLayers.Current.Settings.Name}:{_textureLayers.Current.Analytics}");
            Debug.Log($"{_meshLayers.Current.Settings.Name}:{_meshLayers.Current.Analytics}");
        }

        private async void OnCurrentLayerChanged(IDataLayer layer)
        {
            await UniTask.SwitchToMainThread();
            CurrentLayerChanged?.Invoke(layer);
        }
    }
}