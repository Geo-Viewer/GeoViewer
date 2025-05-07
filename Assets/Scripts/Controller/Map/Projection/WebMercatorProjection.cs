using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using Unity.Mathematics;
using UnityEngine;

namespace GeoViewer.Controller.Map.Projection
{
    /// <summary>
    /// <para>
    /// An Implementation of the spherical Web-Mercator-Projection based on Google's implementation (EPSG:900913),
    /// as it is the standard for most map tile services.
    /// This is based on the explanations found at
    /// https://www.maptiler.com/google-maps-coordinates-tile-bounds-projection
    /// </para>
    /// <para>
    /// Calculating the tile coordinates is based on
    /// https://wiki.openstreetmap.org/wiki/Slippy_map_tilenames
    /// </para>
    /// </summary>
    public class WebMercatorProjection : IProjection, ITileProjection
    {
        public string Type { get; } = "WebMercator";

        /// <summary>
        /// The Earth's radius in metres
        /// </summary>
        private const int EarthRadius = 6378137;

        /// <summary>
        /// Shifts the origin by half the Earth's circumference
        /// </summary>
        private const double OriginShift = 2 * math.PI * EarthRadius / 2.0;

        /// <summary>
        /// Converts a given <see cref="GlobePoint"/> in WGS84 to a XZ in Spherical Mercator EPSG:900913 with a scaled Y-height,
        /// based on the given points position.
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to be converted</param>
        /// <returns>A position with XZ point in Spherical Mercator EPSG:900913 and a scaled Y-Height as a <see cref="double3"/></returns>
        public double3 GlobePointToPosition(GlobePoint globePoint)
        {
            // project lat, lon to x, z - plane
            var position = new double3
            {
                x = globePoint.Longitude * OriginShift / 180.0,
                z = math.log(math.tan((90 + globePoint.Latitude) * math.PI / 360.0)) / (math.PI / 180.0)
            };
            position.z *= OriginShift / 180.0;

            //scale altitude according to position
            position.y = globePoint.Altitude * GetScaleFactor(globePoint);

            return position;
        }

        /// <summary>
        /// Converts a given position with a XZ point in Spherical Mercator EPSG:900913 to a <see cref="GlobePoint"/>
        /// in WGS84 with the Y-Height scaled accordingly to position.
        /// </summary>
        /// <param name="position">A position with a XZ point in Spherical Mercator EPSG:900913</param>
        /// <returns>A <see cref="GlobePoint"/> in WGS84 with the Y-Height scaled accordingly to position</returns>
        public GlobePoint PositionToGlobePoint(double3 position)
        {
            //project point from x, z - plane onto globe
            var result = new double2()
            {
                x = position.x / OriginShift * 180.0,
                y = position.z / OriginShift * 180.0
            };
            result.y = 180 / math.PI * (2 * math.atan(math.exp(result.y * math.PI / 180.0)) - math.PI / 2.0);

            //scale altitude down
            var globePoint = new GlobePoint(result.y, result.x);
            globePoint.Altitude = position.y / GetScaleFactor(globePoint);

            return globePoint;
        }

        /// <summary>
        /// Calculates the absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/> based on
        /// the given Zoom-Factor.
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the absolute tile coordinates for</param>
        /// <param name="zoomFactor">The Zoom-Factor to get the absolute tile coordinates for</param>
        /// <returns>The absolute tile coordinates of the tile containing the given <see cref="GlobePoint"/></returns>
        public Vector2Int GlobePointToTileCoordinates(GlobePoint globePoint, int zoomFactor)
        {
            var latRad = globePoint.Latitude / 180 * math.PI;
            return new Vector2Int
            {
                //Longitude to tileX
                x = (int)math.floor((globePoint.Longitude + 180) / 360 * (1 << zoomFactor)),
                //Latitude to tileY
                y = (int)math.floor((1 - math.log(math.tan(latRad) + 1 / math.cos(latRad)) / math.PI)
                    / 2 * (1 << zoomFactor))
            };
        }

        /// <summary>
        /// Calculates the <see cref="GlobePoint"/> at the north-west corner of the tile with the given absolute tile coordinates
        /// and the given Zoom-Factor.
        /// </summary>
        /// <param name="tile">The tile to convert</param>
        /// <returns>A <see cref="GlobePoint"/> at the north-west corner of the tile</returns>
        public GlobePoint TileToGlobePoint(TileId tile)
        {
            var tmp = math.PI - 2.0 * math.PI * tile.Coordinates.y / (1 << tile.Zoom);
            double2 point = new()
            {
                //tileX to Longitude
                x = tile.Coordinates.x / (double)(1 << tile.Zoom) * 360 - 180,
                //TileY to Latitude
                y = 180.0 / math.PI * math.atan(0.5 * (math.exp(tmp) - math.exp(-tmp)))
            };

            return new GlobePoint(point.y, point.x);
        }

        /// <summary>
        /// Calculates the factor, by which an object would need to get scaled, to match the surroundings.
        /// This is because the Mercator-Projection is non-scale sustaining.
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the scale factor at</param>
        /// <returns>A scale factor as a double. If the argument given to this function is null, it returns 1</returns>
        public double GetScaleFactor(GlobePoint? globePoint)
        {
            if (globePoint == null)
            {
                return 1;
            }

            return 1 / math.cos(globePoint.Latitude * Mathf.Deg2Rad);
        }
    }
}