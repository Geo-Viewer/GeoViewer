namespace GeoViewer.Model.DataLayers.Settings
{
    public abstract class MeshLayerSettings : DataLayerSettings
    {
        /// <summary>
        /// The amount of vertices at each edge of the grid. Has to be at least 2
        /// </summary>
        public int MeshResolution { get; set; } = 10;

        /// <inheritdoc/>
        public override bool Validate()
        {
            return MeshResolution > 1 && base.Validate();
        }
    }
}