using GeoViewer.Model.Globe;
using Unity.Mathematics;

namespace GeoViewer.Controller.Util
{
    /// <summary>
    /// A utility class for converting ECEF to LLA (WGS84) coordinates based on
    /// https://github.com/MexicanMan/GeoConvert/blob/master/GeoConvert/GeoConvertor.cs
    /// </summary>
    public static class EcefConverter
    {
        private const double EquatorialRadius = 6378137.0;
        private const double InverseFlattening = 1.0 / 298.257224;
        private const double PolarRadius = EquatorialRadius - EquatorialRadius * InverseFlattening;

        /// <summary>
        /// Converts LLA (WGS84) coordinates to ECEF coordinates
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to convert</param>
        /// <returns>ECEF coordinates as a <see cref="double3"/></returns>
        public static double3 GlobePointToEcef(GlobePoint globePoint)
        {
            var lat = globePoint.Latitude;
            var lon = globePoint.Longitude;
            var alt = globePoint.Altitude;

            lat = math.PI * lat / 180.0;
            lon = math.PI * lon / 180.0;

            var cosLat = math.cos(lat);
            var sinLat = math.sin(lat);

            var cosLong = math.cos(lon);
            var sinLong = math.sin(lon);

            var c = 1 / math.sqrt(cosLat * cosLat +
                                  (1 - InverseFlattening) * (1 - InverseFlattening) * sinLat * sinLat);
            var s = (1 - InverseFlattening) * (1 - InverseFlattening) * c;

            var x = (EquatorialRadius * c + alt) * cosLat * cosLong;
            var y = (EquatorialRadius * c + alt) * cosLat * sinLong;
            var z = (EquatorialRadius * s + alt) * sinLat;

            return new double3(x, y, z);
        }

        /// <summary>
        /// Converts ECEF coordinates to LLA (WGS84) coordinates
        /// </summary>
        /// <param name="ecef">The ECEF coordinates as a <see cref="double3"/></param>
        /// <returns>The <see cref="GlobePoint"/> at the given ECEF coordinates</returns>
        public static GlobePoint ToGlobePoint(this double3 ecef)
        {
            var ea = math.sqrt((EquatorialRadius * EquatorialRadius - PolarRadius * PolarRadius) /
                               (EquatorialRadius * EquatorialRadius));
            var eb = math.sqrt((EquatorialRadius * EquatorialRadius - PolarRadius * PolarRadius) /
                               (PolarRadius * PolarRadius));
            var p = math.sqrt(ecef.x * ecef.x + ecef.y * ecef.y);

            var theta = math.atan2(ecef.z * EquatorialRadius, p * PolarRadius);
            var lon = math.atan2(ecef.y, ecef.x);
            var lat = math.atan2(ecef.z + eb * eb * PolarRadius * math.pow(math.sin(theta), 3),
                p - ea * ea * EquatorialRadius * math.pow(math.cos(theta), 3));
            var n = EquatorialRadius / math.sqrt(1 - ea * ea * math.sin(lat) * math.sin(lat));
            var alt = p / math.cos(lat) - n;

            return new GlobePoint(lat * (180.0 / math.PI), lon * (180.0 / math.PI), alt);
        }
    }
}