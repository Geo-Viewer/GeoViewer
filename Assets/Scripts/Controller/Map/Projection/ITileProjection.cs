using System;
using System.Collections.Generic;
using GeoViewer.Controller.Util;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace GeoViewer.Controller.Map.Projection
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(WebMercatorProjection), "WebMercator")]
    public interface ITileProjection
    {
        public string Type { get; }

        /// <summary>
        /// Calculates the absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/> based on
        /// the given Zoom-Factor.
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the absolute tile coordinates for</param>
        /// <param name="zoomFactor">The Zoom-Factor to get the absolute tile coordinates for</param>
        /// <returns>The absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/></returns>
        public Vector2Int GlobePointToTileCoordinates(GlobePoint globePoint, int zoomFactor);

        /// <summary>
        /// Calculates the <see cref="GlobePoint"/> at the north-west corner of the tile with the given absolute tile coordinates
        /// and the given Zoom-Factor.
        /// </summary>
        /// <param name="tile">The tile to calculate the north-west corner point for</param>
        /// <returns>A <see cref="GlobePoint"/> at the north-west corner of the tile</returns>
        public GlobePoint TileToGlobePoint(TileId tile);

        /// <summary>
        /// Converts the given tile to a <see cref="GlobeArea"/>
        /// </summary>
        /// <param name="tile">The tile to convert</param>
        /// <returns>A <see cref="GlobeArea"/> matching the given tile coordinates and zoom factor</returns>
        public GlobeArea TileToGlobeArea(TileId tile)
        {
            return new GlobeArea(TileToGlobePoint(tile),
                TileToGlobePoint(new TileId(tile.Coordinates + Vector2Int.one, tile.Zoom)));
        }

        /// <summary>
        /// Converts the given <see cref="GlobeArea"/> to multiple tiles with the given zoom factor, covering the whole area
        /// </summary>
        /// <param name="globeArea">The <see cref="GlobeArea"/> to convert</param>
        /// <param name="zoomFactor">The zoom factor of the resulting tiles</param>
        /// <returns>An IEnumerable of tiles covering the given <see cref="GlobeArea"/></returns>
        public IEnumerable<TileId> GlobeAreaToTiles(GlobeArea globeArea, int zoomFactor)
        {
            var tileCoordinates = GlobePointToTileCoordinates(globeArea.NorthWestPoint, zoomFactor);
            var ne = GlobePointToTileCoordinates(globeArea.NorthEastPoint, zoomFactor);
            var sw = GlobePointToTileCoordinates(globeArea.SouthWestPoint, zoomFactor);
            for (var x = tileCoordinates.x; x <= ne.x; x++)
            {
                for (var y = tileCoordinates.y; y <= sw.y; y++)
                {
                    var tile = new TileId(new Vector2Int(x, y), zoomFactor);
                    if (tile.IsInbounds())
                    {
                        yield return tile;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates a number of tiles covering the whole GlobeArea. Chooses a zoom factor based on the targetTileCount.
        /// This returns the target tile count or less tiles if possible
        /// </summary>
        /// <param name="globeArea">The <see cref="GlobeArea"/> to convert</param>
        /// <param name="zoomBounds">The bounds of the zoom factor</param>
        /// <param name="targetTileCount">The amount of tiles that should be targeted for returning.</param>
        /// <returns>An IEnumerable of tiles covering the given <see cref="GlobeArea"/></returns>
        public IEnumerable<TileId> GlobeAreaToTiles(GlobeArea globeArea, Bounds<int> zoomBounds, int targetTileCount)
        {
            var currentZoom = zoomBounds.Min;

            while (true)
            {
                var tileCoordinates = GlobePointToTileCoordinates(globeArea.NorthWestPoint, currentZoom);
                var ne = GlobePointToTileCoordinates(globeArea.NorthEastPoint, currentZoom);
                var sw = GlobePointToTileCoordinates(globeArea.SouthWestPoint, currentZoom);


                if ((ne.x - tileCoordinates.x + 1) * (sw.y - tileCoordinates.y + 1) > targetTileCount)
                {
                    return GlobeAreaToTiles(globeArea, Math.Clamp(currentZoom - 1, zoomBounds.Min, zoomBounds.Max));
                }

                currentZoom++;

                if (currentZoom > zoomBounds.Max)
                {
                    return GlobeAreaToTiles(globeArea, currentZoom - 1);
                }
            }
        }
    }
}