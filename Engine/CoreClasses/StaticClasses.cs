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
        public static void AddTexture(string name, Texture2D texture) => textures.Add(name, texture);

        public static Texture2D GetTexture(string name) => textures.ContainsKey(name) ? textures[name] : throw new System.Exception("Haven't texture " + name);
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
    }
}
