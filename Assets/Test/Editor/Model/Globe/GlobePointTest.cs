using GeoViewer.Model.Globe;
using NUnit.Framework;

namespace GeoViewer.Test.Editor.Model.Globe
{
    /// <summary>
    /// values from https://www.calculatorsoup.com/calculators/conversions/convert-decimal-degrees-to-degrees-minutes-seconds.php
    /// </summary>
    public class GlobePointTest
    {
        [TestCaseSource(nameof(DegreeToDmsInputs))]
        public void DegreeToDmsTest(string latitude, string longitude, GlobePoint globePoint)
        {
            Assert.AreEqual(latitude, globePoint.DmsLatitude);
            Assert.AreEqual(longitude, globePoint.DmsLongitude);
        }

        public static object[] DegreeToDmsInputs =
        {
            new object[] { "45° 14' 02'' N", "23° 13' 48'' E", new GlobePoint(45.234, 23.23, 24.78) },
            new object[] { "74° 07' 22'' N", "13° 51' 35'' W", new GlobePoint(74.123, -13.86, 234.32) },
            new object[] { "52° 25' 47'' S", "54° 45' 35'' W", new GlobePoint(-52.43, -54.76, 24.78) }
        };
    }
}