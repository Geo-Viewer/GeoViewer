using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoViewer.Model.Grid;
using GeoViewer.Model.State;
using NaughtyAttributes;
using UnityEngine;

namespace GeoViewer.View.Rendering
{
    /// <summary>
    /// A class sitting on the Tile Prefab storing the references to components and setting their values.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TileGameObject : MonoBehaviour
    {
        #region Settings

        [SerializeField] private MeshFilter meshFilter = null!;

        [SerializeField] private MeshCollider meshCollider = null!;

        [SerializeField] private MeshRenderer meshRenderer = null!;

        [SerializeField] private Material tileMaterial = null!;

        [SerializeField] private float fadeDuration;

        private const float ZOffsetMultiplier = 0.01f;

        #endregion Settings

        #region Fields

        public TileId TileId { get; set; }

        /// <summary>
        /// The priority of the currently rendered texture. int.MinValue when no texture is rendered
        /// </summary>
        public int TexturePriority { get; private set; } = int.MinValue;

        /// <summary>
        /// The priority of the currently rendered mesh. int.MinValue when no mesh is rendered
        /// </summary>
        public int MeshPriority { get; private set; } = int.MinValue;

        /// <summary>
        /// Whether this tile is currently in the process of being removed
        /// </summary>
        public bool RemovalInProgress { get; private set; }

        private float _fadeValue;
        private bool _fadeIn = true;
        private Material _material = null!;
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        private static readonly int ZOffsetValue = Shader.PropertyToID("_ZOffsetValue");

        public event Action<TileId> MeshSet;

        #endregion Fields

        private void Awake()
        {
            meshRenderer.material = tileMaterial;
            meshRenderer.enabled = false;
            _material = meshRenderer.material;
            SetAlpha(1);
        }

        public async void Remove()
        {
            if (RemovalInProgress) return;
            RemovalInProgress = true;
            await Task.Delay((int)(fadeDuration * 1000));
            FadeOut();
            Destroy(gameObject, fadeDuration);
        }

        #region Data Rendering

        /// <summary>
        /// Sets the mesh of this tile. Whether it's rendered or not depends on the <paramref name="priority"/>
        /// </summary>
        /// <param name="mesh">The mesh which should be rendered</param>
        /// <param name="priority">The priority of the mesh</param>
        public void SetMesh(Mesh mesh, int priority)
        {
            if (priority < MeshPriority)
            {
                return;
            }

            if (MeshPriority < 0 && TexturePriority >= 0)
            {
                FadeIn();
            }

            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            MeshPriority = priority;
            MeshSet.Invoke(TileId);
        }

        /// <summary>
        /// Clears the mesh of this tile and resets the <see cref="MeshPriority"/>
        /// </summary>
        public void ClearMesh()
        {
            meshFilter.mesh = null;
            meshCollider.sharedMesh = null;
            MeshPriority = int.MinValue;
        }

        [Button]
        public void DebugNeighbours()
        {
            Debug.Log($"This Tile Id is: {TileId}");
            foreach (var neighbour in ApplicationState.Instance.MapRenderer.GetRenderedNeighbours(this))
            {
                Debug.Log($"Neighbour: {neighbour.tileGameObject.TileId} in  {neighbour.direction}");
            }
            AdjustRenderingOrder(false);
        }

        [Button]
        public void AdjustEdges()
        {
            Debug.Log($"This Tile Id is: {TileId}");
            foreach (var neighbour in ApplicationState.Instance.MapRenderer.GetRenderedNeighbours(this))
            {
                AdjustVertexHeight(neighbour.tileGameObject, neighbour.direction, (uint)8);
            }
        }

        public void AdjustVertexHeight(TileGameObject neighbour, Vector2Int direction, uint resolution,
            bool allowPropagation = true)
        {
            var zoomDifference = TileId.Zoom - neighbour.TileId.Zoom;
            if (zoomDifference <= 0 && allowPropagation)
            {
                neighbour.AdjustVertexHeight(this, -direction, resolution, false);
                return;
            }

            if (MeshPriority <= 0 || neighbour.MeshPriority <= 0)
            {
                return;
            }

            //it's ensured, that we are on the tile with the higher zoom factor (and therefore higher absolute resolution)
            var mesh = meshFilter.mesh;
            var neighbourMesh = neighbour.meshFilter.mesh;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var neighbourVertices = neighbourMesh.vertices;
            var neighbourNormals = neighbourMesh.normals;

            var vertexInterval = (uint)Math.Pow(2, zoomDifference);
            var tilePositionDelta = TileId.Coordinates - neighbour.TileId.GetSubTile(zoomDifference).Coordinates;

            for (uint i = 0; i < resolution; i++)
            {
                //Calculate nearest vertices on neighbour mesh
                float indexValue =
                    (i + (direction.x == 0 ? tilePositionDelta.x : vertexInterval - tilePositionDelta.y - 1) *
                        (resolution - 1))
                    / (float)vertexInterval;
                //linear interpolation
                var lowerVertexIndex = GetEdgeVertex(-direction, (uint)Math.Floor(indexValue), resolution, out _);
                var upperVertexIndex = GetEdgeVertex(-direction, (uint)Math.Ceiling(indexValue), resolution, out _);

                var middle = (indexValue - (uint)Math.Floor(indexValue));

                //AdjustHeight
                var height = neighbourVertices[lowerVertexIndex].y + middle *
                    (neighbourVertices[upperVertexIndex].y - neighbourVertices[lowerVertexIndex].y);
                vertices[GetEdgeVertex(direction, i, resolution, out _)].y = height;

                //Adjust Normals
                var normal = neighbourNormals[lowerVertexIndex] +
                             middle * (neighbourNormals[upperVertexIndex] - neighbourNormals[lowerVertexIndex]);
                normals[GetEdgeVertex(direction, i, resolution, out _)] = normal;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.MarkModified();
        }

        /// <summary>
        /// Index 0 is always left/bottom
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="index"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private uint GetEdgeVertex(Vector2Int edge, uint index, uint resolution, out (uint x, uint y) coordinates)
        {
            if (Math.Abs(edge.x) + Math.Abs(edge.y) != 1)
            {
                throw new ArgumentException("Invalid direction");
            }

            if (edge == Vector2Int.up)
                coordinates = (index, resolution - 1);
            else if (edge == Vector2Int.down)
                coordinates = (index, 0);
            else if (edge == Vector2Int.left)
                coordinates = (0, index);
            else
                coordinates = (resolution - 1, index);
            return GetVertexIndex(coordinates, resolution);
        }

        private uint GetVertexIndex((uint x, uint y) position, uint resolution)
        {
            return position.y * resolution + position.x;
        }

        /// <summary>
        /// Sets the texture of this tile. Whether it's rendered or not depends on the <paramref name="priority"/>
        /// </summary>
        /// <param name="texture">The texture which should be rendered</param>
        /// <param name="priority">The priority of the texture</param>
        public void SetTexture(Texture texture, int priority)
        {
            if (priority < TexturePriority)
            {
                return;
            }

            if (TexturePriority < 0)
            {
                meshRenderer.enabled = true;
                if (MeshPriority >= 0)
                    FadeIn();
            }

            _material.SetTexture(BaseMap, texture);
            TexturePriority = priority;
        }

        /// <summary>
        /// Clears the texture of this tile and resets the <see cref="TexturePriority"/>
        /// </summary>
        public void ClearTexture()
        {
            _material.SetTexture(BaseMap, null);
            meshRenderer.enabled = false;
            TexturePriority = int.MinValue;
        }

        #endregion Data Rendering
        public async void AdjustRenderingOrder(bool delayed = true)
        {
            if (delayed)
            {
                await Task.Delay((int)(fadeDuration * 2 * 1000));
                _material.SetFloat(ZOffsetValue, 0f);
            }
            else
            {
                var offset = _material.GetFloat(ZOffsetValue);
                _material.SetFloat(ZOffsetValue, offset - ZOffsetMultiplier);
            }
        }

        #region Fading

        private void SetAlpha(float value)
        {
            _material.SetFloat(Alpha, value);
        }

        private void FadeIn()
        {
            _material.SetFloat(ZOffsetValue, 5f * ZOffsetMultiplier);
            _fadeValue = 1;
            _fadeIn = true;
            SetAlpha(0);
        }

        private void FadeOut()
        {
            _fadeValue = 0;
            _fadeIn = false;
            SetAlpha(1);
        }

        private void Update()
        {
            if (_fadeIn && !(_fadeValue > 0))
            {
                return;
            }

            if (!_fadeIn && !(_fadeValue < 1))
            {
                return;
            }

            if (fadeDuration <= 0)
            {
                _fadeValue = _fadeIn ? 0 : 1;
            }
            else
            {
                var val = Time.deltaTime / fadeDuration;
                _fadeValue = _fadeIn ? _fadeValue - val : _fadeValue + val;
            }

            SetAlpha(1 - _fadeValue);
        }

        #endregion Fading
    }
}