using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace Direct3D11_RenderingCubeAndSphere
{
    public static class GeometricPrimitives
    {
        /// <summary>
        /// Creates a sphere primitive.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="diameter">The diameter.</param>
        /// <param name="tessellation">The tessellation.</param>
        /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is true.</param>
        /// <returns>A sphere primitive.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">tessellation;Must be >= 3</exception>
        public static void GenerateSphere(out Vertex[] vertices, out int[] indices, Color color, float radius = 0.5f, int tessellation = 16, bool clockWiseWinding = true)
        {
            if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation", "Must be >= 3");

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            vertices = new Vertex[(verticalSegments + 1) * (horizontalSegments + 1)];
            indices = new int[(verticalSegments) * (horizontalSegments + 1) * 6];

            int vertexCount = 0;
            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i <= verticalSegments; i++)
            {
                float v = 1.0f - (float)i / verticalSegments;

                var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
                var dy = (float)Math.Sin(latitude);
                var dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float u = (float)j / horizontalSegments;

                    var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
                    var dx = (float)Math.Sin(longitude);
                    var dz = (float)Math.Cos(longitude);

                    dx *= dxz;
                    dz *= dxz;

                    var normal = new Vector3(dx, dy, dz);
                    var position = normal * radius;
                    // To generate a UV texture coordinate:
                    //var textureCoordinate = new Vector2(u, v);
                    // To generate a UVW texture cube coordinate
                    //var textureCoordinate = normal;

                    vertices[vertexCount++] = new Vertex(position, normal, color);
                }
            }

            // Fill the index buffer with triangles joining each pair of latitude rings.
            int stride = horizontalSegments + 1;

            int indexCount = 0;
            for (int i = 0; i < verticalSegments; i++)
            {
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % stride;

                    indices[indexCount++] = (i * stride + j);
                    // Implement correct winding of vertices
                    if (clockWiseWinding)
                    {
                        indices[indexCount++] = (i * stride + nextJ);
                        indices[indexCount++] = (nextI * stride + j);
                    }
                    else
                    {
                        indices[indexCount++] = (nextI * stride + j);
                        indices[indexCount++] = (i * stride + nextJ);
                    }

                    indices[indexCount++] = (i * stride + nextJ);
                    // Implement correct winding of vertices
                    if (clockWiseWinding)
                    {
                        indices[indexCount++] = (nextI * stride + nextJ);
                        indices[indexCount++] = (nextI * stride + j);
                    }
                    else
                    {
                        indices[indexCount++] = (nextI * stride + j);
                        indices[indexCount++] = (nextI * stride + nextJ);
                    }
                }
            }
        }
    }
}
