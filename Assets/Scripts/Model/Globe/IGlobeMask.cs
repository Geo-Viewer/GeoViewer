using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeoViewer.Model.Globe
{
    public interface IGlobeMask
    {
        public GlobePoint[] Points { get; }
        public bool Contains(GlobePoint point);

        public bool Intersects(IGlobeMask globeMask)
        {
            return globeMask.Points.Any(Contains) || Points.Any(globeMask.Contains);
        }

        public bool Contains(IGlobeMask globeMask)
        {
            return globeMask.Points.All(Contains);
        }

        public void ScaleAround(GlobePoint pivot, float factor)
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new GlobePoint(pivot.Latitude + (Points[i].Latitude - pivot.Latitude) * factor,
                    pivot.Longitude + (Points[i].Longitude - pivot.Longitude) * factor, Points[i].Altitude);
            }
        }
    }
}