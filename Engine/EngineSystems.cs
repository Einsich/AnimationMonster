using ECS;
using System;
using System.Drawing;
using System.IO;

using System.Numerics;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.Texture;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;

namespace Engine
{
    public class StartSystem : BaseSystem
    {

        ShaderProgram standartShader = new ShaderProgram();
        public StartSystem()
            : base(0) { }
        Texture2D texture = new Texture2D();
        public override void Start()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Models\rp_manuel_animated_001_dancing.fbx");
            Assimp.Scene s;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            s = importer.ImportFile(fileName);
            var r = s.Meshes[0];
            //foreach(var mesh in r)
            {

                Entity model = Entity.Create<MeshRenderer, Transform>();
                model.GetComponent<MeshRenderer>().mesh = new ProcessedMesh(s.Meshes[0]);
                model.GetComponent<Transform>();
            }
           
            EntitySystem.AddSystem<CameraControlSystem>();
            EntitySystem.AddSystem<SkyBoxRenderSystem>();
            standartShader.Create(GLContainer.OpenGL, File.ReadAllText(@"Shaders\standart_shader.vertex"), File.ReadAllText(@"Shaders\standart_shader.fragment"), null);


            texture.Create(GLContainer.OpenGL);

            texture.SetImage(GLContainer.OpenGL, new Bitmap(@"Models\tex\rp_manuel_animated_001_dif.jpg"), true);
            texture.Unbind(GLContainer.OpenGL);
            
            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Position, "Position");
            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Normal, "Normal");
            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.TexCoord, "TexCoord");


        }
        public void Update(MeshRenderer meshRenderer, Transform transform)
        {

            ProcessedMesh mesh = meshRenderer.mesh;
            OpenGL gl = GLContainer.OpenGL;
            standartShader.Bind(gl);

            //  Set the light position.
            standartShader.SetUniform3(gl, "LightPosition", 0.25f, 0.25f, 10f);
            Camera camera = null;
            Transform cameraTransform = null;
            EntitySystem.FirstQuery(new Action<Camera, Transform, MainCameraTag>((cam, tr, tag) => { camera = cam; cameraTransform = tr; }));
            if (camera == null)
                return;
            //  Set the matrices.
            Matrix4x4 proj;
            Matrix4x4.Invert(cameraTransform.GetMatrix, out proj);
            standartShader.SetUniformMatrix4(gl, "Projection", (camera.GetProjection ).ToArray());
            standartShader.SetUniformMatrix4(gl, "Modelview", (proj * transform.GetMatrix).ToArray());
            standartShader.SetUniformMatrix3(gl, "NormalMatrix", transform.GetMatrix.To3x3Array());
                
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            texture.Bind(gl);
            gl.Uniform1(gl.GetUniformLocation(standartShader.ShaderProgramObject, "mainTex"), 0);

            var vertexBufferArray = mesh.vertexBufferArray;
            vertexBufferArray.Bind(gl);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, mesh.vertexCount);

            vertexBufferArray.Unbind(gl);
            
            texture.Unbind(gl);
            standartShader.Unbind(gl);
        }

        public override void End()
        {
        }
    }
    public class CameraControlSystem : BaseSystem
    {
        public CameraControlSystem():base(2)
        {
            
            Entity camera = Entity.Create<Camera>();            
            camera.GetComponent<Camera>().CreatePerspective(Mathf.DegreesToRadian(90), MainWindow.aspectRatio, 0.01f, 100000);
            camera.AddComponent<Transform>().position = new Vector3(0,150,90);
            camera.AddComponent<MainCameraTag>();
            camera = Entity.Create<Camera>();
            camera.GetComponent<Camera>().CreatePerspective(Mathf.DegreesToRadian(60), MainWindow.aspectRatio, 0.01f, 100000);
            camera.AddComponent<Transform>().position = new Vector3(0, 150, 150);
            

        }

        public override void End()
        {

        }

        public override void Start()
        {
        }
        int tick = 0;
        public void Update(Camera camera, Transform transform)
        {
            //camera.transform.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.1f));
            // entity.GetComponent<Camera>().transform.position -= Vector3.UnitZ * 0.1f - Vector3.UnitY * 0.1f;
            transform.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.01f));
            tick++;
        }

    }
    public class SkyBoxRenderSystem : BaseSystem
    {

        ShaderProgram skyboxShader = new ShaderProgram();
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
            skyboxShader.Create(GLContainer.OpenGL, File.ReadAllText(@"Shaders\sky_box.vertex"), File.ReadAllText(@"Shaders\sky_box.fragment"), null);
            skyboxShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Position, "Position");
        }
        public void Update(SkyBox skyBox)
        {
            OpenGL gl = GLContainer.OpenGL;
            gl.DepthMask((byte)OpenGL.GL_FALSE);
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
