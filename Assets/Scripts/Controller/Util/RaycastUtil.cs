using System.Collections.Generic;
using GeoViewer.Controller.Input;
using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.Controller.Util
{
    /// <summary>
    /// A helper class for ray casts.
    /// </summary>
    public static class RaycastUtil
    {
        private static readonly Inputs Inputs;

        static RaycastUtil()
        {
            Inputs = ApplicationState.Instance.Inputs;
        }

        /// <summary>
        /// The method does a ray cast from the position of the cursor, it returns whether it was successful.
        /// </summary>
        /// <param name="layer">the layer to use in the ray cast</param>
        /// <param name="hit">the information about the ray cast</param>
        /// <returns>true if the ray cast hit otherwise false</returns>
        public static bool GetCursorRaycastHit(int layer, out RaycastHit hit)
        {
            if (!Inputs.MousePosition.HasValue || ApplicationState.Instance.Camera is null)
            {
                hit = new RaycastHit();
                return false;
            }

            var ray = ApplicationState.Instance.Camera.ScreenPointToRay(Inputs.MousePosition.Value);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << layer);
        }

        /// <summary>
        /// The method does a ray cast from the position of the cursor, it returns whether it was successful.
        /// </summary>
        /// <param name="layers">A list of layers to use in the ray cast</param>
        /// <param name="hit">the information about the ray cast</param>
        /// <returns>true if the ray cast hit otherwise false</returns>
        public static bool GetCursorRaycastHit(List<int> layers, out RaycastHit hit)
        {
            if (!Inputs.MousePosition.HasValue || ApplicationState.Instance.Camera is null)
            {
                hit = new RaycastHit();
                return false;
            }

            var ray = ApplicationState.Instance.Camera.ScreenPointToRay(Inputs.MousePosition.Value);
            var layerMask = BuildLayerMask(layers);
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        private static int BuildLayerMask(List<int> layers)
        {
            var layerMask = 0;
            foreach (var layer in layers)
            {
                layerMask |= 1 << layer;
            }

            return layerMask;
        }
    }
}