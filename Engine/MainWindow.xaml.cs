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
    static class GLContainer
    {
        public static OpenGL OpenGL;
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {


        }
        public static float aspectRatio;
        void Initialize(OpenGL openGL)
        {

            aspectRatio = (float)Width / (float)Height;
            GLContainer.OpenGL = openGL;
            EntitySystem.AddSystem<StartSystem>();
        }
        private void OpenGLControl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            
        }
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            OpenGL gl = args.OpenGL;
           
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
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
