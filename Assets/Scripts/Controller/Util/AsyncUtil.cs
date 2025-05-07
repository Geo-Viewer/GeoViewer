using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeoViewer.Controller.Util
{
    /// <summary>
    /// An util class for executing asynchronous functions in parallel.
    /// </summary>
    public static class AsyncUtil
    {
        /// <summary>
        /// An extension method for executing a given Function for every element in a given enumerable asynchronously and parallel.
        /// </summary>
        /// <param name="enumerable">The enumerable containing function parameter as elements</param>
        /// <param name="method">The method to execute</param>
        /// <typeparam name="T">The data type for enumerator and function parameter</typeparam>
        public static async Task ExecuteAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> method)
        {
            await Task.WhenAll(enumerable.Select(method));
        }
    }
}