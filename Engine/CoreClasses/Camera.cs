using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using ECS;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;
namespace Engine
{
    public class MainCameraTag
    {
        public bool Lock = true;
    }
    public class FreeCamera
    {
        public Vector2 EulerRotation;
    }
    public class ArcballCamera
    {
        public float Distance = 10;
        public Vector2 Rotation;
        public Vector3 Target;
        public void Init(float Radius, float degreeX, float degreeY, Vector3 target)
            => (Distance, Rotation, Target) = (Radius, new Vector2(degreeX, degreeY) * Mathf.DegToRad, target);
    }
    public class Camera
    {
        private Matrix4x4 projection;
        public void CreateOrthographic(float width, float height, float zNear, float zFar) 
             { projection = Matrix4x4.CreateOrthographic(width, height, zNear, zFar); }
        public void CreatePerspective(float fieldOfView, float aspectRatio, float zNear, float zFar)
        { projection = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, zNear, zFar); }

        public Matrix4x4 GetProjection => projection;

        static Entity mainEntity;
        public static (Camera, Transform) mainCamera
        {
            get
            {
                Camera camera = null;
                Transform cameraTransform = null;
                if (mainEntity != null)
                {
                    camera = mainEntity.GetComponent<Camera>();
                    cameraTransform = mainEntity.GetComponent<Transform>();
                    if (camera != null && cameraTransform != null)
                        return (camera, cameraTransform);
                }
                EntitySystem.FirstQuery(new Action<Entity, Camera, Transform, MainCameraTag>((entity, cam, tr, tag) => (mainEntity, camera, cameraTransform) = (entity, cam, tr)));
                return (camera, cameraTransform);
            }
        }
        public static Vector2D WorldToScreen(Vector3D world)
        {
            (Camera camera, Transform cameraTransform) = mainCamera;
            if (camera == null)
                return new Assimp.Vector2D(0, 0);
            Matrix4x4 view = cameraTransform.GetMatrix;
            Matrix4x4.Invert(view, out view);
            Assimp.Matrix4x4 matrix4x4 = (view * camera.GetProjection).ToAssimp();
            var clip = matrix4x4 * new Vector4D(world, 1);
            if(clip.Z <=0)
                return new Assimp.Vector2D(float.NaN, float.NaN);
            int[] viewport = new int[4];
            GLContainer.OpenGL.GetInteger(SharpGL.Enumerations.GetTarget.Viewport, viewport);
            Vector2D screen = new Vector2D(clip.X, clip.Y);
            screen = (screen * 0.5f / clip.W + new Vector2D(0.5f))*new Vector2D(viewport[2], viewport[3]);
            
            return screen;

        }



    }
}
