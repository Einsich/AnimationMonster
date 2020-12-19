using System;
using System.Collections.Generic;
using System.Linq;
using SharpGL;
using SharpGL.Shaders;
using Assimp;
using ECS;

namespace Engine
{
    public class AnimationInfo
    {
        public AnimationInfo(string name, Dictionary<string, int> boneMap,List<Matrix4x4> meshToBone, List<CoreClasses.AnimationNode> animationNodes, float allTime, int frameCount) =>
            (animationName, this.boneMap,this.meshToBone, this.animationNodes, this.endTime, this.frameCount) = (name, boneMap, meshToBone, animationNodes, allTime, frameCount);
        public string animationName;
        public Dictionary<string, int> boneMap;
        public List<Matrix4x4> meshToBone;
        public List<CoreClasses.AnimationNode> animationNodes;
        public float endTime;
        public int frameCount;
    }
    public struct AnimationRenderInfo
    {
        public AnimationRenderInfo(Node root, AnimationInfo info, float time, int currentFrame, int nextFrame) =>
            (this.animationInfo, modelRoot, this.Lerptime, this.currentFrame, this.nextFrame) = (info, root, time, currentFrame, nextFrame);
        public AnimationInfo animationInfo;
        public Node modelRoot;
        public float Lerptime;
        public int currentFrame, nextFrame;
    }
  public class AnimationRenderer
    {
        private List<AnimationInfo> animations;
        private int animationIndex;
        private int currentFrame, nextFrame;
        private float animationLerpTime;
        private float speed = 1;
        private Node modelRoot;
        public void Create(List<AnimationInfo> animations, Node root)
        {
            this.animations = animations;
            animationIndex = 0;
            currentFrame = 0;
            nextFrame = 1;
            animationLerpTime = 0;
            modelRoot = root;
        }
        public AnimationRenderInfo TickAnimation()
        {
            AnimationRenderInfo p = new AnimationRenderInfo(modelRoot,animations[animationIndex], animationLerpTime, currentFrame, nextFrame);
            if (Input.GetKey(System.Windows.Input.Key.Enter) > 0)
                return p;
            speed = Mathf.Clamp(speed * (10 + Input.GetKey(System.Windows.Input.Key.OemPlus) - Input.GetKey(System.Windows.Input.Key.OemMinus))/10, 0.1f, 100);
            animationLerpTime += Time.deltaTime * speed / animations[animationIndex].endTime * animations[animationIndex].frameCount;
            while(animationLerpTime >= 1f)
            {
                animationLerpTime -= 1f;
                currentFrame++;
                nextFrame++;
                if (nextFrame == animations[animationIndex].frameCount)
                {
                    animationLerpTime = 0;
                    animationIndex++;
                    currentFrame = 0;
                    nextFrame = 1;
                    if (animationIndex >= animations.Count)
                        animationIndex = 0;
                }
                
            }
            return p;
        }
    }
    public struct NotRequareAnimation { }
    public class BoneRender
    {
        static BoneRender renderer;
        public static Vector3D rootPoint;
        public static string animationInfo;
        public static void AddBones(Matrix4x4 from_m, Matrix4x4 to_m, Vector3D color, string name)
        {
            Vector3D from = new Vector3D(from_m.A4, from_m.B4, from_m.C4);
            Vector3D to = new Vector3D(to_m.A4, to_m.B4, to_m.C4);
            Vector3D v1 = new Vector3D(from_m.A1, from_m.B1, from_m.C1);
            Vector3D v2 = new Vector3D(from_m.A2, from_m.B2, from_m.C2);
            Vector3D v3 = new Vector3D(from_m.A3, from_m.B3, from_m.C3);
            if (renderer == null)
                renderer = Entity.Create<BoneRender>().GetComponent<BoneRender>();
            renderer.AddBone(from, to, color, name);
            return;
            float scale = (from - to).Length() * 0.2f;
            renderer.AddBone(from, from + v1 * scale, new Vector3D(1, 0, 0), null);
            renderer.AddBone(from, from + v2 * scale, new Vector3D(0, 1, 0), null);
            renderer.AddBone(from, from + v3 * scale, new Vector3D(0, 0, 1), null);
        }

        public List<(Vector3D, Vector3D, Vector3D, string)> bones = new List<(Vector3D, Vector3D, Vector3D, string)>();
        private void AddBone(Vector3D from, Vector3D to, Vector3D color, string name)
        {
            bones.Add((from, to, color, name));
        }
        Matrix4x4 getTransform(Vector3D from, Vector3D to)
        {
            Vector3D d = to - from;
            float length = d.Length();
            float size = Mathf.Min(length * 0.1f, 3);
            Vector3D scale = new Vector3D(size, length, size);
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
        public void RenderAnimationInfo(OpenGL gl)
        {
            gl.DrawText(100, 100, 0, 0, 0, "Arial", 20.0f, animationInfo);

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

            string animName = model.Children[ind].Name;
            for (int i = 0; i < animation.ChildCount; i++)
                if (animation.Children[i].Name == animName)
                    return animation.Children[i];
            
            return  null;
        }
    }
}
