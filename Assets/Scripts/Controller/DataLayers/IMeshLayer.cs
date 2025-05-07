using System.Collections.Generic;
using GeoViewer.Model.Globe;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// An interface for a mesh layer
    /// </summary>
    public interface IMeshLayer : IDataLayer, IDataRequest<IReadOnlyList<GlobePoint>>
    {
    }
}