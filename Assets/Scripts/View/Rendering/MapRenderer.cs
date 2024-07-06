using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Map.Projection;
using GeoViewer.Controller.Util;
using GeoViewer.Model.DataLayers.Settings;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.Model.State;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GeoViewer.View.Rendering
{
    public class MapRenderer
    {
        #region Settings

        /// <summary>
        /// The current projection used by the map renderer
        /// </summary>
        private IProjection ViewProjection { get; } = new WebMercatorProjection();

        /// <summary>
        /// At which distance the zoom should switch from 18 to 19
        /// </summary>
        private const float Zoom19Distance = 256f;

        /// <summary>
        /// The int value of the terrain layer
        /// </summary>
        private const int TerrainLayer = 6;

        /// <summary>
        /// The maximum distance of the target from the rotation center (if greater map is scaled down)
        /// </summary>
        private const float MaxTargetDistance = 100f;

        /// <summary>
        /// The minimum distance of the target from the rotation center (if smaller map is scaled up)
        /// </summary>
        private const float MinTargetDistance = 50f;

        /// <summary>
        /// The maximum distance of the rotation center from (0, 0, 0) (if grater origin is moved)
        /// </summary>
        private const float MaxRotationCenterDistance = 100f;

        /// <summary>
        /// The minimum size of the request area (Calculated by MinRequestAreaSize * ApplicationState.RadiusMultiplier)
        /// </summary>
        private const float MinRequestAreaSize = 100f;

        /// <summary>
        /// The minimum tile count of the map
        /// </summary>
        private const int BaseTileCount = 16;

        #endregion Settings

        #region Fields

        /// <summary>
        /// The area to currently be requested
        /// </summary>
        public GlobeArea? CurrentRequestArea { get; private set; }

        /// <summary>
        /// The current world origin
        /// </summary>
        public GlobePoint Origin { get; private set; } = new(49.011622, 8.416714, 120);

        private double3 _originPosition = new(936944.30060395563, 183, 6276834.0088540893);
        public double CurrentWorldScale { get; private set; } = 1f;

        private readonly ConcurrentDictionary<TileId, TileGameObject> _renderedTiles = new();
        private readonly ConcurrentDictionary<TileId, TileRequest> _requests = new();
        private readonly ConcurrentDictionary<Transform, GlobePoint> _mapObjects = new(); //TODO: somehow update GlobePoint on move 

        private TaskCompletionSource<object> _updateCancelTask = new();

        private readonly TileGameObject _tilePrefab;
        private readonly Transform _mapParent;
        private Transform? _target;
        private Transform? _rotationCenter;

        private readonly LayerManager _layerManager;

        private HashSet<TileId> _currentSegmentation = new();
        private SegmentationSettings CurrentSegmentationSettings => _layerManager.CurrentSegmentationSettings;

        private bool _enabled = true;

        #endregion Fields

        #region Configuration

        /// <summary>
        /// Creates a new <see cref="MapRenderer"/> for a given <paramref name="layerManager"/>
        /// </summary>
        /// <param name="layerManager">The layer manager to be used</param>
        public MapRenderer(LayerManager layerManager)
        {
            _mapParent = new GameObject("Map").transform;
            _tilePrefab = Resources.Load<TileGameObject>("TilePrefab");
            _layerManager = layerManager;
            _layerManager.CurrentLayerChanged += (layer) => ClearMap(layer);
        }

        /// <summary>
        /// Configures the target transform for this <see cref="MapRenderer"/>
        /// </summary>
        /// <param name="target">The target transform</param>
        public void ConfigureTarget(Transform target)
        {
            _target = target;
            _rotationCenter = ApplicationState.Instance.RotationCenter!.transform;
        }

        #endregion Configuration

        #region Map Building

        /// <summary>
        /// Updates the map by:
        /// 1. Calculating the area to be requested and the corresponding segmentation of tiles
        /// 2. Checking which tile data needs to be requested and requesting them
        /// 3. Rendering the tile data as soon as the request is done
        /// 4. Removing all tiles that are not part of the current segmentation anymore
        /// Any call of this method cancels the current update. If one of the data requests fails, all corresponding map data
        /// will be removed and requested again with the next update.
        /// </summary>
        public async void UpdateMap()
        {
            if (!_enabled || _target == null)
            {
                return;
            }

            CurrentRequestArea = GetRequestArea();
            AdjustWorldScaleAndPosition();

            _currentSegmentation = CalculateSegmentation(CurrentRequestArea, BaseTileCount).ToHashSet();

            //Collect all tasks we have to wait for
            List<TileId> requestIds = new();
            var tcs = new TaskCompletionSource<object>();
            var tasksToAwait = new List<Task> { tcs.Task };
            foreach (var tile in _currentSegmentation)
            {
                if (!_requests.TryGetValue(tile, out var request))
                {
                    var tileObject = GetOrCreateTileObject(tile);
                    if (tileObject is { MeshPriority: >= 0, TexturePriority: >= 0 })
                    {
                        continue;
                    }

                    request = _layerManager.GetTileRequest(tile, tileObject, this);
                    _requests.TryAdd(tile, request);
                }

                tasksToAwait.Add(request.TextureRender!);
                tasksToAwait.Add(request.MeshRender!);
                requestIds.Add(tile);
            }

            if (tasksToAwait.Count <= 1)
            {
                return;
            }

            //Cancel old update if necessary
            if (!_updateCancelTask.Task.IsCanceled)
            {
                Interlocked.Exchange(ref _updateCancelTask, tcs).SetCanceled();
            }
            else
            {
                Interlocked.Exchange(ref _updateCancelTask, tcs);
            }

            //Wait for tasks to finish
            while (tasksToAwait.Count > 1)
            {
                try
                {
                    var task = await Task.WhenAny(tasksToAwait);
                    tasksToAwait.Remove(task);
                    if (task == tcs.Task)
                    {
                        var toCancel = requestIds.Where(tile => !_currentSegmentation.Contains(tile)).ToArray();
                        CancelRequests(toCancel);
                        AdjustRenderingOrder(toCancel, false);
                        return;
                    }

                    await task;
                }
                catch (LayerFailedException)
                {
                    CancelRequests(requestIds);
                    return;
                }
            }

            //Cleanup requests and old tiles
            CancelRequests(requestIds);
            RemoveTiles(_renderedTiles
                .Where(tile => !_currentSegmentation.Contains(tile.Key))
                .Select(tile => tile.Key));
            AdjustRenderingOrder(requestIds);
        }

        /// <summary>
        /// Builds a new map at the given <paramref name="initialOrigin"/>
        /// </summary>
        /// <param name="initialOrigin">The initial origin of the map</param>
        public void BuildMap(GlobePoint initialOrigin)
        {
            if (_enabled)
            {
                ClearMap();
            }

            _enabled = true;
            MoveOrigin(initialOrigin);
            UpdateMap();
        }

        /// <summary>
        /// Tries to get or create a <see cref="TileGameObject"/> for the given <paramref name="tileId"/>.
        /// </summary>
        /// <param name="tileId">The tileId of the tile to get</param>
        /// <returns>The retrieved <see cref="TileGameObject"/></returns>
        private TileGameObject GetOrCreateTileObject(TileId tileId)
        {
            if (_renderedTiles.TryGetValue(tileId, out var tileObject))
            {
                if (!tileObject.RemovalInProgress)
                {
                    return tileObject;
                }

                _renderedTiles.TryRemove(tileId, out _);
            }

            var pos = GlobePointToApplicationPosition(TileToArea(tileId).MidPoint);

            tileObject = Object.Instantiate(_tilePrefab, pos, Quaternion.identity, _mapParent);
            tileObject.TileId = tileId;
            _renderedTiles.TryAdd(tileId, tileObject);

            return tileObject;
        }

        /// <summary>
        /// Creates the area of which to request all tiles
        /// </summary>
        /// <returns>A new <see cref="GlobeArea"/> to be requested</returns>
        private GlobeArea GetRequestArea()
        {
            var pos = _target!.position;
            var distance = Math.Max(Vector3.Distance(pos, ResampleHeight(pos)), MinRequestAreaSize);
            var multiplier = ApplicationState.Instance.Settings.RequestRadiusMultiplier;
            return new GlobeArea(
                ApplicationPositionToGlobePoint(pos + new Vector3(-1, 0, 1) * (distance * multiplier)),
                ApplicationPositionToGlobePoint(pos + new Vector3(1, 0, -1) * (distance * multiplier)));
        }

        /// <summary>
        /// Converts a given <paramref name="tile"/> to a globe area
        /// </summary>
        /// <param name="tile">the tile to convert</param>
        /// <returns>A new <see cref="GlobeArea"/></returns>
        private GlobeArea TileToArea(TileId tile)
        {
            return _layerManager.CurrentSegmentationSettings.Projection.TileToGlobeArea(tile);
        }

        /// <summary>
        /// Clears the whole rendered ma and data layer caches
        /// </summary>
        public void ClearMap(bool clearAttachedObjects = false)
        {
            _enabled = false;
            _updateCancelTask.SetCanceled();
            foreach (var tile in _requests.Keys)
            {
                CancelRequest(tile);
            }

            RemoveTiles(_renderedTiles.Keys);
            if (clearAttachedObjects)
                _mapObjects.Clear();
            MoveOrigin(new GlobePoint());
            CurrentWorldScale = 1f;
            _mapParent.localScale = Vector3.one;

            _layerManager.ClearLayers();
        }

        /// <summary>
        /// Clears the map data for a specific layer
        /// </summary>
        /// <param name="layerType">The data layer type to remove the map data for</param>
        /// <param name="queueUpdate">whether the map should queue and update after clearing</param>
        /// <exception cref="ArgumentException">Thrown, if the given type does not match an expected type</exception>
        private void ClearMap(Type layerType, bool queueUpdate = true)
        {
            CancelRequests(_requests.Keys);
            Action<TileGameObject>? clearAction;
            if (layerType == typeof(ITextureLayer))
            {
                clearAction = tile => tile.ClearTexture();
            }
            else if (layerType == typeof(IMeshLayer))
            {
                clearAction = tile => tile.ClearMesh();
            }
            else
            {
                throw new ArgumentException("Unknown layer type");
            }

            _updateCancelTask.SetCanceled();

            foreach (var tile in _renderedTiles)
            {
                clearAction?.Invoke(tile.Value);
            }

            if (queueUpdate)
                UpdateMap();
        }

        /// <summary>
        /// Clears map data for the given <paramref name="dataLayer"/> object
        /// </summary>
        /// <param name="dataLayer">The data layer of the type to clear the map for</param>
        /// <param name="queueUpdate">whether the map should queue and update after clearing</param>
        /// <exception cref="ArgumentException">Thrown, if the given dataLayer does not match an expected type</exception>
        private void ClearMap(IDataLayer dataLayer, bool queueUpdate = true)
        {
            if (dataLayer is ITextureLayer)
            {
                ClearMap(typeof(ITextureLayer), queueUpdate);
            }
            else if (dataLayer is IMeshLayer)
            {
                ClearMap(typeof(IMeshLayer), queueUpdate);
            }
            else
            {
                throw new ArgumentException("Unknown layer type");
            }
        }

        #endregion Map Building

        #region Segmentation Calculation

        /// <summary>
        /// Calculates the segmentation for the given <paramref name="area"/> with the following algorithm:
        /// 1. Calculate base segmentation with tile base count and add them to count
        /// 2. Foreach queue element:
        /// 2.1 Check if zoom + 1 is out of bounds -> yield return element
        /// 2.2. Check if the point closest to the camera has a higher zoom factor. If yes queue sub tiles. Else return tile.
        /// 2.3 Queue every sub tile intersecting the main area
        /// </summary>
        /// <param name="area">The area to be covered by the segmentation</param>
        /// <param name="targetTileCount">The minimum amount of tiles in the segmentation</param>
        /// <param name="fillBaseTiles">Whether outside base tiles should be returned or deleted</param>
        /// <returns>The calculated segmentation as an enumerable</returns>
        public IEnumerable<TileId> CalculateSegmentation(GlobeArea area, int targetTileCount = 32,
            bool fillBaseTiles = true)
        {
            var settings = _layerManager.CurrentSegmentationSettings;
            Queue<TileId> queue = new();
            foreach (var tile in settings.Projection.GlobeAreaToTiles(area, settings.ZoomBounds, targetTileCount))
            {
                queue.Enqueue(tile);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Zoom >= settings.ZoomBounds.Max)
                {
                    yield return current;
                    continue;
                }

                if (GetTargetZoom(current) <= current.Zoom)
                {
                    yield return current;
                    continue;
                }

                foreach (var subTile in current.GetSubTiles())
                {
                    if (fillBaseTiles)
                    {
                        queue.Enqueue(subTile);
                    }
                    else
                    {
                        var subArea = settings.Projection.TileToGlobeArea(current);
                        if (subArea.Intersects(area))
                        {
                            queue.Enqueue(subTile);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the target zoom factor for a given <paramref name="globePoint"/>
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the target zoom for</param>
        /// <returns>The target zoom</returns>
        private int GetTargetZoom(GlobePoint globePoint)
        {
            var distance = Vector3.Distance(
                ApplicationPositionToWorldPosition(GlobePointToApplicationPosition(globePoint, true)),
                ApplicationPositionToWorldPosition(_target!.position));

            var log = Math.Log(
                distance / (Zoom19Distance * ApplicationState.Instance.Settings.ResolutionMultiplier *
                            CurrentSegmentationSettings.ResolutionMultiplier), 2);
            var roundedLog = log < 0 ? (int)log - 1 : (int)log;
            var targetZoom = 18 - roundedLog;

            return Math.Clamp(targetZoom, CurrentSegmentationSettings.ZoomBounds.Min,
                CurrentSegmentationSettings.ZoomBounds.Max);
        }

        /// <summary>
        /// Calculates the target zoom factor for the closest point ofa given <paramref name="tileId"/>
        /// </summary>
        /// <param name="tileId">The <see cref="TileId"/> to calculate the target zoom for</param>
        /// <returns>The target zoom</returns>
        private int GetTargetZoom(TileId tileId)
        {
            return GetTargetZoom(CurrentSegmentationSettings.Projection.TileToGlobeArea(tileId)
                .GetClosestPoint(ApplicationPositionToGlobePoint(_target!.position)));
        }

        #endregion Segmentation Calculation

        /// <summary>
        /// Adjust the positions of the given <paramref name="tileIds"/>.
        /// They are initially moved up, so that are rendered above (amazing, I know), so they have to be moved down.
        /// </summary>
        /// <param name="tileIds">The tiles to adjust</param>
        /// <param name="delayed">whether the adjustment should be delayed by fade duration</param>
        private void AdjustRenderingOrder(IEnumerable<TileId> tileIds, bool delayed = true)
        {
            foreach (var tile in tileIds)
            {
                if (_renderedTiles.TryGetValue(tile, out var tileObj))
                {
                    tileObj.AdjustRenderingOrder(delayed);
                }
            }
        }

        /// <summary>
        /// Scales the map according to the distance to the target and adjusts the origin based on the rotation center position
        /// </summary>
        private void AdjustWorldScaleAndPosition()
        {
            if (Vector3.Distance(_rotationCenter!.position, Vector3.zero) >
                MaxRotationCenterDistance)
            {
                MoveOrigin(_rotationCenter.position);
            }

            const float targetDistance = MinTargetDistance + (MaxTargetDistance - MinTargetDistance) / 2;
            var distance = Vector3.Distance(_target!.position, _rotationCenter.position);
            if (distance > MaxTargetDistance || (distance < MinTargetDistance && _mapParent.transform.localScale.x < 1))
            {
                CurrentWorldScale *= targetDistance / distance;
                _mapParent.transform.localScale = Vector3.one * (float)CurrentWorldScale;
            }

            CurrentRequestArea = GetRequestArea();
        }

        /// <summary>
        /// Sets the new origin to a given <paramref name="globePoint"/>
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to set the origin to</param>
        public void MoveOrigin(GlobePoint globePoint)
        {
            Origin = globePoint;
            var newOriginPosition = ViewProjection.GlobePointToPosition(globePoint);

            var delta = WorldPositionToApplicationPosition(newOriginPosition) -
                        WorldPositionToApplicationPosition(_originPosition);
            foreach (Transform transform in _mapParent.transform)
            {
                transform.position -= delta;
            }

            _originPosition = newOriginPosition;
        }

        /// <summary>
        /// Sets the new origin to a given <paramref name="applicationPosition"/>
        /// </summary>
        /// <param name="applicationPosition">The Application position to set the origin to</param>
        public void MoveOrigin(Vector3 applicationPosition)
        {
            MoveOrigin(ApplicationPositionToGlobePoint(applicationPosition));
        }

        public void MoveOrigin(Transform mapObject)
        {
            if (!_mapObjects.TryGetValue(mapObject, out var point)) return;
            MoveOrigin(point);
        }

        /// <summary>
        /// Attaches the given transform to the map, so that it scales and moves with it.
        /// </summary>
        /// <param name="transform">The transform to attach</param>
        public void AttachToMap(Transform transform)
        {
            var point = ApplicationPositionToGlobePoint(transform.position);
            if (_mapObjects.TryAdd(transform, point))
                transform.parent = _mapParent;
            else
                _mapObjects.TryUpdate(transform, point, _mapObjects[transform]);
        }

        /// <summary>
        /// Attaches the given transform to the map at the given <paramref name="globePoint"/>, so that it scales and moves with it.
        /// This will also adjust its scale.
        /// </summary>
        /// <param name="transform">The transform to attach</param>
        /// <param name="globePoint">The globe point to attach the transform to</param>
        /// <param name="pivotDelta">The amount to move the object pivot by</param>
        /// <param name="attachToGround">Whether the pivot should be snapped to ground height</param>
        public void AttachToMap(Transform transform, GlobePoint globePoint, Vector3 pivotDelta,
            bool attachToGround = false)
        {
            var scale = (float)ViewProjection.GetScaleFactor(globePoint) * (float)CurrentWorldScale;
            transform.localScale = Vector3.one * scale;

            var pos = GlobePointToApplicationPosition(globePoint) - pivotDelta * scale;

            if (attachToGround)
            {
                pos = ResampleHeight(pos);
                pos.y -= pivotDelta.y * scale;
            }

            transform.position = pos;
            AttachToMap(transform);
        }

        /// <summary>
        /// Returns all rendered Tiles that are neighbours of the given <paramref name="tileId"/>
        /// </summary>
        /// <param name="tileId">The tile to get the neighbours of</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all neighbours</returns>
        public IEnumerable<(TileGameObject tileGameObject, Vector2Int direction)> GetRenderedNeighbours(
            TileGameObject tileObject)
        {
            foreach (var renderedTile in _renderedTiles)
            {
                if (!renderedTile.Value.RemovalInProgress &&
                    tileObject.TileId.IsNeighbourOf(renderedTile.Key, out var direction))
                {
                    if (_currentSegmentation.Contains(renderedTile.Key))
                        yield return (renderedTile.Value, direction);
                }
            }
        }

        #region Position Calculation

        /// <summary>
        /// Converts a given <paramref name="globePoint"/> to a position in the Application. This considers map transformation.
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to convert</param>
        /// <param name="resampleHeight">Whether the returned position should be at terrain height</param>
        /// <returns>A position in the Application</returns>
        public Vector3 GlobePointToApplicationPosition(GlobePoint globePoint, bool resampleHeight = false)
        {
            var pos = WorldPositionToApplicationPosition(
                ViewProjection.GlobePointToPosition(globePoint));
            if (resampleHeight)
            {
                pos = ResampleHeight(pos);
            }

            return pos;
        }

        /// <summary>
        /// Converts a given <param name="applicationPosition"/> to a globe point
        /// </summary>
        /// <param name="applicationPosition">The position in the application</param>
        /// <returns>The matching globe point</returns>
        public GlobePoint ApplicationPositionToGlobePoint(Vector3 applicationPosition)
        {
            return ViewProjection.PositionToGlobePoint(ApplicationPositionToWorldPosition(applicationPosition)
                .ToDouble3());
        }

        /// <summary>
        /// Converts a given <paramref name="applicationPosition"/> to a real world scale position
        /// </summary>
        /// <param name="applicationPosition">The position in the Application</param>
        /// <returns>A position in real world scale</returns>
        public Vector3 ApplicationPositionToWorldPosition(Vector3 applicationPosition)
        {
            return applicationPosition / (float)CurrentWorldScale + _originPosition.ToVector3();
        }

        /// <summary>
        /// Converts a real world scale position to an application position
        /// </summary>
        /// <param name="worldPosition">The position in real world scale</param>
        /// <returns>A position in the Application</returns>
        public Vector3 WorldPositionToApplicationPosition(double3 worldPosition)
        {
            return ((worldPosition - _originPosition) * (float)CurrentWorldScale).ToVector3();
        }

        /// <summary>
        /// Moves a given <paramref name="position"/> to terrain height
        /// </summary>
        /// <param name="position">The position to move</param>
        /// <returns>The adjusted position</returns>
        private Vector3 ResampleHeight(Vector3 position)
        {
            if (Physics.Raycast(new Vector3(position.x, 10000, position.z), Vector3.down, out var hit,
                Mathf.Infinity, 1 << TerrainLayer))
            {
                position.y = hit.point.y;
            }
            else
            {
                position.y = (float)-_originPosition.y * (float)CurrentWorldScale;
            }

            return position;
        }

        #endregion Position Calculation

        private void CancelRequest(TileId tileId)
        {
            if (_requests.TryRemove(tileId, out var request))
            {
                request.Cancel();
            }
        }

        private void CancelRequests(IEnumerable<TileId> tileIds)
        {
            foreach (var tile in tileIds)
            {
                CancelRequest(tile);
            }
        }

        private void RemoveTile(TileId tileId)
        {
            if (_renderedTiles.TryRemove(tileId, out var tileGameObject))
            {
                tileGameObject.Remove();
            }
        }

        private void RemoveTiles(IEnumerable<TileId> tileIds)
        {
            foreach (var tile in tileIds)
            {
                RemoveTile(tile);
            }
        }
    }
}