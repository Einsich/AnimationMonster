﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpGL.VertexBuffers;
using SharpGL;
using Assimp;
namespace Engine
{
    public class ProcessedMesh
    {
        public int vertexCount { get; private set; }
        public bool hasUvs { get; private set; } = false;
        public VertexBufferArray vertexBufferArray;
        public Dictionary<string, int> nodeMap;
        public float[] boneTransform;
        public ProcessedMesh(List<Face> Faces, List<Vector3D> Vertices, List<Vector3D> Normals, List<Vector3D> uv)
        {
            OpenGL gl = GLContainer.OpenGL;

            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            var verticesVertexBuffer = new VertexBuffer();
            verticesVertexBuffer.Create(gl);
            verticesVertexBuffer.Bind(gl);
            int stride = 3;


            verticesVertexBuffer.SetData(gl, VertexAttributes.Position,
                                 Extract(Faces, Vertices, 3),
                                 false, stride);
            if (Normals != null)
            {
                var normalsVertexBuffer = new VertexBuffer();
                normalsVertexBuffer.Create(gl);
                normalsVertexBuffer.Bind(gl);
                stride = 3;
                normalsVertexBuffer.SetData(gl, VertexAttributes.Normal,
                                             Extract(Faces, Normals, 3),
                                            false, stride);
                normalsVertexBuffer.Unbind(gl);
            }

            if (uv != null)
            {
                hasUvs = true;
                var texCoordsVertexBuffer = new VertexBuffer();
                texCoordsVertexBuffer.Create(gl);
                texCoordsVertexBuffer.Bind(gl);
                stride = 2;
                texCoordsVertexBuffer.SetData(gl, VertexAttributes.TexCoord,
                                               Extract(Faces, uv, stride),
                                              false, stride);
                texCoordsVertexBuffer.Unbind(gl);
            }

            vertexCount = Faces.Count * 3;
            verticesVertexBuffer.Unbind(gl);
        }
        public ProcessedMesh(Scene scene, int index)
        {
            Mesh mesh = scene.Meshes[index];
            OpenGL gl = GLContainer.OpenGL;
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            var verticesVertexBuffer = new VertexBuffer();
            verticesVertexBuffer.Create(gl);
            verticesVertexBuffer.Bind(gl);
            int stride = 3;


            verticesVertexBuffer.SetData(gl, VertexAttributes.Position,
                                 Extract(mesh.Faces, mesh.Vertices, 3),
                                 false, stride);
            if (mesh.HasNormals)
            {
                var normalsVertexBuffer = new VertexBuffer();
                normalsVertexBuffer.Create(gl);
                normalsVertexBuffer.Bind(gl);
                stride = 3;
                normalsVertexBuffer.SetData(gl, VertexAttributes.Normal,
                                             Extract(mesh.Faces, mesh.Normals, 3),
                                            false, stride);
                normalsVertexBuffer.Unbind(gl);
            }

            if (mesh.HasTextureCoords(0))
            {
                hasUvs = true;
                var texCoordsVertexBuffer = new VertexBuffer();
                texCoordsVertexBuffer.Create(gl);
                texCoordsVertexBuffer.Bind(gl);
                stride = mesh.UVComponentCount[0];
                texCoordsVertexBuffer.SetData(gl, VertexAttributes.TexCoord,
                                               Extract(mesh.Faces, mesh.TextureCoordinateChannels[0], stride),
                                              false, stride);
                texCoordsVertexBuffer.Unbind(gl);
            }
            if (mesh.HasBones)
            {
                Vector4D[] weights = new Vector4D[mesh.VertexCount];
                Vector4D[] boneInd = new Vector4D[mesh.VertexCount];
                nodeMap = new Dictionary<string, int>();
                boneTransform = new float[16 * mesh.BoneCount];
                Dictionary<int, List<Vector2D>> stat = new Dictionary<int, List<Vector2D>>();
                for (int i = 0; i < mesh.BoneCount; i++)
                {

                    nodeMap.Add(mesh.Bones[i].Name, i);

                    for (int j = 0; j < mesh.Bones[i].VertexWeightCount; j++)
                    {
                        var w = mesh.Bones[i].VertexWeights[j];
                        if (!stat.ContainsKey(w.VertexID))
                            stat.Add(w.VertexID, new List<Vector2D>());
                        stat[w.VertexID].Add(new Vector2D(w.Weight, i));
                    }
                }
                AddWeight(weights, boneInd, stat);
                var weightsVertexBuffer = new VertexBuffer();
                weightsVertexBuffer.Create(gl);
                weightsVertexBuffer.Bind(gl);
                stride = 4;
                weightsVertexBuffer.SetData(gl, VertexAttributes.BoneWeights, Extract(mesh.Faces, weights, 4), false, stride);
                weightsVertexBuffer.Unbind(gl);

                var boneIdVertexBuffer = new VertexBuffer();
                boneIdVertexBuffer.Create(gl);
                boneIdVertexBuffer.Bind(gl);
                stride = 4;
                boneIdVertexBuffer.SetData(gl, VertexAttributes.BoneIndex, Extract(mesh.Faces, boneInd, 4), false, stride);
                boneIdVertexBuffer.Unbind(gl);


            }
            vertexCount = mesh.FaceCount * 3;
            verticesVertexBuffer.Unbind(gl);
        }
        static void AddWeight(Vector4D[] weights, Vector4D[] bones, Dictionary<int, List<Vector2D>> pack)
        {
            foreach (var p in pack)
            {
                int j = p.Key;
                var list = p.Value;
                Vector4D ws = new Vector4D();
                Vector4D bs = new Vector4D();
                for (int i = 0; i < list.Count; i++)
                {
                    ws[i] = list[i].X;
                    bs[i] = list[i].Y;
                }
                float t = ws[0] + ws[1] + ws[2] + ws[3];
                for (int i = 0; i < 4; i++)
                    ws[i] /= t;
                weights[j] = ws;
                bones[j] = bs;
            }
        }
        static float[] Extract(List<Face> faces, Vector4D[] data, int stride)
        {
            float[] output = new float[stride * faces.Count * 3];
            for (int i = 0; i < faces.Count; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int j = (i * 3 + k) * stride;
                    Vector4D v = data[faces[i].Indices[k]];
                    for (int p = 0; p < stride; p++)
                        output[j + p] = v[p];
                }
            }
            return output;
        }
        static float[] Extract(List<Face> faces, List<Vector3D> data, int stride)
        {
            float[] output = new float[stride * faces.Count * 3];
            for (int i = 0; i < faces.Count; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int j = (i * 3 + k) * stride;
                    Vector4D v = new Vector4D(data[faces[i].Indices[k]]);
                    for (int p = 0; p < stride; p++)
                        output[j + p] = v[p];
                }
            }
            return output;
        }
        private Dictionary<string, int> boneMap;
        private List<Matrix4x4> meshToBone;
        public void ProcessAnimation(AnimationRenderInfo info)
        {
            boneMap = info.boneMap;
            meshToBone = info.meshToBone;
            CalculateBonesTransform(info.root, info.animRoot, info.root.Transform, info.animation, info.time * (float)info.animation.TicksPerSecond, 0);
        }
        private void CalculateBonesTransform(Node node, Node animNode, Matrix4x4 parent, Animation animation, float time, int d)
        {
            Matrix4x4 nodeTransform = animNode != null ? animNode.Transform : node.Transform ;
            // nodeTransform = node.Transform;
            
            if (boneMap.ContainsKey(node.Name))
            {
                
                int nodeId = boneMap[node.Name];
                var chanel = animation.NodeAnimationChannels[nodeId];
                Matrix4x4 scale = Matrix4x4.FromScaling(chanel.GetLerpedScale(time));
                Matrix4x4 rotation = new Matrix4x4(chanel.GetLerpedRotation(time).GetMatrix());
                Matrix4x4 translation = Matrix4x4.FromTranslation(chanel.GetLerpedPosition(time));
                nodeTransform = scale * rotation * translation;
                
            }
            else
            {
                //Console.WriteLine($"!{node.Name} + {(animNode != null ? animNode.Name : "null")}");
            }
            nodeTransform = nodeTransform * parent;

            BoneRender.AddBones(parent, nodeTransform);

            if (nodeMap.ContainsKey(node.Name))
            {
                int bone = nodeMap[node.Name];

                WriteMatrix(boneTransform, bone, meshToBone[bone] * nodeTransform);
            }
            for (int i = 0; i < node.ChildCount; i++)
            {
                /*
                Node nextAnimNode = animNode != null && i < animNode.ChildCount ? animNode.Children[i] : null;
                if (d < 2 || node.Name == "Spine3")
                    nextAnimNode = animNode;
                else
                    if (node.Name == "Spine4")
                    nextAnimNode = animNode.Children[(i ^ 3) % 3];
                    */
                CalculateBonesTransform(node.Children[i], BoneRender.GetNextAnimTreeChild(node, animNode, i), nodeTransform, animation, time, d + 1);

            }
        }
        private void WriteMatrix(float[] array, int ind, Matrix4x4 m)
        {
            ind *= 16;
            (array[ind], array[ind + 1], array[ind + 2], array[ind + 3],
             array[ind + 4], array[ind + 5], array[ind + 6], array[ind + 7],
             array[ind + 8], array[ind + 9], array[ind + 10], array[ind + 11],
             array[ind + 12], array[ind + 13], array[ind + 14], array[ind + 15]) =
             (m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2,
             m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);
        }
        static ProcessedMesh cube, arrow;
        public static ProcessedMesh Cube()
        {
            if (cube == null)
            {
                List<Vector3D> vertecs = new List<Vector3D>();
                List<Vector3D> normals = new List<Vector3D>();
                List<Face> faces = new List<Face>();
                for (int face = 0; face < 3; face++)
                    for (int d = -1; d <= 1; d += 2)
                    {
                        Vector3D normal = new Vector3D();
                        normal[face] = d;
                        int ind = vertecs.Count;
                        float a = -1, b = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3D v = new Vector3D();
                            v[face] = d;
                            v[(face + 1) % 3] = a;
                            v[(face + 2) % 3] = b;
                            (a, b) = (-b * d, a * d);
                            vertecs.Add(v * 50f);
                            normals.Add(normal);
                        }
                        faces.Add(new Face(new int[] { ind, ind + 1, ind + 2 }));
                        faces.Add(new Face(new int[] { ind, ind + 2, ind + 3 }));
                    }

                cube = new ProcessedMesh(faces, vertecs, normals, null);
            }
            return cube;
        }
        public static ProcessedMesh Arrow()
        {
            if (arrow == null)
            {
                Vector3D[] p = new Vector3D[4];
                for (int i = 0; i < 3; i++)
                {
                    float a = i * 120 * Mathf.DegToRad;
                    p[i] = new Vector3D(Mathf.Cos(a), 0, Mathf.Sin(a));
                }
                p[3] = new Vector3D(0, 1, 0);
                List<Vector3D> vertecs = new List<Vector3D>();
                List<Face> faces = new List<Face>();
                void addTriangle(Vector3D a, Vector3D b, Vector3D c)
                {
                    int ind = vertecs.Count;
                    vertecs.Add(a); vertecs.Add(b); vertecs.Add(c);
                    faces.Add(new Face(new int[] { ind, ind + 1, ind + 2 }));
                }
                addTriangle(p[0], p[2], p[1]);
                addTriangle(p[3], p[2], p[0]);
                addTriangle(p[3], p[1], p[2]);
                addTriangle(p[3], p[0], p[1]);

                arrow = new ProcessedMesh(faces, vertecs, null, null);
            }
            return arrow;
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
        public const uint BoneWeights = 3;
        public const uint BoneIndex = 4;
    }
}
