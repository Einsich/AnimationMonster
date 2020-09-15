using System.Collections.Generic;
using System.Drawing;
using SharpGL.Texture;
using SharpGL.Shaders;
using System.IO;
using Assimp;
using ECS;
namespace Engine
{
    static class SceneLoader
    {
        public static void LoadScene()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Models\rp_manuel_animated_001_dancing.fbx");
            Scene s;
            AssimpContext importer = new Assimp.AssimpContext();
            s = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights);
            var r = s.Meshes[0];
            //foreach(var mesh in r)
            {

                Entity model = Entity.Create<MeshRenderer, Transform>();
                model.GetComponent<MeshRenderer>().mesh = new ProcessedMesh(s, 0);
                model.GetComponent<Transform>();
            }
            {
                Entity cube = Entity.Create<MeshRenderer, Transform>();
                cube.GetComponent<MeshRenderer>().mesh = ProcessedMesh.Cube();
                cube.GetComponent<Transform>().position = new System.Numerics.Vector3(100, 0, 0);
                cube.GetComponent<Transform>().SetScale(5);

            }
            Texture2D texture = new Texture2D();
            TextureContainer.AddTexture("mainTexture", texture);
            texture.Create(GLContainer.OpenGL);
            texture.SetImage(GLContainer.OpenGL, new Bitmap(@"Models\tex\rp_manuel_animated_001_dif.jpg"), true);
            texture.Unbind(GLContainer.OpenGL);

            ShaderContainer.AddShader("standart_shader", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" }  });


            ShaderContainer.AddShader("animation", new Dictionary<uint, string>()
            {  { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" } ,
            { VertexAttributes.BoneWeights, "BoneWeights" },{ VertexAttributes.BoneIndex, "BoneIndex" }});


            ShaderContainer.AddShader("sky_box",  new Dictionary<uint, string>() { { VertexAttributes.Position, "Position" }});

        }
    }
}
