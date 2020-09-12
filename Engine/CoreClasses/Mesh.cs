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
        public int vertexCount { get; private set; }
        public VertexBufferArray vertexBufferArray;
        private Mesh mesh;
        public bool hasAnimation => animation != null;
        private Animation animation;
        private Dictionary<string, int> boneMap, nodeMap;
        public float[] boneTransform;
        private Node rootNode;
        public ProcessedMesh(Scene scene, int index)
        {
            mesh = scene.Meshes[index];
            rootNode = scene.RootNode;
            OpenGL gl = GLContainer.OpenGL;
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            var verticesVertexBuffer = new VertexBuffer();
            verticesVertexBuffer.Create(gl);
            verticesVertexBuffer.Bind(gl);
            int stride = 3;

            
            verticesVertexBuffer.SetData(gl, VertexAttributes.Position,
                                 Extract(mesh.Faces,mesh.Vertices,3) ,
                                 false, stride);
            if (mesh.HasNormals)
            {
                var normalsVertexBuffer = new VertexBuffer();
                normalsVertexBuffer.Create(gl);
                normalsVertexBuffer.Bind(gl);
                stride = 3;
                normalsVertexBuffer.SetData(gl, VertexAttributes.Normal,
                                             Extract(mesh.Faces, mesh.Normals,3),
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
                                               Extract(mesh.Faces, mesh.TextureCoordinateChannels[0],stride),
                                              false, stride);
                texCoordsVertexBuffer.Unbind(gl);
            }
            if (mesh.HasBones && index < scene.AnimationCount)
            {
                animation = scene.Animations[index];
                boneMap = new Dictionary<string, int>();
                nodeMap = new Dictionary<string, int>();
                for (int i = 0; i < animation.NodeAnimationChannelCount; i++)
                    nodeMap[animation.NodeAnimationChannels[i].NodeName] = i;
                Vector4D[] weights = new Vector4D[mesh.VertexCount];
                Vector4D[] boneInd = new Vector4D[mesh.VertexCount];
                
                boneTransform = new float[16 * mesh.BoneCount];
                Dictionary<int, List<Vector2D>> stat = new Dictionary<int, List<Vector2D>>();
                for (int i = 0; i < mesh.BoneCount; i++)
                {
                    boneMap[mesh.Bones[i].Name] = i;

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
            foreach(var p in pack)
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
            for(int i=0;i< faces.Count;i++)
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
        int anim = 100;
        public void ProcessAnimation()
        {
            anim = (anim + 1) % animation.NodeAnimationChannels[0].PositionKeyCount;
            Matrix4x4 root =  Matrix4x4.FromRotationX(-Mathf.PI / 2);
           
            CalculateBonesTransform(rootNode, root, anim);
        }
        private void CalculateBonesTransform(Node node, Matrix4x4 parent, int anim)
        {
            Matrix4x4 nodeTransform = node.Transform;
            if(nodeMap.ContainsKey(node.Name))
            {
                int nodeId = nodeMap[node.Name];
                var chanel = animation.NodeAnimationChannels[nodeId];
                Matrix4x4 scale = Matrix4x4.FromScaling(chanel.ScalingKeys[anim].Value);
                Matrix4x4 rotation = new Matrix4x4(chanel.RotationKeys[anim].Value.GetMatrix());
                Matrix4x4 translation = Matrix4x4.FromTranslation(chanel.PositionKeys[anim].Value);
                nodeTransform = scale * rotation * translation;

            }
            nodeTransform = nodeTransform * parent;
            if(boneMap.ContainsKey(node.Name))
            {
                int bone = boneMap[node.Name];
                WriteMatrix(boneTransform, bone,  mesh.Bones[bone].OffsetMatrix * nodeTransform);
            }
            for (int i = 0; i < node.ChildCount; i++)
                CalculateBonesTransform(node.Children[i], nodeTransform, anim);
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
