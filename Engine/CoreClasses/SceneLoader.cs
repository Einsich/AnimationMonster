using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System;
using SharpGL.Texture;
using SharpGL.Shaders;
using System.IO;
using Assimp;
using ECS;
using System.Windows;
using Engine.CoreClasses;

namespace Engine
{
    static class SceneLoader
    {
        public static void LoadScene()
        {
            AssimpContext importer = new Assimp.AssimpContext();
            // CreateTerrein();
            LoadMocapAnimations(importer);
            {
                AddCube(new System.Numerics.Vector3(0, -150, 0), new System.Numerics.Vector3(200,1,200));

            }

            
            ShaderContainer.AddShader("height_map", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});

            ShaderContainer.AddShader("bones", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});

            ShaderContainer.AddShader("standart_shader", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" }  });


            ShaderContainer.AddShader("animation", new Dictionary<uint, string>()
            {  { VertexAttributes.Position, "Position" },{ VertexAttributes.Normal, "Normal" },{ VertexAttributes.TexCoord, "TexCoord" } ,
            { VertexAttributes.BoneWeights, "BoneWeights" },{ VertexAttributes.BoneIndex, "BoneIndex" }});


            ShaderContainer.AddShader("sky_box",  new Dictionary<uint, string>() { { VertexAttributes.Position, "Position" }});

        }
        static void CreateTerrein()
        {
            Entity terrain = Entity.Create<HeightMap, Transform>();
            terrain.GetComponent<HeightMap>().CreateTerrain(1024, 1024, 1.5f, 1000f);
            terrain.GetComponent<Transform>().position = new System.Numerics.Vector3(0, -10, 0);
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
        static void printTree(Node node, int d, List<AnimationNode> animationNodes = null)
        {
            string ContainsInAnimNodes(string nodeName)
            {
                if (animationNodes != null)
                {
                    foreach (var x in animationNodes)
                        if (x.name == nodeName)
                            return "*";
                }
                return "";
            }
                Console.WriteLine(new string(' ', d * 2) + " " + node.Name + ContainsInAnimNodes(node.Name) + " " + node.Transform.ToString()) ;
            for (int i = 0; i < node.ChildCount; i++)
                printTree(node.Children[i], d + 1, animationNodes);
        }
        static void LoadMocapAnimations(AssimpContext importer)
        {
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string fileName;
            importer.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
            fileName = Path.Combine(directory, @"FBX_Animations\MotusMan_v55\MotusMan_v55.fbx");
            Scene b = importer.ImportFile(fileName, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs | PostProcessSteps.LimitBoneWeights | PostProcessSteps.GenerateNormals);
            Entity model = Entity.Create<MeshRenderer, AnimationRenderer, Transform>();
            var mesh = new ProcessedMesh(b, 0);
            model.GetComponent<MeshRenderer>().mesh = mesh;
            Node modelRoot = FindHips(b.RootNode);
            Dictionary<string, int> boneMap = new Dictionary<string, int>();
            for (int i = 0; i < b.Meshes[0].BoneCount; i++)
                boneMap.Add(b.Meshes[0].Bones[i].Name, i);
            string[] names = Directory.GetFiles(Path.Combine(directory, $@"FBX_Animations\Animation\Root_Motion"), "*.fbx");
            List<Matrix4x4> meshToBone = new List<Matrix4x4>(b.Meshes[0].Bones.Select((x) => x.OffsetMatrix));
            List<AnimationInfo> animations = new List<AnimationInfo>();
            int size = 0;
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine($"**{names[i]}**");
                Scene s = importer.ImportFile(names[i], PostProcessSteps.OptimizeGraph);
                Animation anim = s.Animations[0];
                List<AnimationNode> animationNodes = GetAnimationInfo(anim.NodeAnimationChannels, boneMap);

                Console.WriteLine($"Loaded {anim.DurationInTicks} channel count = {animationNodes.Count}");
                int frameCount = -1;
                foreach (var channel in animationNodes)
                {        
                    //Console.WriteLine($"{channel.name} P = {channel.translations.Count}, R = {channel.rotations.Count}");
                    if (channel.rotations.Count != 0)
                    {
                        if (frameCount < 0)
                            frameCount = channel.rotations.Count;
                        else
                            if (frameCount != channel.rotations.Count)
                            channel.rotations.Clear();
                    }
                    size += channel.rotations.Count * 4;
                    size += channel.translations.Count * 3;
                }
                size += animationNodes.Count;
                Node animNode = FindHips(s.RootNode);
               
                //normalizeDFS(s.RootNode, 0);
                //printTree(s.RootNode, 0, animationNodes);
                string animatonName = string.Format("{0} {1} / {2}", names[i].Substring(names[i].LastIndexOf('\\')+1), i + 1, names.Length);
                animations.Add(new AnimationInfo(animatonName, boneMap, meshToBone, animationNodes, (float)(anim.DurationInTicks / anim.TicksPerSecond), frameCount));
            }
            Console.WriteLine("Size in bytes = " + (size * 4 / 1024).ToString());
            model.GetComponent<AnimationRenderer>().Create(animations, modelRoot);

        }
        static string NormalizeName(string name)
        {
            int index = name.IndexOf('$');
            return (index > 0) ? name.Substring(0, index - 1) : name;
        }
        static void normalizeDFS(Node node, int d)
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
                if (NormalizeName(child.Name) == nodeName)
                {
                    transform = child.Transform * transform;
                }
                else
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
        static List<AnimationNode> GetAnimationInfo(List<NodeAnimationChannel> channels, Dictionary<string, int> boneMap)
        {
            List<AnimationNode> animationNodes = new List<AnimationNode>(boneMap.Count);
            for (int i = 0; i < boneMap.Count; i++)
                animationNodes.Add(new AnimationNode());
            foreach (var channel in channels)
            {
                string name = NormalizeName(channel.NodeName);
                int i = boneMap[name];
                if (channel.PositionKeys.Count > 1)
                {
                    foreach (var x in channel.PositionKeys)
                        animationNodes[i].translations.Add(x.Value);
                }
                if (channel.RotationKeys.Count > 1)
                {
                    foreach (var x in channel.RotationKeys)
                        animationNodes[i].rotations.Add(x.Value);
                }
            }
            return animationNodes;
        }
        static  List<Matrix4x4> GetMocapMeshToBones(Node root, Node animRoot, Animation animation, List<Bone>bones, Dictionary<string, int> nodeMap)
        {
            List<Matrix4x4> meshToBone = new List<Matrix4x4>(bones.Select((b) => b.OffsetMatrix));
            
            
            void dfs(Node node, Node animNode, Matrix4x4 parent, Matrix4x4 animParent)
            {
                string name = node.Name;

                Matrix4x4 nodeTransform = node.Transform;
                Matrix4x4 animTransform = animNode != null ? animNode.Transform : node.Transform;
                parent = nodeTransform * parent;
                animParent = animTransform * animParent;
                if (nodeMap.ContainsKey(node.Name))
                {
                    int bone = nodeMap[node.Name];
                    if (animNode != null)
                    {
                        Matrix4x4 m = (parent);
                        Matrix4x4 a = (animParent);
                        a.Inverse();
                        Matrix4x4 t = a * m;
                        t.SetTranslation(new Vector3D(0));
                        meshToBone[bone] *= t;
                    }
                    else
                    {
                        Console.WriteLine("I hasn't animNode" + node.Name);
                    }
                    
                }
                for (int i = 0; i < node.ChildCount; i++)
                    dfs(node.Children[i], BoneRender.GetNextAnimTreeChild(node, animNode, i), parent, animParent);
            }
            //dfs(root, animRoot, Matrix4x4.Identity, Matrix4x4.Identity);
            return meshToBone;
        }
        static Node FindHips(Node node)
        {
            Matrix4x4 transform = node.Transform;
            while (node.ChildCount > 0 && node.Name != "Hips")
            {
                node = node.Children[0];
                transform = node.Transform * transform;
            }
            transform = node.Transform * transform;
            transform.SetTranslation(new Vector3D(0));
            //node.Transform =  Matrix4x4.FromRotationY(Mathf.DegToRad * -90) * Matrix4x4.FromRotationX(Mathf.DegToRad * -90);
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
