using System;
using System.Net.Http;

namespace GeoViewer.Controller.Networking
{
    /// <summary>
    /// An utility class for creating <see cref="HttpClient"/>s
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        /// Stores an application-wide handler
        /// </summary>
        private static readonly HttpClientHandler Handler = new();

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> with a persistent handler.
        /// </summary>
        /// <param name="baseAddress">The base address for the client</param>
        /// <returns>A new <see cref="HttpClient"/></returns>
        public static HttpClient CreateClient(Uri? baseAddress = null)
        {
            return new HttpClient(Handler)
            {
                BaseAddress = baseAddress
            };
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> with a persistent handler for Osm requests.
        /// This sets the user-agent, which is required to use Osm.
        /// </summary>
        /// <param name="baseAddress">The base address for the client</param>
        /// <returns>A new <see cref="HttpClient"/></returns>
        public static HttpClient CreateOsmClient(Uri? baseAddress = null)
        {
            return new HttpClient(new OsmClientHandler(Handler))
            {
                BaseAddress = baseAddress
            };
        }
    }
}