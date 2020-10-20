using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
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
            AssimpContext importer = new Assimp.AssimpContext();
            LoadRococoAnimations(importer);
            {
                AddCube(new System.Numerics.Vector3(0, -100, 0), new System.Numerics.Vector3(200,1,200));

            }
            ShaderContainer.AddShader("bones", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});

            ShaderContainer.AddShader("standart_shader", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" }  });


            ShaderContainer.AddShader("animation", new Dictionary<uint, string>()
            {  { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" } ,
            { VertexAttributes.BoneWeights, "BoneWeights" },{ VertexAttributes.BoneIndex, "BoneIndex" }});


            ShaderContainer.AddShader("sky_box",  new Dictionary<uint, string>() { { VertexAttributes.Position, "Position" }});

        }
        static void LoadAnimations(AssimpContext importer)
        {
            /*string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName;
            fileName = Path.Combine(directory, @"Models\newtonalpha.fbx");
            Scene b = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateNormals);

            Entity model = Entity.Create<MeshRenderer, AnimationRenderer, Transform>();
            var mesh = new ProcessedMesh(b, 0);
            model.GetComponent<MeshRenderer>().mesh = mesh;


            List<Animation> animations = new List<Animation>();
            List<Node> animTrees = new List<Node>();
            for (int i = 11; i < 17; i++)
            {
                fileName = Path.Combine(directory, $@"Models\mocap\FBX 2013\16_{i}.fbx");
                Scene s = importer.ImportFile(fileName);
                animations.Add(s.Animations[0]);
                animTrees.Add(s.RootNode);
            }

            model.GetComponent<AnimationRenderer>().Create(animations);
            if(false)
            using (StreamWriter wr = new StreamWriter(Path.Combine(directory, $@"Models\report.txt")))
            {
                Dictionary<string, int> animNodes = new Dictionary<string, int>();
                Dictionary<string, int> animChnels = new Dictionary<string, int>();
                void WrDFS(Node node, bool r = false)
                {
                    if (r)
                    {
                        if (!animNodes.ContainsKey(node.Name))
                            animNodes.Add(node.Name, 0);
                        animNodes[node.Name]++;
                    }
                    else
                        wr.WriteLine(node.Name);
                    for (int i = 0; i < node.ChildCount; i++)
                        WrDFS(node.Children[i], r);
                }
                wr.WriteLine("Model");
                WrDFS(b.RootNode);
                for (int i = 0; i < animTrees.Count; i++) 
                {
                    WrDFS(animTrees[i], true);
                }
                wr.WriteLine("\n animations \n");
                foreach(var p in animNodes)
                {
                    wr.WriteLine($"{p.Key} {p.Value}");
                }
                for (int i = 0; i < animations.Count; i++)
                {
                    //wr.WriteLine(animations[i].Name);
                    for (int j = 0; j < animations[i].NodeAnimationChannelCount; j++)
                    // wr.WriteLine(animations[i].NodeAnimationChannels[j].NodeName);
                    {
                        if (!animChnels.ContainsKey(animations[i].NodeAnimationChannels[j].NodeName))
                            animChnels.Add(animations[i].NodeAnimationChannels[j].NodeName, 0);
                        animChnels[animations[i].NodeAnimationChannels[j].NodeName]++;
                    }
                }
                wr.WriteLine("\n animations ccchheels \n");
                foreach (var p in animChnels)
                {
                    wr.WriteLine($"{p.Key} {p.Value}");
                }
            }

            var map = GetBonesMap(b.RootNode.Children[0], animations[0], directory);
            //mesh.SetBoneMap(map, b.RootNode.Children[0].Children[0].Children[0].Children[0], s.RootNode.Children[0].Children[0]);
            mesh.SetBoneMap(map, b.RootNode, animTrees[0]);*/
        }
        static Dictionary<string, int> GetBonesMap(Node root, Animation animation, string directory)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            string path = Path.Combine(directory, $@"Models\bones_map_mocap_lab.txt");
            Dictionary<string, string> nameMap = new Dictionary<string, string>();
            using (StreamReader sr = File.OpenText(path))
            {
                while (!sr.EndOfStream)
                {
                    string[] s = sr.ReadLine().Split(' ');
                    if (s.Length != 2 || s[0].Length < 2 || s[0].StartsWith("//"))
                        continue;
                    nameMap.Add(s[0], s[1]);
                }
            }
            void dfs(Node node)
            {
                string name = node.Name;

                if (nameMap.ContainsKey(name))
                    name = nameMap[name];

                for (int i = 0; i < animation.NodeAnimationChannelCount; i++)
                    if (name == animation.NodeAnimationChannels[i].NodeName)
                    {
                        map.Add(node.Name, i);
                        break;
                    }
                for (int i = 0; i < node.ChildCount; i++)
                    dfs(node.Children[i]);
            }
            dfs(root);
            return map;
        }
        static void LoadDancer(AssimpContext importer)
        {
            string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Models\rp_manuel_animated_001_dancing.fbx");
            Scene s = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights);
            var r = s.Meshes[0];
            //foreach(var mesh in r)
            {

                Entity model = Entity.Create<MeshRenderer, Transform>();
                model.GetComponent<MeshRenderer>().mesh = new ProcessedMesh(s, 0);
                model.GetComponent<Transform>();
            }

            Texture2D texture = new Texture2D();
            TextureContainer.AddTexture("mainTexture", texture);
            texture.Create(GLContainer.OpenGL);
            texture.SetImage(GLContainer.OpenGL, new Bitmap(@"Models\tex\rp_manuel_animated_001_dif.jpg"), true);
            texture.Unbind(GLContainer.OpenGL);
        }
        static void printTree(Node node, int d = 0)
        {
            
            Console.WriteLine(new string(' ', d * 2) + " " + node.Name);
            for (int i = 0; i < node.ChildCount; i++)
                printTree(node.Children[i], d + 1);
        }
        static void LoadRococoAnimations(AssimpContext importer)
        {
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName;
            fileName = Path.Combine(directory, @"Models\newtonalpha.fbx");
            Scene b = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateNormals);
            Entity model = Entity.Create<MeshRenderer, AnimationRenderer, Transform>();
            var mesh = new ProcessedMesh(b, 0);
            model.GetComponent<MeshRenderer>().mesh = mesh;
            Node modelRoot = FindHips(b.RootNode);
            
            string[] names = Directory.GetFiles(Path.Combine(directory, $@"Models\mocap"), "*.fbx");
            
            List<AnimationInfo> animations = new List<AnimationInfo>();
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine($"*****{names[i]}*****");
                Scene s = importer.ImportFile(names[i]);
                Animation anim = s.Animations[0];
                int c = anim.NodeAnimationChannels[0].PositionKeys.Count;
                try
                {
                    Console.WriteLine($"{anim.DurationInTicks} {anim.TicksPerSecond} {c} [{anim.NodeAnimationChannels[0].PositionKeys[1].Time}, {anim.NodeAnimationChannels[0].PositionKeys[c - 1].Time}]");
                } catch
                {
                    int t = 0;
                }
                Node animNode = FindHips(s.RootNode);
                var p = GetRococoBonesMap(modelRoot, animNode, anim, directory, b.Meshes[0].Bones, mesh.nodeMap);
                //printTree(animNode);
                animations.Add(new AnimationInfo(p.Item1, p.Item2, anim, animNode));
            }
            
            model.GetComponent<AnimationRenderer>().Create(animations, modelRoot);

        }
        static (Dictionary<string, int>, List<Matrix4x4>) GetRococoBonesMap(Node root, Node animRoot, Animation animation, string directory, List<Bone>bones, Dictionary<string, int> nodeMap)
        {
            Dictionary<string, int> map = new Dictionary<string, int>();
            List<Matrix4x4> meshToBone = new List<Matrix4x4>(bones.Select((b) => b.OffsetMatrix));
            using (StreamWriter wr = new StreamWriter(Path.Combine(directory, $@"Models\report.txt")))
            {
                void WrDFS(Node node)
                {
                    wr.WriteLine(node.Name);
                    foreach (var x in node.Metadata)
                        wr.WriteLine(x.Key.ToString() + " " + x.Value.ToString());
                    wr.WriteLine("-----------------------");
                    for (int i = 0; i < node.ChildCount; i++)
                        WrDFS(node.Children[i]);
                }
                //WrDFS(root);
            }
            string NormalizeName(string name)
            {
                int index = name.IndexOf('$');
                return (index > 0) ? name.Substring(0, index - 1) : name;
            }
            void normalizeDFS(Node node, int d)
            {
                //Console.WriteLine(new string(' ', d * 4) + " " + node.Name);
                if (node.Name.Contains("_$AssimpFbx$_"))
                {
                    string nodeName = node.Name;
                    nodeName = nodeName.Remove(nodeName.IndexOf('$') - 1);
                    Matrix4x4 transform = node.Transform;
                    Node child = node.Children[0];
                    while (NormalizeName(child.Name) == nodeName && child.ChildCount > 0)
                    {
                        transform = child.Transform * transform;
                        child = child.Children[0];
                    }
                    child = child.Parent;
                    child.Name = nodeName;
                    child.Transform = transform;
                    if (node.Parent != null)
                        node.Parent.Children[node.Parent.Children.IndexOf(node)] = child;
                    node = child;
                }
                //Console.WriteLine(new string(' ', d * 4) + " " + node.Name);
                for (int i = 0; i < node.ChildCount; i++)
                    normalizeDFS(node.Children[i], d + 1);
            }
            void dfs(Node node, Node animNode, Matrix4x4 parent, Matrix4x4 animParent)
            {
                string name = node.Name;
                
                //Console.WriteLine($"{name} {(animNode != null ? animNode.Name : "null")}");
                
                parent = node.Transform * parent;
                animParent = (animNode != null ? animNode.Transform : node.Transform) * animParent;
                if (nodeMap.ContainsKey(node.Name))
                {
                    int bone = nodeMap[node.Name];
                    Matrix3x3 m = new Matrix3x3(parent);
                    Matrix3x3 a = new Matrix3x3(animParent);
                    a.Inverse();
                    Matrix4x4 t = new Matrix4x4(m * a);
                    meshToBone[bone] *= t;
                }

                for (int i = 0; i < animation.NodeAnimationChannelCount && animNode != null; i++)
                    if (animNode.Name == animation.NodeAnimationChannels[i].NodeName)
                    {
                        map.Add(node.Name, i);
                        break;
                    }
                for (int i = 0; i < node.ChildCount; i++)
                    dfs(node.Children[i], BoneRender.GetNextAnimTreeChild(node, animNode, i), parent, animParent);
            }
            normalizeDFS(animRoot, 0);
            dfs(root, animRoot, Matrix4x4.Identity, Matrix4x4.Identity);
            return (map, meshToBone);
        }
        static Node FindHips(Node node)
        {
            while (node.ChildCount > 0 && node.Name != "Hips")
                node = node.Children[0];
            return node;
        }
        static void AddCube(System.Numerics.Vector3 position, float scale) => AddCube(position, new System.Numerics.Vector3(scale));

        static void AddCube(System.Numerics.Vector3 position, System.Numerics.Vector3 scale)
        {
            Entity cube = Entity.Create<MeshRenderer, Transform, NotRequareAnimation>();
            cube.GetComponent<MeshRenderer>().mesh = ProcessedMesh.Cube();
            cube.GetComponent<Transform>().position = position;
            cube.GetComponent<Transform>().SetScale(scale);
        }
    }

}
