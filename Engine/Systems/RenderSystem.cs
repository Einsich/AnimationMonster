using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;

using SharpGL;
using SharpGL.Shaders;
using SharpGL.Texture;
using Assimp;
using ECS;
using Matrix4x4 = System.Numerics.Matrix4x4;
namespace Engine.Systems
{
    public class RenderSystem : BaseSystem
    {

        public RenderSystem()
            : base(0) { }
        public override void Start()
        {

            

        }

        public void Update(MeshRenderer meshRenderer, Transform transform)
        {

            ProcessedMesh mesh = meshRenderer.mesh;
            OpenGL gl = GLContainer.OpenGL;
            ShaderProgram shader = mesh.hasAnimation ? ShaderContainer.GetShader("animation") : ShaderContainer.GetShader("standart_shader");
            Texture2D texture = TextureContainer.GetTexture("mainTexture");
            shader.Bind(gl);

            //  Set the light position.
            Camera camera = null;
            Transform cameraTransform = null;
            EntitySystem.FirstQuery(new Action<Camera, Transform, MainCameraTag>((cam, tr, tag) => { camera = cam; cameraTransform = tr; }));
            if (camera == null)
                return;

            Matrix4x4 view = cameraTransform.GetMatrix, tran = transform.GetMatrix;
            Matrix4x4.Invert(view, out view);

            shader.SetUniformMatrix4(gl, "ViewProjection", (view * camera.GetProjection).ToArray());
            shader.SetUniformMatrix4(gl, "Model", (tran).ToArray());
            shader.SetUniformMatrix3(gl, "NormalMatrix", tran.To3x3Array());

            if (mesh.hasAnimation)
            {
                mesh.ProcessAnimation();
                gl.UniformMatrix4(gl.GetUniformLocation(shader.ShaderProgramObject, "Bones"), mesh.boneTransform.Length / 16, false, mesh.boneTransform);
            }
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            texture.Bind(gl);
            gl.Uniform1(gl.GetUniformLocation(shader.ShaderProgramObject, "mainTex"), 0);


            shader.SetUniform3(gl, "LightPosition", 0.25f, 15f, 10f);
            if (mesh.hasUvs)
            {
                shader.SetUniform3(gl, "DiffuseMaterial", -1, -1, -1);
            }
            else
            {
                shader.SetUniform3(gl, "DiffuseMaterial", 0, 0.1f, 0.3f);
                shader.SetUniform3(gl, "AmbientMaterial", 0, 0.2f, 0.5f);
                shader.SetUniform3(gl, "SpecularMaterial", 0.3f, 0.3f, 0.3f);
                var cam = cameraTransform.position;
                shader.SetUniform3(gl, "CameraPosition", cam.X, cam.Y, cam.Z);
                shader.SetUniform1(gl, "Shininess", 0.1f);
                shader.SetUniform1(gl, "Reflectance", 0.1f);
            }


            var vertexBufferArray = mesh.vertexBufferArray;
            vertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, mesh.vertexCount); 

            vertexBufferArray.Unbind(gl);

            texture.Unbind(gl);
            shader.Unbind(gl);
        }

        public override void End()
        {
        }
        public void PrintMat(Matrix4x4 m)
        {
            var t = m.ToArray();
            string s = "";
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    s += t[i * 4 + j].ToString("N2") + (j == 3 ? "\n" : " ");
            Console.WriteLine(s);
        }
    }
}
