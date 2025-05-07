using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class OtdMeshLayerSettings : MeshLayerSettings
    {
        public override string Type { get; } = "OtdMesh";

        /// <summary>
        /// The path to the heightdata provider
        /// </summary>
        public string Url { get; set; } = "https://api.opentopodata.org/v1/aster30m";

        /// <summary>
        /// The interpolation to use for the heightdata
        /// </summary>
        public OtdInterpolation Interpolation { get; set; } = OtdInterpolation.Cubic;


        public OtdMeshLayerSettings()
        {
            ParallelRequests = 1;
            RequestsPerSecond = 1;
        }

        public override IDataLayer CreateDataLayer()
        {
            return new OtdMeshLayer(this);
        }
    }
}