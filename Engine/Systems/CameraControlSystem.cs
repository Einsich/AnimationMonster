using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using ECS;

namespace Engine.Systems
{
    public class CameraControlSystem : BaseSystem
    {
        public CameraControlSystem() : base(2)
        {

            Entity camera = Entity.Create<Camera>();
            camera.GetComponent<Camera>().CreatePerspective(Mathf.DegToRad * 90, MainWindow.aspectRatio, 0.01f, 100000);
            camera.AddComponent<Transform>().position = new Vector3(0, 100, 150);
            //camera.AddComponent<ArcballCamera>().Init(150, 0, 0, new Vector3(0, 100, 0));
            camera.AddComponent<FreeCamera>();
            camera.AddComponent<MainCameraTag>();

            Input.KeyAction[Key.Space] += LockMainCamera;
        }

        public override void End()
        {

        }

        public override void Start()
        {
        }
        private void LockMainCamera(bool locked)
        {
            if(locked)
            EntitySystem.FirstQuery(new Action<MainCameraTag>((tag) => { tag.Lock = !tag.Lock; }));
        }

        public void Update(Transform transform, FreeCamera freeCamera, MainCameraTag mainCamera)
        {
            if (mainCamera.Lock)
                return;
            Vector3 d = transform.forward * (Input.GetKey(Key.W) - Input.GetKey(Key.S)) +
                transform.right * (Input.GetKey(Key.D) - Input.GetKey(Key.A)) +
                 transform.up * (Input.GetKey(Key.E) - Input.GetKey(Key.C));
            float pixToDeg = 1f;
            freeCamera.EulerRotation +=Input.mouseDelta * pixToDeg* Mathf.DegToRad;
            transform.position += d*10f;
            transform.SetRotation(freeCamera.EulerRotation.X, freeCamera.EulerRotation.Y, 0);
        }
        public void Update(Transform transform, ArcballCamera arcballCamera, MainCameraTag mainCamera)
        {

            arcballCamera.Distance -= Input.wheelDelta * 20;
            if (Input.MouseDown(MouseButton.Left))
            {
                float pixToDeg = 1.5f;
                arcballCamera.Rotation -= Input.mouseDelta * pixToDeg * Mathf.DegToRad;
            }
            float y = arcballCamera.Rotation.Y;
            float x = arcballCamera.Rotation.X + Mathf.PI * 0.5f;
            Vector3 p = new Vector3(Mathf.Cos(x) * Mathf.Cos(y), Mathf.Sin(y), Mathf.Sin(x) * Mathf.Cos(y));
            transform.position = arcballCamera.Target + arcballCamera.Distance * p;
            transform.SetRotation(-arcballCamera.Rotation.X, -arcballCamera.Rotation.Y, 0);
        }

    }
}
