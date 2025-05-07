using GeoViewer.Controller.Util;
using GeoViewer.Model.Globe;
using NUnit.Framework;
using Unity.Mathematics;

namespace GeoViewer.Test.Editor.Controller.Util
{
    /// <summary>
    /// uses converted values from https://astroconverter.com/xyzllh.html
    /// </summary>
    public class EcefConverterTest
    {
        private const float DegreeError = 0.001f;
        private const float DistanceError = 2f;

        [Test]
        public void EcefToGlobePointTest1()
        {
            var calculatedPoint = EcefConverter.ToGlobePoint(new double3(4147204.614, 606567.079, 4791650.573));
            var expectedPoint = new GlobePoint(49.0140, 8.3210, 97.9306);
            Assert.GreaterOrEqual(DegreeError, GlobePointDistance(calculatedPoint, expectedPoint));
        }

        [Test]
        public void EcefToGlobePointTest2()
        {
            var calculatedPoint = EcefConverter.ToGlobePoint(new double3(3963989.3590, 482828.5150, 4956881.5350));
            var expectedPoint = new GlobePoint(51.3328, 6.9446, 154.2695);
            Assert.GreaterOrEqual(DegreeError, GlobePointDistance(calculatedPoint, expectedPoint));
        }

        [Test]
        public void GlobePointToEcefTest()
        {
            var calculatedPoint = EcefConverter.GlobePointToEcef(new GlobePoint(51.3328, 6.9446, 154.2695));
            var expectedPoint = new double3(3963989.3590, 482828.5150, 4956881.5350);
            Assert.GreaterOrEqual(DistanceError, math.distance(calculatedPoint, expectedPoint));
        }

        private double GlobePointDistance(GlobePoint point1, GlobePoint point2)
        {
            return math.distance(point1.Latitude, point2.Latitude)
                   + math.distance(point1.Longitude, point2.Longitude)
                   + math.distance(point1.Altitude, point2.Altitude);
        }
    }
}