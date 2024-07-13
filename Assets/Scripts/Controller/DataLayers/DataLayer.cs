using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// An abstract class handling the base behaviour for data layers. This handles caching, sending of parallel requests
    /// as well as activation and deactivation.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings for the layer</typeparam>
    /// <typeparam name="TData">The type of data the layer requests and renders</typeparam>
    public abstract class DataLayer<TSettings, TData> : IDataLayer, IDataRequest<TData>
        where TSettings : DataLayerSettings
    {
        /// <summary>
        /// The settings for this layer.
        /// </summary>
        protected readonly TSettings _settings;

        public DataLayerAnalytics Analytics { get; } = new();

        public bool Active { get; private set; } = true;
        public DataLayerSettings Settings => _settings;

        public event Action<IDataLayer>? ActiveChanged;

        private readonly ConcurrentDictionary<GlobeArea, TData> _cache = new();
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Creates a new instance of the <see cref="DataLayer{TSettings, TData}"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="DataLayer{TSettings, TData}"/></param>
        protected DataLayer(TSettings settings)
        {
            _settings = settings;
            _semaphore = new SemaphoreSlim(_settings.ParallelRequests);
        }

        /// <summary>
        /// Requests the data for the given <paramref name="request"/>. Caches the result, if possible.
        /// </summary>
        /// <inheritdoc/>
        public async Task<TData> RequestData((TileId tileId, GlobeArea area) request, CancellationToken token)
        {
            if (_cache.TryGetValue(request.area, out var result))
            {
                return result;
            }

            await _semaphore.WaitAsync(token);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                result = await RequestDataInternal(request, token).ConfigureAwait(false);
                if (_cache.Count < _settings.CacheSize)
                {
                    _cache.TryAdd(request.area, result);
                }

                return result;
            }
            catch (Exception e) when (e is not OperationCanceledException && e is not WebException
                                      {
                                          Status: WebExceptionStatus.RequestCanceled
                                      })
            {
                UnityEngine.Debug.LogWarning($"Layer {_settings.Name} failed to load with error: {e}. Disabling Layer.");
                SetActive(false);
                throw new LayerFailedException($"Layer {_settings.Name} failed.", e, this);
            }
            finally
            {
                stopWatch.Stop();
                Analytics.AddRequestTime((int)stopWatch.ElapsedMilliseconds);
                if (_settings.RequestsPerSecond > 0)
                {
                    DelayedRelease(_settings.ParallelRequests / _settings.RequestsPerSecond * 1000 -
                                   (int)stopWatch.ElapsedMilliseconds);
                }
                else
                {
                    _semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public void RenderData(TData data, TileGameObject tileGameObject, MapRenderer mapRenderer)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                RenderDataInternal(data, tileGameObject, mapRenderer);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Layer {_settings.Name} failed to load with error: {e}. Disabling Layer.");
                SetActive(false);
                throw new LayerFailedException($"Layer {_settings.Name} failed.", e, this);
            }
            finally
            {
                stopWatch.Stop();
                Analytics.AddRenderTime((int)stopWatch.ElapsedMilliseconds);
            }
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            _cache.Clear();
        }

        /// <inheritdoc/>
        public void SetActive(bool active)
        {
            if (active == Active)
            {
                return;
            }

            Active = active;
            ActiveChanged?.Invoke(this);
        }

        /// <summary>
        /// Requests the data for the given <paramref name="request"/> from a service.
        /// </summary>
        /// <param name="request">The area to request data for</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>A Task with the requested data</returns>
        protected abstract Task<TData> RequestDataInternal((TileId tileId, GlobeArea globeArea) request,
            CancellationToken token);

        /// <summary>
        /// Renders the given <paramref name="data"/> to a given <paramref name="tileGameObject"/>.
        /// </summary>
        /// <param name="data">The data to render</param>
        /// <param name="tileGameObject">The <see cref="TileGameObject"/> to render the data onto</param>
        /// <param name="mapRenderer">The map renderer the tile belongs to</param>
        protected abstract void RenderDataInternal(TData data, TileGameObject tileGameObject, MapRenderer mapRenderer);

        /// <summary>
        /// Releases the semaphore after the given <paramref name="milliseconds"/>
        /// </summary>
        /// <param name="milliseconds">The amount of milliseconds to wait</param>
        private async void DelayedRelease(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                _semaphore.Release();
            }
            else
            {
                await Task.Delay(milliseconds);
                _semaphore.Release();
            }
        }
    }

    public class LayerFailedException : Exception
    {
        public IDataLayer Layer { get; }

        public LayerFailedException(string message, Exception exception, IDataLayer layer) : base(message, exception)
        {
            Layer = layer;
        }
    }
}