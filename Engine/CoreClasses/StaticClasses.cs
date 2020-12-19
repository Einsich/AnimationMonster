using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Assimp;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.Texture;
using Matrix4x4 = System.Numerics.Matrix4x4;
namespace Engine
{

    static class GLContainer
    {
        public static OpenGL OpenGL;
        public static int Width, Height;
    }
    static class ShaderContainer
    {
        static Dictionary<string, (ShaderProgram, Dictionary<uint, string> attribs)> shaders = new Dictionary<string, (ShaderProgram, Dictionary<uint, string> attribs)>();
        static ShaderContainer()
        {
            Input.KeyDownAction[System.Windows.Input.Key.F9] += ReloadShaders;
        }
        public static void AddShader(string name, Dictionary<uint, string> attribs)
        {
            ShaderProgram shader = new ShaderProgram();
            shaders.Add(name, (shader, attribs));
            CreateShader(shader, name, attribs);
        }
        static void CreateShader(ShaderProgram shader, string name, Dictionary<uint, string> attribs)
        {
            string vertex = @"Shaders\" + name + ".vertex";
            string fragment = @"Shaders\" + name + ".fragment";
            try
            {
                shader.Create(GLContainer.OpenGL, File.ReadAllText(vertex), File.ReadAllText(fragment), attribs);
            }
            catch
            {
                shader.AssertValid(GLContainer.OpenGL);
                Console.WriteLine("Error: " + shader.GetInfoLog(GLContainer.OpenGL));
            }
        }
        static void ReloadShaders()
        {
            OpenGL gl = GLContainer.OpenGL;
            foreach(var shader in shaders)
            {
                shader.Value.Item1.Delete(gl);
                CreateShader(shader.Value.Item1, shader.Key, shader.Value.attribs);
            }
        }

        public static ShaderProgram GetShader(string name) => shaders.ContainsKey(name) ? shaders[name].Item1 : throw new System.Exception("Haven't shader " + name);
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
        public static Assimp.Matrix4x4 ToAssimp(this Matrix4x4 m)
        {
            return new Assimp.Matrix4x4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
        }
        public static Assimp.Vector3D Translation(this Assimp.Matrix4x4 m)
        {
            return new Assimp.Vector3D(m.A4, m.B4, m.C4);
        }
        public static void SetTranslation(this ref Assimp.Matrix4x4 m, Assimp.Vector3D translation)
        {
            (m.A4, m.B4, m.C4) = (translation.X, translation.Y, translation.Z);
        }
        public static string ToWriteLine(this Assimp.Matrix4x4 m)
        {
            return $"[{m.A1:N2}, {m.A2:N2}, {m.A3:N2}, {m.A4:N2}]\n[{m.B1:N2}, {m.B2:N2}, {m.B3:N2}, {m.B4:N2}]\n[{m.C1:N2}, {m.C2:N2}, {m.C3:N2}, {m.C4:N2}]\n[{m.D1:N2}, {m.D2:N2}, {m.D3:N2}, {m.D4:N2}]\n";
        }
        public static void SetUniform4f(this ShaderProgram shader,string name, Vector4D v)
        {
            GLContainer.OpenGL.Uniform4(GLContainer.OpenGL.GetUniformLocation(shader.ShaderProgramObject, name), v.X, v.Y, v.Z, v.W);
        }

    }
}
