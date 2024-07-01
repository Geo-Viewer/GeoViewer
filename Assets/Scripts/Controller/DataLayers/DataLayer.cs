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
        protected readonly TSettings Settings;

        public bool Active { get; private set; } = true;
        public int Priority => Settings.Priority;

        public event Action<IDataLayer>? ActiveChanged;

        private readonly ConcurrentDictionary<GlobeArea, TData> _cache = new();
        private readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Creates a new instance of the <see cref="DataLayer{TSettings, TData}"/> class.
        /// </summary>
        /// <param name="settings">The settings for the <see cref="DataLayer{TSettings, TData}"/></param>
        protected DataLayer(TSettings settings)
        {
            Settings = settings;
            _semaphore = new SemaphoreSlim(Settings.ParallelRequests);
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
                if (_cache.Count < Settings.CacheSize)
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
                UnityEngine.Debug.LogWarning($"Layer {Settings.Name} failed to load with error: {e}. Disabling Layer.");
                SetActive(false);
                throw new LayerFailedException($"Layer {Settings.Name} failed.", e, this);
            }
            finally
            {
                stopWatch.Stop();
                if (Settings.RequestsPerSecond > 0)
                {
                    DelayedRelease(Settings.ParallelRequests / Settings.RequestsPerSecond * 1000 -
                                   (int)stopWatch.ElapsedMilliseconds);
                }
                else
                {
                    _semaphore.Release();
                }
            }
        }

        /// <inheritdoc/>
        public abstract void RenderData(TData data, TileGameObject tileGameObject, MapRenderer mapRenderer);

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