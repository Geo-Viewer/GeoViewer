using System.Collections.Generic;
using System.Linq;

namespace GeoViewer.Model.Globe
{
    public interface IGlobeMask
    {
        public GlobePoint[] Points { get; }
        public bool Contains(GlobePoint point);

        public bool Intersects(IGlobeMask globeMask)
        {
            return globeMask.Points.Any(Contains);
        }

        public bool Contains(IGlobeMask globeMask)
        {
            return globeMask.Points.All(Contains);
        }
    }
}