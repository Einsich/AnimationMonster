using ECS;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
        ShaderProgram animationShader = new ShaderProgram();
        public StartSystem()
            : base(0) { }
        Texture2D texture = new Texture2D();
        public override void Start()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Models\rp_manuel_animated_001_dancing.fbx");
            Assimp.Scene s;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            s = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights);
            var r = s.Meshes[0];
            //foreach(var mesh in r)
            {

                Entity model = Entity.Create<MeshRenderer, Transform>();
                model.GetComponent<MeshRenderer>().mesh = new ProcessedMesh(s,0);
                model.GetComponent<Transform>();
            }
           
            EntitySystem.AddSystem<CameraControlSystem>();
            EntitySystem.AddSystem<SkyBoxRenderSystem>();

            texture.Create(GLContainer.OpenGL);
            texture.SetImage(GLContainer.OpenGL, new Bitmap(@"Models\tex\rp_manuel_animated_001_dif.jpg"), true);
            texture.Unbind(GLContainer.OpenGL);

            standartShader.Create(GLContainer.OpenGL, File.ReadAllText(@"Shaders\standart_shader.vertex"), File.ReadAllText(@"Shaders\standart_shader.fragment"), null);

            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Position, "Position");
            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Normal, "Normal");
            standartShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.TexCoord, "TexCoord");

            animationShader.Create(GLContainer.OpenGL, File.ReadAllText(@"Shaders\animation.vertex"), File.ReadAllText(@"Shaders\animation.fragment"), null);

            animationShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Position, "Position");
            animationShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.Normal, "Normal");
            animationShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.TexCoord, "TexCoord");
            animationShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.TexCoord, "BoneWeights");
            animationShader.BindAttributeLocation(GLContainer.OpenGL, VertexAttributes.TexCoord, "BoneIndex");

        }
        public void Update(MeshRenderer meshRenderer, Transform transform)
        {

            ProcessedMesh mesh = meshRenderer.mesh;
            OpenGL gl = GLContainer.OpenGL;
            ShaderProgram shader = mesh.hasAnimation ? animationShader : standartShader;
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
            shader.SetUniformMatrix4(gl, "Projection", (camera.GetProjection ).ToArray());
            shader.SetUniformMatrix4(gl, "Modelview", (proj * transform.GetMatrix).ToArray());
            shader.SetUniformMatrix3(gl, "NormalMatrix", transform.GetMatrix.To3x3Array());
            if(mesh.hasAnimation)
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
    public class CameraControlSystem : BaseSystem
    {
        public CameraControlSystem():base(2)
        {
            
            Entity camera = Entity.Create<Camera>();            
            camera.GetComponent<Camera>().CreatePerspective(Mathf.DegreesToRadian(90), MainWindow.aspectRatio, 0.01f, 100000);
            camera.AddComponent<Transform>().position = new Vector3(0,100,150);
            camera.AddComponent<MainCameraTag>();
            

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
    public class AnimationRenderSystem : BaseSystem
    {

        public AnimationRenderSystem() : base(1)
        {

        }
        public override void End()
        {
        }

        public override void Start()
        {
           
        }
        public void Update()
        {
            OpenGL gl = GLContainer.OpenGL;
           
        }
    }

}
