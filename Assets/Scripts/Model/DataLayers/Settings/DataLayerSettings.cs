using GeoViewer.Controller.DataLayers;
using JsonSubTypes;
using Newtonsoft.Json;

namespace GeoViewer.Model.DataLayers.Settings
{
    /// <summary>
    /// Base class for all data layer settings
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(BaseTextureLayerSettings), "BaseTexture")]
    [JsonSubtypes.KnownSubType(typeof(BaseMeshLayerSettings), "BaseMesh")]
    [JsonSubtypes.KnownSubType(typeof(OsmTextureLayerSettings), "OsmTexture")]
    [JsonSubtypes.KnownSubType(typeof(BingTextureLayerSettings), "BingTexture")]
    [JsonSubtypes.KnownSubType(typeof(OtdMeshLayerSettings), "OtdMesh")]
    [JsonSubtypes.KnownSubType(typeof(TopoSharpMeshLayerSettings), "TopoSharpMesh")]
    public abstract class DataLayerSettings
    {
        public abstract string Type { get; }

        /// <summary>
        /// The display name of the layer
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// The display priority for this layer (highest priority layer will be rendered)
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        /// The amount of requests that can be processed in parallel
        /// </summary>
        public int ParallelRequests { get; set; } = 5;

        /// <summary>
        /// The amount of requests that can be processed per second. Negative values mean unlimited
        /// </summary>
        public int RequestsPerSecond { get; set; } = -1;

        /// <summary>
        /// The amount of request results, the cache can store at once. Only the first results will be cached
        /// </summary>
        public int CacheSize { get; set; } = 0;

        /// <summary>
        /// Creates an <see cref="IDataLayer"/> based on this
        /// </summary>
        /// <returns>An <see cref="IDataLayer"/> with these settings</returns>
        public abstract IDataLayer CreateDataLayer();

        /// <summary>
        /// Checks whether these settings are valid
        /// </summary>
        /// <returns><c>true</c>, if the settings are valid, <c>false</c> otherwise</returns>
        public virtual bool Validate()
        {
            return Priority > 0 && ParallelRequests > 0 && Name.Length > 0;
        }
    }
}