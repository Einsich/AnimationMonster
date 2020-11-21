using System;
using System.Collections.Generic;
using System.Windows;
using SharpGL;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph;
using SharpGL.WPF;

using ECS;
namespace Engine
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ConsoleManager.Show();
        }
        public static float aspectRatio;
        void Initialize(OpenGL openGL)
        {

            aspectRatio = (float)Width / (float)Height;
            GLContainer.OpenGL = openGL;
            EntitySystem.AddSystem<Systems.RenderSystem>();
            EntitySystem.AddSystem<Systems.BoneRenderSystem>();
            EntitySystem.AddSystem<Systems.HeightMapRenderSystem>();
            EntitySystem.AddSystem<Systems.CameraControlSystem>();
            EntitySystem.AddSystem<Systems.SkyBoxRenderSystem>();
            SceneLoader.LoadScene();
            Focus();
            KeyDown += Input.KeyDownEventListener;
            KeyUp += Input.KeyUpEventListener;
            MouseDown += Input.MouseDownEventListener;
            MouseUp += Input.MouseUpEventListener;
            MouseMove += Input.MouseMoveEventListener;
            MouseEnter += Input.MouseEnterEventListener;
            MouseLeave += Input.MouseLeaveEventListener;
            MouseWheel += Input.MouseWheelEventListener;

        }
        private void OpenGLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
        }
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            int[] viewport = new int[4];
            GLContainer.OpenGL.GetInteger(SharpGL.Enumerations.GetTarget.Viewport, viewport);
            GLContainer.Width = viewport[2];
            GLContainer.Height = viewport[3];
            OpenGL gl = args.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            Time.Update();
            EntitySystem.Update();
            
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the OpenGLControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            Initialize(gl);
        }
    }
   
}
