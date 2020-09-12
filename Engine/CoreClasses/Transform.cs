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
        public Vector3 position { get => transform.Translation; set => transform.Translation = value; }
        //public Vector3 EulerRotation { get =>rotation; set => euler = value; }
        public Quaternion QuaternionRotation => Quaternion.CreateFromRotationMatrix(transform);
        public void Rotate(Quaternion rotation) =>        
            transform = Matrix4x4.Transform(transform, rotation);
        public void SetRotation(float yaw, float pitch, float roll)
            => transform = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix4x4.CreateTranslation(position);

        public void LookAt(Vector3 target)
            => transform = Matrix4x4.CreateLookAt(position, target, Vector3.UnitY);
        public Vector3 forward => new Vector3(-transform.M31, -transform.M32, -transform.M33);
        public Vector3 up => new Vector3(transform.M21, transform.M22, transform.M23);
        public Vector3 right => new Vector3(transform.M11, transform.M12, transform.M13);

        public Matrix4x4 GetMatrix => transform;


    }
}
