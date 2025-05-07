using System.Threading;
using System.Threading.Tasks;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.View.Rendering;

namespace GeoViewer.Controller.DataLayers
{
    /// <summary>
    /// An interface containing behaviours for requesting and rendering data for a layer.
    /// </summary>
    /// <typeparam name="T">The type of data to request and render</typeparam>
    public interface IDataRequest<T>
    {
        /// <summary>
        /// Requests the data for the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The area to request data for</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>A Task with the requested data, or <see langword="default"/> if the request failed</returns>
        public Task<T> RequestData((TileId tileId, GlobeArea area) request, CancellationToken token);

        /// <summary>
        /// Renders requested data onto the given <paramref name="tileGameObject"/>.
        /// </summary>
        /// <param name="data">The data to render. This should be the result of <see cref="RequestData"/></param>
        /// <param name="tileGameObject">The <see cref="TileGameObject"/> to render onto</param>
        /// <param name="mapRenderer">The <see cref="MapRenderer"/> to render for</param>
        public void RenderData(T data, TileGameObject tileGameObject, MapRenderer mapRenderer);
    }
}