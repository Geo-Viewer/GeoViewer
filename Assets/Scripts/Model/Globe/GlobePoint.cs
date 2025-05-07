using System;
using Unity.Mathematics;

namespace GeoViewer.Model.Globe
{
    /// <summary>
    /// A data class storing the position of a point on the globe in latitude, longitude and altitude.
    /// </summary>
    public class GlobePoint : IEquatable<GlobePoint>
    {
        /// <summary>
        /// Stores latitude in degrees
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// Max (Min) latitude value
        /// </summary>
        public const double LatitudeLimit = 85.06d;

        /// <summary>
        /// Returns the latitude as a DMS string
        /// </summary>
        public string DmsLatitude => DegreeToDms(Latitude) + (Latitude < 0 ? " S" : " N");

        /// <summary>
        /// Stores longitude in degrees
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Max (Min) longitude value
        /// </summary>
        public const double LongitudeLimit = 180d;

        /// <summary>
        /// Returns the longitude as a DMS string
        /// </summary>
        public string DmsLongitude => DegreeToDms(Longitude) + (Longitude < 0 ? " W" : " E");

        /// <summary>
        /// Stores altitude in metres above sea height
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Creates a new <see cref="GlobePoint"/> with a given latitude, longitude and altitude.
        /// </summary>
        /// <param name="latitude">latitude in degrees</param>
        /// <param name="longitude">longitude in degrees</param>
        /// <param name="altitude">altitude in degrees</param>
        public GlobePoint(double latitude = 0, double longitude = 0, double altitude = 0)
        {
            Math.Clamp(latitude, 0, LatitudeLimit);
            Math.Clamp(longitude, 0, LongitudeLimit);

            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
        }

        public GlobePoint(GlobePoint globePoint, double altitude) : this(globePoint.Latitude, globePoint.Longitude,
            altitude)
        {
        }

        /// <summary>
        /// Calculates the DMS representation of the given degree value.
        /// </summary>
        /// <param name="degreeValue">given degree value</param>
        /// <returns>string representation of degree value in DMS</returns>
        private string DegreeToDms(double degreeValue)
        {
            degreeValue = math.abs(degreeValue);
            const int multiplier = 60;
            var degrees = (int)math.floor(degreeValue);
            var minutes = (int)math.floor((degreeValue - degrees) * multiplier);
            var seconds = (int)math.floor(((degreeValue - degrees) * multiplier - minutes) * multiplier);
            return degrees + "° " + minutes.ToString().PadLeft(2, '0') + "' " +
                   seconds.ToString().PadLeft(2, '0') + "''";
        }

        public bool Equals(GlobePoint? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) &&
                   Altitude.Equals(other.Altitude);
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

            return Equals((GlobePoint)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Latitude, Longitude, Altitude);
        }

        /// <summary>
        /// Calculates the mid point of two given <see cref="GlobePoint"/>s.
        /// </summary>
        /// <param name="point1">The first <see cref="GlobePoint"/></param>
        /// <param name="point2">The second <see cref="GlobePoint"/></param>
        /// <returns>A <see cref="GlobePoint"/> representing the mid point</returns>
        public static GlobePoint MidPoint(GlobePoint point1, GlobePoint point2)
        {
            return new GlobePoint((point1.Latitude + point2.Latitude) / 2,
                (point1.Longitude + point2.Longitude) / 2,
                (point1.Altitude + point2.Altitude) / 2);
        }
    }
}