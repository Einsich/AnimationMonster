using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ECS;
namespace Engine
{
    public class Transform
    {

        private Matrix4x4 transform = Matrix4x4.Identity;
        private List<Transform> childs = new List<Transform>();
        private Transform parent;
        public Vector3 position { get => transform.Translation; set => transform.Translation = value; }
        //public Vector3 EulerRotation { get =>rotation; set => euler = value; }
        public Quaternion QuaternionRotation => Quaternion.CreateFromRotationMatrix(transform);
        public void Rotate(Quaternion rotation) =>        
            transform = Matrix4x4.Transform(transform, rotation);
        public Vector3 right => new Vector3(transform.M13, transform.M23, transform.M33);
        public Vector3 up => new Vector3(transform.M12, transform.M22, transform.M32);
        public Vector3 forward => new Vector3(transform.M11, transform.M21, transform.M31);

        public Matrix4x4 GetMatrix => transform;
        public void f()
        {

        }

        public void Constructor()
        {
            Matrix4x4 transform = Matrix4x4.Identity;
            childs.Clear();
        }

        public void Destuctor()
        {
        }
    }
}
