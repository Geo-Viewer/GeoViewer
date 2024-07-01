using GeoViewer.Controller.DataLayers;

namespace GeoViewer.Model.DataLayers.Settings
{
    public class BaseMeshLayerSettings : MeshLayerSettings
    {
        public override string Type { get; } = "BaseMesh";

        public BaseMeshLayerSettings()
        {
            MeshResolution = 2;
            CacheSize = 0;
        }

        public override IDataLayer CreateDataLayer()
        {
            return new BaseMeshLayer(this);
        }
    }
}