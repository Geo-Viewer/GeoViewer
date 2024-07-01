using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeoViewer.Model.Grid
{
    /// <summary>
    /// A class storing the coordinates of a tile and its zoom level.
    /// Provides helper methods to get neighbouring tiles, childs or parents.
    /// </summary>
    public class TileId : IEquatable<TileId>
    {
        /// <summary>
        /// Creates a new <see cref="TileId"/>
        /// </summary>
        /// <param name="coordinates">The coordinates of the tile</param>
        /// <param name="zoom">The zoom factor of the tile</param>
        public TileId(Vector2Int coordinates, int zoom)
        {
            Coordinates = coordinates;
            Zoom = zoom;
        }

        /// <summary>
        /// The coordinates of the tile
        /// </summary>
        public Vector2Int Coordinates { get; private set; }

        /// <summary>
        /// The zoom factor of the tile
        /// </summary>
        public int Zoom { get; private set; }

        /// <summary>
        /// Calculates top left sub-tile id of the current tile
        /// </summary>
        /// <param name="zoomDifference">The zoom difference</param>
        /// <returns>The <see cref="TileId"/> of the top left sub-tile</returns>
        public TileId GetTopLeftSubTile(int zoomDifference = 1)
        {
            return new TileId(Coordinates * (int)Math.Pow(2, zoomDifference),
                Zoom + zoomDifference);
        }

        public TileId GetNeighbour(Vector2Int direction)
        {
            return new TileId(Coordinates + direction * new Vector2Int(1, -1), Zoom);
        }

        /// <summary>
        /// Returns all sub-tiles of the current tile with the given zoom difference
        /// </summary>
        /// <param name="zoomDifference">The amount of sub-tile iterations to perform</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing all sub-tiles</returns>
        public IEnumerable<TileId> GetSubTiles(int zoomDifference = 1)
        {
            for (var i = 0; i < Math.Pow(2, zoomDifference); i++)
            {
                for (var j = 0; j < Math.Pow(2, zoomDifference); j++)
                {
                    yield return new TileId(Coordinates * (int)Math.Pow(2, zoomDifference) + new Vector2Int(i, j),
                        Zoom + zoomDifference);
                }
            }
        }

        /// <summary>
        /// Returns the parent tile with the given zoom difference
        /// </summary>
        /// <param name="zoomDifference">The amount of parent iterations to perform</param>
        /// <returns>The parent tile with the given zoom difference</returns>
        public TileId GetParentTile(int zoomDifference = 1)
        {
            if (zoomDifference == 0) return this;
            if (zoomDifference < 0) zoomDifference = -zoomDifference;
            return new TileId(Coordinates / (int)Math.Pow(2, zoomDifference), Zoom - zoomDifference);
        }

        /// <summary>
        /// Checks if the current tile is a neighbour of the given tile
        /// </summary>
        /// <param name="neighbour">The tile to check for</param>
        /// <param name="direction">The direction to the given tile. This tiles Coordinates + the direction gives given tile coordinates (adjusted to same zoom)</param>
        /// <returns><c>true</c>, if the current tile is a neighbour of the given tile, <c>false</c> otherwise</returns>
        public bool IsNeighbourOf(TileId neighbour, out Vector2Int direction)
        {
            var zoomDifference = Zoom - neighbour.Zoom;
            if (zoomDifference < 0)
            {
                var isNeighbour = neighbour.IsNeighbourOf(this, out direction);
                direction = -direction;
                return isNeighbour;
            }

            var parent = GetParentTile(zoomDifference);
            direction = (neighbour.Coordinates - parent.Coordinates) * new Vector2Int(1, -1);
            if (Math.Abs(direction.x) + Math.Abs(direction.y) != 1) return false;
            return !GetNeighbour(direction).GetParentTile(zoomDifference).Equals(parent); //make sure tile is at edge
        }

        /// <summary>
        /// Checks if the current tile is covered by the given tile (or if the given tile is a parent of this tile)
        /// </summary>
        /// <param name="tileId">The tile to check for</param>
        /// <returns><c>true</c>, if the current tile is covered by the given tile, <c>false</c> otherwise</returns>
        public bool IsCoveredBy(TileId tileId)
        {
            if (tileId.Zoom > Zoom) //the other tile is smaller
            {
                return false;
            }

            return tileId.Coordinates == Coordinates / (int)Math.Pow(2, Zoom - tileId.Zoom);
        }

        public override string ToString()
        {
            return $"{Zoom}_{Coordinates.x}_{Coordinates.y}";
        }

        /// <summary>
        /// Checks if the tile coordinates are in bounds
        /// </summary>
        /// <returns><c>true</c>, if the tile coordinates are in bounds, <c>false</c> otherwise</returns>
        public bool IsInbounds()
        {
            var maxCoord = Math.Pow(2, Zoom);
            return Coordinates.x < maxCoord && Coordinates.y < maxCoord && Coordinates.x >= 0 && Coordinates.y >= 0;
        }

        public bool Equals(TileId? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Coordinates.Equals(other.Coordinates) && Zoom == other.Zoom;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((TileId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coordinates, Zoom);
        }
    }
}