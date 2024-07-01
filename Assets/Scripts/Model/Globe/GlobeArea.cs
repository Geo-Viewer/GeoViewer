using System;
using System.Collections.Generic;
using GeoViewer.Controller.Util;

namespace GeoViewer.Model.Globe
{
    /// <summary>
    /// A data class storing an rectangular area on the earth.
    /// </summary>
    public class GlobeArea : IEquatable<GlobeArea>
    {
        /// <summary>
        /// latitude bounds of the area in degrees with x &lt;= y
        /// </summary>
        public readonly Bounds<double> BoundsLat;

        /// <summary>
        /// longitude bounds of the area in degrees with x &gt;= y
        /// </summary>
        public readonly Bounds<double> BoundsLon;

        /// <summary>
        /// Height of the rectangular area in degrees
        /// </summary>
        private double AreaHeight => BoundsLat.Max - BoundsLat.Min;

        /// <summary>
        /// Width of the rectangular area in degrees
        /// </summary>
        private double AreaWidth => BoundsLon.Max - BoundsLon.Min;

        /// <summary>
        /// <see cref="GlobePoint"/> in the middle of the rectangular area on earth
        /// </summary>
        public GlobePoint MidPoint { get; }

        public GlobePoint NorthEastPoint => new(BoundsLat.Max, BoundsLon.Max);
        public GlobePoint NorthWestPoint => new(BoundsLat.Max, BoundsLon.Min);
        public GlobePoint SouthEastPoint => new(BoundsLat.Min, BoundsLon.Max);
        public GlobePoint SouthWestPoint => new(BoundsLat.Min, BoundsLon.Min);

        /// <summary>
        /// Creates a new <see cref="GlobeArea"/> with the given bounds.
        /// </summary>
        /// <param name="boundsLat">latitude bounds of the area in degrees</param>
        /// <param name="boundsLon">longitude bounds of the area in degrees</param>
        private GlobeArea(Bounds<double> boundsLat, Bounds<double> boundsLon)
        {
            BoundsLat = boundsLat;
            BoundsLon = boundsLon;

            //get Midpoint
            var lat = BoundsLat.Min + AreaHeight / 2;
            var lon = BoundsLon.Min + AreaWidth / 2;
            MidPoint = new GlobePoint(lat, lon);
        }

        /// <summary>
        /// Creates a new <see cref="GlobeArea"/> with the <see cref="GlobePoint"/> in the north-west,
        /// and the <see cref="GlobePoint"/> in the south-east of the target area
        /// </summary>
        /// <param name="northWestPoint">A <see cref="GlobePoint"/> at the north-west corner of the target area</param>
        /// <param name="southEastPoint">A <see cref="GlobePoint"/> at the south-east corner of the target area</param>
        public GlobeArea(GlobePoint northWestPoint, GlobePoint southEastPoint) : this(
            new Bounds<double>(northWestPoint.Latitude, southEastPoint.Latitude),
            new Bounds<double>(northWestPoint.Longitude, southEastPoint.Longitude))
        {
        }

        /// <summary>
        ///     gets a List of GlobePoints equally spread over the Area
        /// </summary>
        /// <param name="resolution">the amount of points on each axis</param>
        /// <returns>a resolution x resolution grid of GlobePoints</returns>
        public IReadOnlyList<GlobePoint> GetPointGrid(int resolution)
        {
            List<GlobePoint> pointGrid = new();
            for (var i = 0; i < resolution; i++)
            {
                for (var j = 0; j < resolution; j++)
                {
                    pointGrid.Add(new GlobePoint(BoundsLat.Min + i * (AreaHeight / (resolution - 1)),
                        BoundsLon.Min + j * (AreaWidth / (resolution - 1))));
                }
            }

            return pointGrid;
        }

        /// <summary>
        /// Checks whether this <see cref="GlobeArea"/> contains a given <see cref="GlobePoint"/>
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to check</param>
        /// <returns><c>true</c>, if the <see cref="GlobeArea"/> contains the <see cref="GlobePoint"/>, <c>false</c> otherwise</returns>
        public bool Contains(GlobePoint globePoint)
        {
            return BoundsLat.Contains(globePoint.Latitude) && BoundsLon.Contains(globePoint.Longitude);
        }

        /// <summary>
        /// Checks whether this <see cref="GlobeArea"/> contains a given <see cref="GlobeArea"/>
        /// </summary>
        /// <param name="globeArea">The <see cref="GlobeArea"/> to check</param>
        /// <returns><c>true</c>, if the <see cref="GlobeArea"/> contains the <see cref="GlobeArea"/>, <c>false</c> otherwise</returns>
        public bool Contains(GlobeArea globeArea)
        {
            return BoundsLat.Contains(globeArea.BoundsLat) && BoundsLon.Contains(globeArea.BoundsLon);
        }

        /// <summary>
        /// Checks whether this <see cref="GlobeArea"/> intersects with the given <see cref="GlobeArea"/>
        /// </summary>
        /// <param name="other">The <see cref="GlobeArea"/> to check</param>
        /// <returns><c>true</c>, if the <see cref="GlobeArea"/> intersects with the <see cref="GlobeArea"/>, <c>false</c> otherwise</returns>
        public bool Intersects(GlobeArea other)
        {
            return BoundsLat.Overlaps(other.BoundsLat) && BoundsLon.Overlaps(other.BoundsLon);
        }

        /// <summary>
        /// Returns the <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/>
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to check</param>
        /// <returns>The <see cref="GlobePoint"/> in the GlobeArea, nearest to the given <see cref="GlobePoint"/></returns>
        public GlobePoint GetClosestPoint(GlobePoint globePoint)
        {
            var lat = Math.Clamp(globePoint.Latitude, BoundsLat.Min, BoundsLat.Max);
            var lon = Math.Clamp(globePoint.Longitude, BoundsLon.Min, BoundsLon.Max);

            return new GlobePoint(lat, lon);
        }

        /// <summary>
        /// Determines whether the specified <see cref="GlobeArea"/> is equal to the current <see cref="GlobeArea"/>.
        /// </summary>
        /// <param name="other">The <see cref="GlobeArea"/> to compare with the current <see cref="GlobeArea"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="GlobeArea"/> is equal to the current <see cref="GlobeArea"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(GlobeArea? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return BoundsLat.Equals(other.BoundsLat) && BoundsLon.Equals(other.BoundsLon) &&
                   MidPoint.Equals(other.MidPoint);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
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

            return Equals((GlobeArea)obj);
        }

        /// <summary>
        /// Calculates the hash code for the current object.
        /// </summary>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(BoundsLat, BoundsLon, MidPoint);
        }

        public bool Equals(GlobeArea x, GlobeArea y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.BoundsLat.Equals(y.BoundsLat) && x.BoundsLon.Equals(y.BoundsLon) &&
                   x.MidPoint.Equals(y.MidPoint);
        }

        public int GetHashCode(GlobeArea obj)
        {
            return HashCode.Combine(obj.BoundsLat, obj.BoundsLon, obj.MidPoint);
        }
    }
}