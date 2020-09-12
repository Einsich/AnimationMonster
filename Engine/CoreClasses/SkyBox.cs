using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.Texture;
using SharpGL.VertexBuffers;
using SharpGL;
namespace Engine
{
    public class SkyBox
    {
        CubeMap cubeMap = new CubeMap();
        VertexBufferArray bufferArray = new VertexBufferArray();
        public uint cubeMapObject => cubeMap.textureObject;
        public uint vertexBufferArrayObject => bufferArray.VertexBufferArrayObject;
        public void Init()
        {
            OpenGL gl = GLContainer.OpenGL;
            string[] faces = { "posx.jpg", "negx.jpg", "posy.jpg", "negy.jpg", "posz.jpg", "negz.jpg" };
            cubeMap.Create(gl, faces, false);
            bufferArray.Create(gl);
            bufferArray.Bind(gl);
            VertexBuffer vertexBuffer = new VertexBuffer();
            vertexBuffer.Create(gl);
            vertexBuffer.Bind(gl);
            float[] cube = {
                    -1.0f,  1.0f, -1.0f,
                    -1.0f, -1.0f, -1.0f,
                     1.0f, -1.0f, -1.0f,
                     1.0f, -1.0f, -1.0f,
                     1.0f,  1.0f, -1.0f,
                    -1.0f,  1.0f, -1.0f,

                    -1.0f, -1.0f,  1.0f,
                    -1.0f, -1.0f, -1.0f,
                    -1.0f,  1.0f, -1.0f,
                    -1.0f,  1.0f, -1.0f,
                    -1.0f,  1.0f,  1.0f,
                    -1.0f, -1.0f,  1.0f,

                     1.0f, -1.0f, -1.0f,
                     1.0f, -1.0f,  1.0f,
                     1.0f,  1.0f,  1.0f,
                     1.0f,  1.0f,  1.0f,
                     1.0f,  1.0f, -1.0f,
                     1.0f, -1.0f, -1.0f,

                    -1.0f, -1.0f,  1.0f,
                    -1.0f,  1.0f,  1.0f,
                     1.0f,  1.0f,  1.0f,
                     1.0f,  1.0f,  1.0f,
                     1.0f, -1.0f,  1.0f,
                    -1.0f, -1.0f,  1.0f,

                    -1.0f,  1.0f, -1.0f,
                     1.0f,  1.0f, -1.0f,
                     1.0f,  1.0f,  1.0f,
                     1.0f,  1.0f,  1.0f,
                    -1.0f,  1.0f,  1.0f,
                    -1.0f,  1.0f, -1.0f,

                    -1.0f, -1.0f, -1.0f,
                    -1.0f, -1.0f,  1.0f,
                     1.0f, -1.0f, -1.0f,
                     1.0f, -1.0f, -1.0f,
                    -1.0f, -1.0f,  1.0f,
                     1.0f, -1.0f,  1.0f
            };
            vertexBuffer.SetData(gl, VertexAttributes.Position, cube, false, 3);
            vertexBuffer.Unbind(gl);
            bufferArray.Unbind(gl);
        }
    }
}
