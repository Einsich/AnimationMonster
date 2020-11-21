using System;
using System.Collections.Generic;
using System.Linq;
using SharpGL;
using SharpGL.Shaders;
using Assimp;
using ECS;

namespace Engine
{
    public struct AnimationInfo
    {
        public AnimationInfo(Dictionary<string, int> boneMap, List<Matrix4x4> meshToBone, Animation animation, Node root) =>
            (this.boneMap, this.meshToBone, this.animation, this.root) = (boneMap, meshToBone, animation, root);
        public Dictionary<string, int> boneMap;
        public List<Matrix4x4> meshToBone;
        public Animation animation;
        public Node root;
    }
    public struct AnimationRenderInfo
    {
        public AnimationRenderInfo(AnimationInfo info, float time, Node root) =>
            (this.boneMap, this.animation, this.meshToBone, this.animRoot, this.time, this.root) = (info.boneMap, info.animation, info.meshToBone, info.root, time, root);
        public Dictionary<string, int> boneMap;
        public List<Matrix4x4> meshToBone;
        public Animation animation;
        public Node animRoot;
        public float time;
        public Node root;
    }
  public class AnimationRenderer
    {
        private List<AnimationInfo> animations;
        private int animationIndex;
        private float animationTime;
        private Node root;
        private float speed = 1;
        public void Create(List<AnimationInfo> animations, Node root)
        {
            this.animations = animations;
            animationIndex = 0;
            animationTime = 0;
            this.root = root;
        }
        public AnimationRenderInfo TickAnimation()
        {
            AnimationRenderInfo p = new AnimationRenderInfo(animations[animationIndex], animationTime, root);
            if (Input.GetKey(System.Windows.Input.Key.Enter) > 0)
                return p;
            speed = Mathf.Clamp(speed * (10 + Input.GetKey(System.Windows.Input.Key.OemPlus) - Input.GetKey(System.Windows.Input.Key.OemMinus)) / 10, 0.1f, 10);
            animationTime += Time.deltaTime * speed;
            if(animationTime > animations[animationIndex].animation.DurationInTicks / animations[animationIndex].animation.TicksPerSecond)
            {
                animationTime = 0;
                animationIndex++;
                if (animationIndex >= animations.Count)
                    animationIndex = 0;
            }
            return p;
        }
    }
    public struct NotRequareAnimation { }
    public class BoneRender
    {
        static BoneRender renderer;
        public static void AddBones(Matrix4x4 from_m, Matrix4x4 to_m, Vector3D color, string name)
        {
            Vector3D from = new Vector3D(from_m.A4, from_m.B4, from_m.C4);
            Vector3D to = new Vector3D(to_m.A4, to_m.B4, to_m.C4);
            Vector3D v1 = new Vector3D(from_m.A1, from_m.B1, from_m.C1);
            Vector3D v2 = new Vector3D(from_m.A2, from_m.B2, from_m.C2);
            Vector3D v3 = new Vector3D(from_m.A3, from_m.B3, from_m.C3);
            if (renderer == null)
                renderer = Entity.Create<BoneRender>().GetComponent<BoneRender>();
            float scale = 10f;
            renderer.AddBone(from, to, color, name);
            return;
            renderer.AddBone(from, from + v1 * scale, new Vector3D(1, 0, 0), name);
            renderer.AddBone(from, from + v2 * scale, new Vector3D(0, 1, 0), name);
            renderer.AddBone(from, from + v3 * scale, new Vector3D(0, 0, 1), name);
        }

        public List<(Vector3D, Vector3D, Vector3D, string)> bones = new List<(Vector3D, Vector3D, Vector3D, string)>();
        private void AddBone(Vector3D from, Vector3D to, Vector3D color, string name)
        {
            bones.Add((from, to, color, name));
        }
        Matrix4x4 getTransform(Vector3D from, Vector3D to)
        {
            Vector3D d = to - from;
            Vector3D scale = new Vector3D(1, d.Length(), 1);
            d /= scale.Y;
            return Matrix4x4.FromScaling(scale) * Matrix4x4.FromToMatrix(new Vector3D(0, 1, 0), d) * Matrix4x4.FromTranslation(from);
        }
        public void Render(OpenGL gl, ShaderProgram shader, int vertexCount )
        {
            for (int i = 0; i <bones.Count; i++)
            {
                Matrix4x4 tran = getTransform(bones[i].Item1, bones[i].Item2);
                tran.Transpose();
                Vector3D c = bones[i].Item3;
                shader.SetUniform3(gl, "DiffuseMaterial", c.X,c.Y,c.Z);
                shader.SetUniformMatrix4(gl, "Model", tran.ToArray());

                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vertexCount);
            }
        }
        public void RenderNames(OpenGL gl)
        {
            for (int i = 0; i < bones.Count; i++)
                if (bones[i].Item4 != null)
                {
                    Vector2D screen = Camera.WorldToScreen(bones[i].Item2);

                    gl.DrawText((int)screen.X, (int)screen.Y, 0, 0, 0, "Arial", 20.0f, bones[i].Item4);
                }
        }
        public void Clear()
        {
            bones.Clear();
        }
        static Dictionary<string, string> hipsMap = new Dictionary<string, string> { { "LeftThigh", "LeftUpLeg" }, { "RightThigh", "RightUpLeg" }, { "Spine1", "Spine" } };
        static Dictionary<string, string> fingerMap = new Dictionary<string, string> { 
            { "LeftFinger1Metacarpal", "LeftHandThumb1" }, { "LeftFinger2Metacarpal", "LeftHandIndex1" }, { "LeftFinger3Metacarpal", "LeftHandMiddle1" },
            { "LeftFinger4Metacarpal", "LeftHandRing1" }, { "LeftFinger5Metacarpal", "LeftHandPinky1" },
            { "RightFinger1Metacarpal", "RightHandThumb1" }, { "RightFinger2Metacarpal", "RightHandIndex1" }, { "RightFinger3Metacarpal", "RightHandMiddle1" },
            { "RightFinger4Metacarpal", "RightHandRing1" }, { "RightFinger5Metacarpal", "RightHandPinky1" }        };
        public static Node GetNextAnimTreeChild(Node model, Node animation, int ind)
        {
            string name = model.Name;
            if (name == "Spine3" && !animation.Children[0].Name.Contains("Spine"))
                return animation;
            if(name == "Hips")
            {
                string animName = hipsMap[model.Children[ind].Name];
                for (int i = 0; i < 3; i++)
                    if (animation.Children[i].Name == animName)
                        return animation.Children[i];
            }
            if(name == "Spine4")
            {
                string animName = model.Children[ind].Name;
                for (int i = 0; i < 3; i++)
                    if (animation.Children[i].Name == animName)
                        return animation.Children[i];
            }
            if(model.ChildCount == 5 && animation != null && animation.ChildCount == 5)
            {
                string animName = fingerMap[model.Children[ind].Name];
                for (int i = 0; i < 5; i++)
                {
                    if (animation.Children[i].Name == animName)
                        return animation.Children[i];
                }
            }
            return animation != null && ind < animation.ChildCount ? animation.Children[ind] : null;
        }
    }
}
