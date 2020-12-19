using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
namespace Engine.CoreClasses
{
    class AnimationTree
    {
        public List<AnimationNode> animationNodes;
    }

    public class AnimationNode
    {
        public string name;
        public List<Assimp.Quaternion> rotations = new List<Assimp.Quaternion>();
        public List<Assimp.Vector3D> translations = new List<Assimp.Vector3D>();
        public Assimp.Vector3D GetLerpedPosition(int currentFrame, int nextFrame, float time)
        {
            return translations.Count > 0 ? Mathf.Lerp(translations[currentFrame], translations[nextFrame], time) : new Vector3D();
        }
        public Assimp.Quaternion GetLerpedRotation(int currentFrame, int nextFrame, float time)
        {
            return rotations.Count > 0 ? Assimp.Quaternion.Slerp(rotations[currentFrame], rotations[nextFrame], time) : new Quaternion();
        }
    }
}
