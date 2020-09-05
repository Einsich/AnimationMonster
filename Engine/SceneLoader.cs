using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Serialization;
using SharpGL.SceneGraph;

using System.IO;
namespace Engine
{
    class SceneLoader
    {
        public void f()
        {
            string fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Models\golem.fbx");
            Assimp.Scene s;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            s = importer.ImportFile(fileName);
            int t= s.AnimationCount;
        }
    }
}
