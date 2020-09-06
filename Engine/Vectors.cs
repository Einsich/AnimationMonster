using System;
using System.Numerics;

namespace Engine
{
    class Mathf
    {
        public static float Sqrt(float x) => (float)Math.Sqrt(x);
        public static float Abs(float x) => x < 0 ? -x : x;

        /// <param name="x">Радианы </param>
        public static float Sin(float x) => (float)Math.Sin(x);

        /// <param name="x">Радианы </param>
        public static float Cos(float x) => (float)Math.Cos(x);
        public static float Acos(float cos) => (float)Math.Acos(cos);
        public static float PI => (float)(Math.PI);
        public static float RadianToDegrees(float radian) => radian / PI * 180;
        public static float DegreesToRadian(float degrees) => degrees / 180f * PI;

    }
    static class Extensions
    {
        /// <summary>
        /// Возвращает ориентированную площадь параллепипеда, если поворот от a к b положительный, то и плозадь положительная
        /// </summary>
        public static float Square(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
        public static float[] ToArray(this Vector2 a) => new float[] { a.X, a.Y };
        public static float[] ToArray(this Vector3 a) => new float[] { a.X,a.Y,a.Z };
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

    /*public struct Vector2
    {
        
        public float x, y;
        public Vector2(float X = 0, float Y = 0) => (x, y) = (X, Y);
        public float SqrtMagnitude => x * x + y * y;
        public float Magnitude =>Mathf.Sqrt(x * x + y * y);
        public Vector2 Normalized => this / Magnitude;

        public void Normalize() => this /= Magnitude;
        public float Angle(Vector2 a, Vector2 b) => Mathf.Acos(Dot(a, b) / Mathf.Sqrt(a.SqrtMagnitude * b.SqrtMagnitude));
        public Vector2 Lerp(Vector2 a, Vector2 b, float t) => (b - a) * t + a;
        public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
        /// <summary>
        /// Возвращает ориентированную площадь параллепипеда, если поворот от a к b положительный, то и плозадь положительная
        /// </summary>
        public static float Square(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        public static Vector2 operator +(Vector2 l, Vector2 r) => new Vector2(l.x + r.x, l.y + r.y);
        public static Vector2 operator -(Vector2 l, Vector2 r) => new Vector2(l.x - r.x, l.y - r.y);
        public static Vector2 operator *(Vector2 l, Vector2 r) => new Vector2(l.x * r.x, l.y * r.y);
        public static Vector2 operator *(float t, Vector2 r) => new Vector2(t * r.x, t * r.y);
        public static Vector2 operator *(Vector2 r, float t) => new Vector2(t * r.x, t * r.y);
        public static Vector2 operator /(Vector2 r, float t) => new Vector2(r.x / t, r.y / t);
        public float[] ToArray() => new float[] { x, y };
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float X = 0, float Y = 0, float Z = 0) => (x, y, z) = (X, Y, Z);
        public static Vector3 right => new Vector3(1, 0, 0);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public float SqrtMagnitude => x * x + y * y + z * z;
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        public Vector3 Normalized => this / Magnitude;

        public void Normalize() => this /= Magnitude;
        public float Angle(Vector3 a, Vector3 b) => Mathf.Acos(Dot(a, b) / Mathf.Sqrt(a.SqrtMagnitude * b.SqrtMagnitude));
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => (b - a) * t + a;
       
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => -new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static Vector3 operator +(Vector3 l, Vector3 r) => new Vector3(l.x + r.x, l.y + r.y, l.z + r.z);
        public static Vector3 operator -(Vector3 l, Vector3 r) => new Vector3(l.x - r.x, l.y - r.y, l.z - r.z);
        public static Vector3 operator -(Vector3 r) => new Vector3(-r.x, -r.y, -r.z);
        public static Vector3 operator *(Vector3 l, Vector3 r) => new Vector3(l.x * r.x, l.y * r.y, l.z * r.z);
        public static Vector3 operator *(float t, Vector3 r) => new Vector3(t * r.x, t * r.y, t * r.z);
        public static Vector3 operator *(Vector3 r, float t) => new Vector3(t * r.x, t * r.y, t * r.z);
        public static Vector3 operator /(Vector3 r, float t) => new Vector3(r.x / t, r.y / t, r.z / t);
        public float[] ToArray() => new float[] { x, y,z };
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float X = 0, float Y = 0, float Z = 0, float W = 0) => (x, y, z, w) = (X, Y, Z, W);
        public float SqrtMagnitude => x * x + y * y + z * z;
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        public Vector4 Normalized => this / Magnitude;

        public void Normalize() => this /= Magnitude;
        public Vector4 Lerp(Vector4 a, Vector4 b, float t) => (b - a) * t + a;
        public static Vector4 operator +(Vector4 l, Vector4 r) => new Vector4(l.x + r.x, l.y + r.y, l.z + r.z, l.w + r.w);
        public static Vector4 operator -(Vector4 l, Vector4 r) => new Vector4(l.x - r.x, l.y - r.y, l.z - r.z, l.w - r.w);
        public static Vector4 operator *(Vector4 l, Vector4 r) => new Vector4(l.x * r.x, l.y * r.y, l.z * r.z, l.w * r.w);
        public static Vector4 operator *(float t, Vector4 r) => new Vector4(t * r.x, t * r.y, t * r.z, t * r.w);
        public static Vector4 operator *(Vector4 r, float t) => new Vector4(t * r.x, t * r.y, t * r.z, t * r.w);
        public static Vector4 operator /(Vector4 r, float t) => new Vector4(r.x / t, r.y / t, r.z / t,r.w / t);
        public float[] ToArray() => new float[] { x, y,z,w };
    }
    */
    public struct Vector4D
    {
        public float X, Y, Z, W;
        public Vector4D(Assimp.Vector3D v) => (X, Y, Z, W) = (v.X, v.Y, v.Z, 0);
        public Vector4D(Assimp.Vector3D v, float w) => (X, Y, Z, W) = (v.X, v.Y, v.Z, w);
        public Vector4D(float x, float y, float z, float w) => (X, Y, Z, W) = (x, y, z, w);
        public float this[int i]
        {
            get
            {
                float t = 0;
                switch (i)
                {
                    case 0: t = X; break;
                    case 1: t = Y; break;
                    case 2: t = Z; break;
                    case 3: t = W; break;
                }
                return t;
            }
            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                }
            }
        }
    }
}