using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.Texture;
namespace Engine
{

    static class GLContainer
    {
        public static OpenGL OpenGL;
    }
    static class ShaderContainer
    {
        static Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        public static void AddShader(string name, Dictionary<uint, string> attribs)
        {
            ShaderProgram shader = new ShaderProgram();
            shaders.Add(name, shader);
            string vertex = @"Shaders\" + name + ".vertex";
            string fragment = @"Shaders\" + name + ".fragment";
            shader.Create(GLContainer.OpenGL, File.ReadAllText(vertex), File.ReadAllText(fragment), attribs);
        }

        public static ShaderProgram GetShader(string name) => shaders.ContainsKey(name) ? shaders[name] : throw new System.Exception("Haven't shader " + name);
    }
    static class TextureContainer
    {
        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static Texture2D clearTexture;
        static TextureContainer()
        {
            clearTexture = new Texture2D();
            clearTexture.Create(GLContainer.OpenGL);
            clearTexture.SetImage(GLContainer.OpenGL, new System.Drawing.Bitmap(1, 1), false);
            clearTexture.Unbind(GLContainer.OpenGL);
        }
        public static void AddTexture(string name, Texture2D texture) => textures.Add(name, texture);

        public static Texture2D GetTexture(string name) => textures.ContainsKey(name) ? textures[name] : clearTexture;
    }
    static class Extensions
    {
        /// <summary>
        /// Возвращает ориентированную площадь параллепипеда, если поворот от a к b положительный, то и плозадь положительная
        /// </summary>
        public static float Square(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static float[] ToArray(this Vector2 a) => new float[] { a.X, a.Y };
        public static float[] ToArray(this Vector3 a) => new float[] { a.X, a.Y, a.Z };
        public static float[] ToArray(this Matrix4x4 m) => new float[] {
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44};
        public static float[] ToArray(this Assimp.Matrix4x4 m) => new float[] {
            m.A1, m.A2, m.A3, m.A4,
            m.B1, m.B2, m.B3, m.B4,
            m.C1, m.C2, m.C3, m.C4,
            m.D1, m.D2, m.D3, m.D4};
        public static float[] To3x3Array(this Matrix4x4 m) => new float[] {
            m.M11, m.M12, m.M13,
            m.M21, m.M22, m.M23,
            m.M31, m.M32, m.M33};
        public static float[] ToArray(this Assimp.Vector3D vector, int size = 3)
        {
            if (size == 2)
                return new float[] { vector.X, vector.Y };
            else
                return new float[] { vector.X, vector.Y, vector.Z };
        }
        public static Assimp.Vector3D GetLerpedScale(this Assimp.NodeAnimationChannel a, float time)
        {
            int l = 1, r = a.ScalingKeyCount - 1;
            if (a.ScalingKeys[r].Time < time)
                return a.ScalingKeys[r].Value;
            while (r - l > 1)
            {
                int m = (l + r) / 2;
                if (a.ScalingKeys[m].Time < time)
                    l = m;
                else
                    r = m;
            }
            float t = (time - (float)a.ScalingKeys[l].Time) / (float)(a.ScalingKeys[r].Time -  a.ScalingKeys[l].Time);
            return Mathf.Lerp(a.ScalingKeys[l].Value, a.ScalingKeys[r].Value, t);
        }
        public static Assimp.Vector3D GetLerpedPosition(this Assimp.NodeAnimationChannel a, float time)
        {
            int l = 1, r = a.PositionKeyCount -1;
            if (a.PositionKeys[r].Time < time)
                return a.PositionKeys[r].Value;
            while (r - l > 1)
            {
                int m = (l + r) / 2;
                if (a.PositionKeys[m].Time < time)
                    l = m;
                else
                    r = m;
            }
            
            float t = (time - (float)a.PositionKeys[l].Time) / (float)(a.PositionKeys[r].Time - a.PositionKeys[l].Time);
            System.Console.WriteLine(t);
            return Mathf.Lerp(a.PositionKeys[l].Value, a.PositionKeys[r].Value, t);
        }
        public static Assimp.Quaternion GetLerpedRotation(this Assimp.NodeAnimationChannel a, float time)
        {
            int l = 1, r = a.RotationKeyCount - 1;
            if (a.RotationKeys[r].Time < time)
                return a.RotationKeys[r].Value;
            while (r - l > 1)
            {
                int m = (l + r) / 2;
                if (a.PositionKeys[m].Time < time)
                    l = m;
                else
                    r = m;
            }
            float t = (time - (float)a.RotationKeys[l].Time) / (float)(a.RotationKeys[r].Time - a.RotationKeys[l].Time);
            return Assimp.Quaternion.Slerp(a.RotationKeys[l].Value, a.RotationKeys[r].Value, t);
        }
    }
}
