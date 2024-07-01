using Unity.Mathematics;
using UnityEngine;

namespace GeoViewer.Controller.Util
{
    /// <summary>
    /// A helper class for converting double vectors from Unity.Mathematics
    /// </summary>
    public static class DoubleUtil
    {
        /// <summary>
        /// Converts a <see cref="double3"/> to a <see cref="Vector3"/>.
        /// </summary>
        /// <param name="vector">the <see cref="double3"/> to convert</param>
        /// <returns>a <see cref="Vector3"/></returns>
        public static Vector3 ToVector3(this double3 vector)
        {
            return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        }

        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="double3"/>.
        /// </summary>
        /// <param name="vector">the <see cref="Vector3"/> to convert</param>
        /// <returns>a <see cref="double3"/></returns>
        public static double3 ToDouble3(this Vector3 vector)
        {
            return new double3(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="double3"/>.
        /// </summary>
        /// <param name="vector">the <see cref="Vector2"/> to convert</param>
        /// <returns>a <see cref="double3"/></returns>
        public static double3 ToDouble3(this Vector2 vector)
        {
            return new double3(vector.x, vector.y, 0);
        }
    }
}