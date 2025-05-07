using GeoViewer.Controller.Map.Projection;
using GeoViewer.Model.Globe;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace GeoViewer.Test.Editor.Controller.Map.Projection
{
    /// <summary>
    /// Values are from https://epsg.io/transform#s_srs=4979&t_srs=3857&x=NaN&y=NaN
    /// Position Error is set quite high, because output is not EPSG:900913
    /// so we only guarantee accuracy of 1m
    /// </summary>
    public class WebMercatorProjectionTest
    {
        private readonly WebMercatorProjection _webMercatorProjection = new();

        private const double PositionError = 1;

        [Test]
        public void WebMercatorGlobePointToPositionTests()
        {
            //Basic Test
            Assert.True(WebMercatorGlobePointToPositionTest(
                new GlobePoint(40, 8),
                new double3(890555.9263461885, 0, 4865942.279503175)));
            //Negative Test
            Assert.True(WebMercatorGlobePointToPositionTest(new GlobePoint(-80.12345, -15.12345),
                new double3(-1683534.7530375333, 0, -15618337.927281441)));
            //Height Test
            Assert.True(WebMercatorGlobePointToPositionTest(new GlobePoint(69.696969, -42.424242, 100),
                new double3(-4722645.01673061, 288.196646722, 10970795.49374641)));
        }

        [Test]
        public void WebMercatorPositionToGlobePointTests()
        {
            //Basic Test
            Assert.True(WebMercatorPositionToGlobePointTest(
                new double3(890555.9263461885, 0, 4865942.279503175),
                new GlobePoint(40, 8)));
            //Negative Test
            Assert.True(WebMercatorPositionToGlobePointTest(new double3(-1683534.7530375333, 0, -15618337.927281441),
                new GlobePoint(-80.12345, -15.12345)));
            //Height Test
            Assert.True(WebMercatorPositionToGlobePointTest(
                new double3(-4722645.01673061, 288.196646722, 10970795.49374641),
                new GlobePoint(69.696969, -42.424242, 100)));
        }

        [Test]
        public void WebMercatorGlobePointToTileCoordinateTest()
        {
            //Basic Test
            GlobePoint point1 = new(8, 8, 8);
            Assert.AreEqual(new Vector2Int(0, 0),
                _webMercatorProjection.GlobePointToTileCoordinates(point1, 0));

            //koeri
            GlobePoint point2 = new(49.01192862399907, 8.416472306613555);
            Assert.AreEqual(new Vector2Int(274401, 180025),
                _webMercatorProjection.GlobePointToTileCoordinates(point2, 19));
        }

        private bool WebMercatorGlobePointToPositionTest(GlobePoint point, double3 expectedPosition)
        {
            var position = _webMercatorProjection.GlobePointToPosition(point);
            return math.distance(position, expectedPosition) <= PositionError;
        }

        private bool WebMercatorPositionToGlobePointTest(double3 position, GlobePoint expectedGlobePoint)
        {
            var point = _webMercatorProjection.PositionToGlobePoint(position);
            //Position Error doubled, because 2 conversions
            return math.distance(_webMercatorProjection.GlobePointToPosition(point),
                _webMercatorProjection.GlobePointToPosition(expectedGlobePoint)) <= PositionError * 2;
        }
    }
}