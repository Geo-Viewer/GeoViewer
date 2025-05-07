using System;
using GeoViewer.Model.Globe;
using UnityEngine.UIElements;

namespace GeoViewer.View.UI.InformationBox
{
    /// <summary>
    /// Designed as an entry in the information box, this class displays coordinates.
    /// </summary>>
    public class CoordinateDisplay : VisualElement
    {
        private readonly Label _latitude;
        private readonly Label _longitude;
        private readonly Label _altitude;
        private const string LatitudeIndicator = "Latitude: ";
        private const string LongitudeIndicator = "Longitude: ";
        private const string AltitudeIndicator = "Altitude: ";

        /// <summary>
        /// Creates a heading for the coordinates at the cursor.
        /// </summary>
        public CoordinateDisplay()
        {
            var heading = new Label("Coordinates at cursor");
            heading.AddToClassList("section-heading");
            Add(heading);
            _latitude = new Label(LatitudeIndicator);
            _longitude = new Label(LongitudeIndicator);
            _altitude = new Label(AltitudeIndicator);
            Add(_latitude);
            Add(_longitude);
            Add(_altitude);
        }

        /// <summary>
        /// Displays the given coordinates.
        /// </summary>
        /// <param name="coordinates">The new coordinates</param>
        public void SetCoordinates(GlobePoint coordinates)
        {
            _latitude.text = LatitudeIndicator + coordinates.DmsLatitude;
            _longitude.text = LongitudeIndicator + coordinates.DmsLongitude;
            _altitude.text = AltitudeIndicator + Math.Round(coordinates.Altitude, 2) + "m";
        }
    }
}