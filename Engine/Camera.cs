using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ECS;
namespace Engine
{
    public class MainCameraTag { }
    public class Camera
    {
        private Matrix4x4 projection;
        public void CreateOrthographic(float width, float height, float zNear, float zFar) 
             { projection = Matrix4x4.CreateOrthographic(width, height, zNear, zFar); }
        public void CreatePerspective(float fieldOfView, float aspectRatio, float zNear, float zFar)
        { projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, zNear, zFar); }

       

        public Matrix4x4 GetProjection => projection;
        
        public static Entity GetMainCamera()
        {
            return EntitySystem.GetEntity(typeof(MainCameraTag));
        }



    }
}
