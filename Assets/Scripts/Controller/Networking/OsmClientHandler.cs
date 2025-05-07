using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GeoViewer.Controller.Networking
{
    /// <summary>
    /// A class wrapping a handler for Osm requests.
    /// </summary>
    public class OsmClientHandler : DelegatingHandler
    {
        /// <summary>
        /// Creates a new <see cref="OsmClientHandler"/>.
        /// </summary>
        /// <param name="handler">The handler to wrap</param>
        public OsmClientHandler(HttpClientHandler handler)
        {
            InnerHandler = handler;
        }

        /// <summary>
        /// Intercepts web requests from this handler and sets the required headers
        /// </summary>
        /// <param name="request">The http request which is to be performed.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            request.Headers.Add("user-agent", "GeoViewerTesting");
#else
            request.Headers.Add("user-agent", "GeoViewerRelease");
#endif
            return base.SendAsync(request, cancellationToken);
        }
    }
}