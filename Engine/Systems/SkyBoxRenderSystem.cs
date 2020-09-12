using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.Shaders;
using ECS;
using Matrix4x4 = System.Numerics.Matrix4x4;
namespace Engine.Systems
{

    public class SkyBoxRenderSystem : BaseSystem
    {

        public SkyBoxRenderSystem() : base(-10)
        {

        }
        public override void End()
        {
        }

        public override void Start()
        {
            {
                Entity skyBox = Entity.Create<SkyBox>();
                skyBox.GetComponent<SkyBox>().Init();
            }
        }
        public void Update(SkyBox skyBox)
        {
            OpenGL gl = GLContainer.OpenGL;
            gl.DepthMask((byte)OpenGL.GL_FALSE);
            ShaderProgram skyboxShader = ShaderContainer.GetShader("sky_box");
            skyboxShader.Bind(gl);

            Camera camera = null;
            Transform cameraTransform = null;
            EntitySystem.FirstQuery(new Action<Camera, Transform, MainCameraTag>((cam, tr, tag) => { camera = cam; cameraTransform = tr; }));
            if (camera == null)
                return;
            //  Set the matrices.
            Matrix4x4 proj;
            Matrix4x4.Invert(cameraTransform.GetMatrix, out proj);
            proj.Translation = Vector3.Zero;
            skyboxShader.SetUniformMatrix4(gl, "Projection", (camera.GetProjection).ToArray());
            skyboxShader.SetUniformMatrix4(gl, "Modelview", (proj).ToArray());

            gl.BindVertexArray(skyBox.vertexBufferArrayObject);
            gl.BindTexture(OpenGL.GL_TEXTURE_CUBE_MAP, skyBox.cubeMapObject);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 36);
            gl.DepthMask((byte)OpenGL.GL_TRUE);
            skyboxShader.Unbind(gl);
        }
    }
}
