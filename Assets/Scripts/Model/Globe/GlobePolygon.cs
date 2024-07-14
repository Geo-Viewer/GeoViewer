using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoViewer.Model.Globe
{
    public class GlobePolygon : IGlobeMask
    {
        public GlobePoint[] Points { get; }

        public GlobeArea BoundingBox =>
            new GlobeArea(new GlobePoint(Points.Max(x => x.Latitude), Points.Max(x => x.Longitude)),
                new GlobePoint(Points.Min(x => x.Latitude), Points.Min(x => x.Longitude)));

        public GlobePolygon(GlobePoint[] points)
        {
            Points = points;
        }

        private const float ComparisonTolerance = 0.001f;

        public bool Contains(GlobePoint point)
        {
            bool result = false;
            var a = Points.Last();
            foreach (var b in Points)
            {
                if ((Math.Abs(b.Latitude - point.Latitude) < ComparisonTolerance) &&
                    Math.Abs(b.Longitude - point.Longitude) < ComparisonTolerance)
                {
                    return true;
                }

                if (Math.Abs(b.Longitude - a.Longitude) < ComparisonTolerance &&
                    Math.Abs(point.Longitude - a.Longitude) < ComparisonTolerance)
                {
                    if ((a.Latitude <= point.Latitude) && (point.Latitude <= b.Latitude))
                        return true;

                    if ((b.Latitude <= point.Latitude) && (point.Latitude <= a.Latitude))
                        return true;
                }

                if ((b.Longitude < point.Longitude) && (a.Longitude >= point.Longitude) ||
                    (a.Longitude < point.Longitude) && (b.Longitude >= point.Longitude))
                {
                    if (b.Latitude + (point.Longitude - b.Longitude) / (a.Longitude - b.Longitude) *
                        (a.Latitude - b.Latitude) <= point.Latitude)
                    {
                        result = !result;
                    }
                }

                a = b;
            }

            return result;
        }
    }
}