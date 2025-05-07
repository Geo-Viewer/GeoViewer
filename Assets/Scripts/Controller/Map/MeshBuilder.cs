using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GeoViewer.Controller.Map
{
    public static class MeshBuilder
    {
        /// <summary>
        /// Generates a new tile mesh out of a given point grid
        /// </summary>
        /// <param name="vertices">The positions of the vertices (relative to grid midpoint)</param>
        /// <returns>A <see cref="Mesh"/> generated out of the given vertices</returns>
        /// <exception cref="ArgumentException">Thrown if the given vertex grid is not a square</exception>
        public static Mesh BuildMesh(ICollection<Vector3> vertices)
        {
            var resolution = Math.Sqrt(vertices.Count);

            if (resolution % 1 != 0)
            {
                throw new ArgumentException("Vertices have to be a square grid.");
            }

            var mesh = new Mesh
            {
                name = "Tile",
                vertices = vertices.ToArray(),
                triangles = CalculateTris(vertices, (int)resolution).ToArray()
            };

            mesh.uv = mesh.CalculateUVs().ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Calculates the triangles of a tile <see cref="Mesh"/> for the given vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the <see cref="Mesh"/></param>
        /// <param name="resolution">The resolution of the <see cref="Mesh"/></param>
        /// <returns>The triangles as an <see cref="IEnumerable{T}"/> of ints</returns>
        private static IEnumerable<int> CalculateTris(ICollection<Vector3> vertices, int resolution)
        {
            for (var i = 0; i < vertices.Count; i++)
            {
                //check whether vertex is at bottom left of a quad
                //if it is, add tris so that a quad will be created with this vertex at the bottom left corner
                if (i % resolution >= resolution - 1 || i + resolution >= vertices.Count)
                {
                    continue;
                }

                yield return i;
                yield return i + resolution;
                yield return i + 1;

                yield return i + resolution + 1;
                yield return i + 1;
                yield return i + resolution;
            }
        }

        /// <summary>
        /// Calculates the uvs, so that a square texture with scale 1 will fill the whole <see cref="Mesh"/>.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> to calculate the uvs for</param>
        /// <returns>The uv coordinates as an <see cref="IEnumerable{T}"/> of ints</returns>
        private static IEnumerable<Vector2> CalculateUVs(this Mesh mesh)
        {
            var size = mesh.bounds.size;
            var min = mesh.bounds.min;
            for (var i = 0; i < mesh.vertices.Length; i++)
            {
                yield return new Vector2((mesh.vertices[i].x - min.x) / size.x, (mesh.vertices[i].z - min.z) / size.z);
            }
        }
    }
}