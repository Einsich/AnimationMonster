using System;
using System.Collections.Generic;
using System.Linq;
using SharpGL.VertexBuffers;
using SharpGL;
using Assimp;
namespace Engine
{
    public class ProcessedMesh
    {
        public Mesh mesh;
        public int vertexCount { get; private set; }
        public VertexBufferArray vertexBufferArray;
        public ProcessedMesh(Mesh mesh)
        {
            OpenGL gl = GLContainer.OpenGL;
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            //  Create a vertex buffer for the vertices.
            var verticesVertexBuffer = new VertexBuffer();
            verticesVertexBuffer.Create(gl);
            verticesVertexBuffer.Bind(gl);
            int stride = 3;
            vertexCount = 0;
            foreach (var t in mesh.Faces)
                vertexCount += t.IndexCount == 3 ? 3 : 6;
            verticesVertexBuffer.SetData(gl, VertexAttributes.Position,
                                 FromQuadToTriangles(mesh.Vertices, stride, mesh.Faces, vertexCount),
                                 false, stride);
            if (mesh.HasNormals)
            {
                var normalsVertexBuffer = new VertexBuffer();
                normalsVertexBuffer.Create(gl);
                normalsVertexBuffer.Bind(gl);
                stride = 3;
                normalsVertexBuffer.SetData(gl, VertexAttributes.Normal,
                                            FromQuadToTriangles(mesh.Normals, stride, mesh.Faces, vertexCount),
                                            false, stride);
                normalsVertexBuffer.Unbind(gl);
            }

            if (mesh.HasTextureCoords(0))
            {
                var texCoordsVertexBuffer = new VertexBuffer();
                texCoordsVertexBuffer.Create(gl);
                texCoordsVertexBuffer.Bind(gl);
                stride = mesh.UVComponentCount[0];
                texCoordsVertexBuffer.SetData(gl, VertexAttributes.TexCoord,
                                              FromQuadToTriangles(mesh.TextureCoordinateChannels[0], stride, mesh.Faces, vertexCount),
                                              false, stride);
                texCoordsVertexBuffer.Unbind(gl);
            }
            vertexCount *= 3;
            verticesVertexBuffer.Unbind(gl);
        }
        static float[] FromQuadToTriangles(List<Vector3D> input, int stride, List<Face> faces, int realSize)
        {
            void AddToArray(float[] array, int i, Vector3D v, int s)
            {
                if (s == 2)
                {
                    array[i] = v.X;
                    array[i + 1] = 1 - v.Y;

                }
                else
                {
                    array[i] = v.X;
                    array[i + 1] = v.Y;
                    array[i + 2] = v.Z;
                }
            }
            float[] output = new float[realSize * stride];
            for (int i = 0, j = 0, k = 0; i < faces.Count; i++)
            {
                AddToArray(output, j + 0 * stride, input[k + 0], stride);
                AddToArray(output, j + 1 * stride, input[k + 1], stride);
                AddToArray(output, j + 2 * stride, input[k + 2], stride);
                j += 3 * stride;
                if (faces[i].IndexCount == 4)
                {
                    AddToArray(output, j + 0 * stride, input[k + 0], stride);
                    AddToArray(output, j + 1 * stride, input[k + 2], stride);
                    AddToArray(output, j + 2 * stride, input[k + 3], stride);
                    j += 3 * stride;
                }
                k += faces[i].IndexCount;
            }
            return output;
        }
    }
    public class MeshRenderer
    {
        public ProcessedMesh mesh;
        
    }
    public static class VertexAttributes
    {
        public const uint Position = 0;
        public const uint Normal = 1;
        public const uint TexCoord = 2;
        public const uint TexCoord1 = 3;
        public const uint TexCoord2 = 4;
    }
}
