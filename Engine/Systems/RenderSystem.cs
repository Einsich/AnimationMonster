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
            shader.SetUniform3(gl, "LightPosition", 0.25f, 0.25f, 10f);
            Camera camera = null;
            Transform cameraTransform = null;
            EntitySystem.FirstQuery(new Action<Camera, Transform, MainCameraTag>((cam, tr, tag) => { camera = cam; cameraTransform = tr; }));
            if (camera == null)
                return;
            //  Set the matrices.
            Matrix4x4 proj;
            Matrix4x4.Invert(cameraTransform.GetMatrix, out proj);
            shader.SetUniformMatrix4(gl, "Projection", (camera.GetProjection).ToArray());
            shader.SetUniformMatrix4(gl, "Modelview", (proj * transform.GetMatrix).ToArray());
            shader.SetUniformMatrix3(gl, "NormalMatrix", transform.GetMatrix.To3x3Array());
            if (mesh.hasAnimation)
            {
                mesh.ProcessAnimation();
                gl.UniformMatrix4(gl.GetUniformLocation(shader.ShaderProgramObject, "Bones"), mesh.boneTransform.Length / 16, false, mesh.boneTransform);
            }
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            texture.Bind(gl);
            gl.Uniform1(gl.GetUniformLocation(shader.ShaderProgramObject, "mainTex"), 0);

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
    }
}
