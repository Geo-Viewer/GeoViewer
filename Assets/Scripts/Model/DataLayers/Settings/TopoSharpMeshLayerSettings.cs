using System;
using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class TopoSharpMeshLayerSettings : MeshLayerSettings
    {
        public override string Type { get; } = "TopoSharpMesh";

        /// <summary>
        /// The url to the heightdata server. Has to contain {minlat}, {maxlat}, {minlon}, {maxlon} and {resolution}
        /// </summary>
        public string Url { get; set; } =
            "http://localhost:5125/v2/ArcGis?minlat={minlat}&maxlat={maxlat}&minlon={minlon}&maxlon={maxlon}&resolution={resolution}&interpolation=cubic";

        public TopoSharpMeshLayerSettings()
        {
            ParallelRequests = 5;
        }

        public override bool Validate()
        {
            return base.Validate()
                   && Url.Contains(TopoSharpMeshLayer.MinLatIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(TopoSharpMeshLayer.MaxLatIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(TopoSharpMeshLayer.MinLonIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(TopoSharpMeshLayer.MaxLonIdentifier, StringComparison.OrdinalIgnoreCase)
                   && Url.Contains(TopoSharpMeshLayer.ResolutionIdentifier, StringComparison.OrdinalIgnoreCase);
        }

        public override IDataLayer CreateDataLayer()
        {
            return new TopoSharpMeshLayer(this);
        }
    }
}