using SharpGL;
using SharpGL.Shaders;
using SharpGL.Texture;
using SharpGL.VertexBuffers;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;

namespace Engine.CoreClasses
{
    public class FrameBuffer
    {
        VertexBufferArray vertexBufferArray;
        uint frameBuffer;
        public FrameBuffer()
        {
            OpenGL gl = GLContainer.OpenGL;
            float[] fbo_vertices = {-1.0f, -1.0f,
                                    1.0f, -1.0f,
                                    -1.0f,  1.0f,
                                    -1.0f,  1.0f,
                                    1.0f, -1.0f,
                                    1.0f,  1.0f};

            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);
            var verticesVertexBuffer = new VertexBuffer();
            verticesVertexBuffer.Create(gl);
            verticesVertexBuffer.Bind(gl);
            verticesVertexBuffer.SetData(gl, VertexAttributes.Position, fbo_vertices, false, 2);
            vertexBufferArray.Unbind(gl);
            uint[] FramebufferName = new uint[1];
            gl.GenFramebuffersEXT(1, FramebufferName);
            frameBuffer = FramebufferName[0];
        }
        public void SetRenderTarget(Texture2D texture)
        {
            OpenGL gl = GLContainer.OpenGL;
            vertexBufferArray.Bind(gl);
            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, frameBuffer);
            gl.FramebufferTexture(OpenGL.GL_FRAMEBUFFER_EXT, OpenGL.GL_COLOR_ATTACHMENT0_EXT, texture.textureObject, 0);
            gl.Viewport(0, 0, (int)texture.Width, (int)texture.Height);
        }
        public void Render()
        {

            OpenGL gl = GLContainer.OpenGL;
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);
            vertexBufferArray.Unbind(gl);
            gl.BindFramebufferEXT(OpenGL.GL_FRAMEBUFFER_EXT, 0);

            gl.Viewport(0, 0, GLContainer.Width, GLContainer.Height);

        }
    }
    public class HeightMap
    {
        List<ProcessedMesh> meshes = new List<ProcessedMesh>();
        Vector4D heightMapSize, heightMapProperty;
        FrameBuffer frameBuffer;
        const int mapCount = 3;
        Texture2D[] heightMap = new Texture2D[mapCount];
        bool processErosion = false;
        uint width, height;
        public void CreateTerrain(int height, int width, float horScale, float verScale)
        {
            (this.width, this.height) = ((uint)width, (uint)height);
            heightMapSize = new Vector4D(1f / width, 1f / height, width, height);
            heightMapProperty = new Vector4D(verScale, horScale, 0, 0);
            int tile = 100;
            for (int x = 0; x < width; x += tile)
                for (int y = 0; y < height; y += tile)
                {
                    int tileX = width - x > tile ? tile : width - x;
                    int tileY = height - y > tile ? tile : height - y;
                    meshes.Add(ProcessedMesh.TerrainPart(x, y, tileX, tileY, horScale));
                }
            ShaderContainer.AddShader("perlin_noise", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});
            ShaderContainer.AddShader("erosion", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});

            ShaderContainer.AddShader("gradient_map", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});
            ShaderContainer.AddShader("tex_copy", new Dictionary<uint, string>()
            { { VertexAttributes.Position, "Position" }});

            Texture2D heightMapEurope = new Texture2D();
            OpenGL gl = GLContainer.OpenGL;
            Bitmap eu = new Bitmap(@"Models\tex\hmp.png");
            heightMapEurope.Create(gl);
            heightMapEurope.SetImage(gl, eu, false);
            TextureContainer.AddTexture("europaHeightMap", heightMapEurope);
            uint[] clamps = new uint[mapCount] { OpenGL.GL_CLAMP_TO_EDGE, OpenGL.GL_CLAMP_TO_EDGE, OpenGL.GL_CLAMP };
            for (int i = 0; i < heightMap.Length; i++)
            {
                heightMap[i] = new Texture2D();
                heightMap[i].Create(GLContainer.OpenGL);
                heightMap[i].SetImage(GLContainer.OpenGL, (uint)width, (uint)height, clamps[i]);
            }
            frameBuffer = new FrameBuffer();
            eu.Dispose();
            InitHeightMapOnGPU();

            Input.KeyAction[Key.Escape] += ToggleErosion;
            Input.KeyDownAction[Key.F8] += InitHeightMapOnGPU;

        }
        bool EU = false;
        private void InitHeightMapOnGPU()
        {

            OpenGL gl = GLContainer.OpenGL;
            if (EU)
            {
                var copyShader = ShaderContainer.GetShader("tex_copy");
                copyShader.Bind(gl);
                gl.ActiveTexture(OpenGL.GL_TEXTURE0);
                TextureContainer.GetTexture("europaHeightMap").Bind(gl);
                frameBuffer.SetRenderTarget(heightMap[0]);
                frameBuffer.Render();
                heightMap[0].Unbind(gl);
                TextureContainer.GetTexture("europaHeightMap").Unbind(gl);
                copyShader.Unbind(gl);
            }
            else
            {
                var shader = ShaderContainer.GetShader("perlin_noise");
                shader.Bind(gl);
                frameBuffer.SetRenderTarget(heightMap[0]);
                frameBuffer.Render();
                heightMap[0].Unbind(gl);
                shader.Unbind(gl);
            }
        }
        int counter = 0;
        public void Render(OpenGL gl, float[] viewProjection, float[] model, Transform cameraTransform)
        {
            if (processErosion)
            {
                gl.ActiveTexture(OpenGL.GL_TEXTURE0);
                heightMap[0].Bind(gl);

                GetGradient(gl, heightMap[2]);

                gl.ActiveTexture(OpenGL.GL_TEXTURE1);
                heightMap[2].Bind(gl);

                var erosion_shader = ShaderContainer.GetShader("erosion");

                erosion_shader.Bind(gl);
                erosion_shader.SetUniform3(gl, "time", Time.deltaTime, Time.time, 0);
                erosion_shader.SetUniform4f("heightMapSize", heightMapSize);
                erosion_shader.SetUniform4f("heightMapProperty", heightMapProperty);
                erosion_shader.SetUniform1(gl, "first_time_factor", counter == 0 ? 0 : 1);
                frameBuffer.SetRenderTarget(heightMap[1]);
                frameBuffer.Render();
                erosion_shader.Unbind(gl);
                heightMap[0].Unbind(gl);
                heightMap[1].Unbind(gl);
                heightMap[2].Unbind(gl);

                Texture2D temp = heightMap[0];
                heightMap[0] = heightMap[1];
                heightMap[1] = temp;
                //processErosion = false;
                counter++;
            }

            ShaderProgram shader = ShaderContainer.GetShader("height_map");
            shader.Bind(gl);
            shader.SetUniformMatrix4(gl, "ViewProjection", viewProjection);
            shader.SetUniformMatrix4(gl, "Model", model);

            shader.SetUniform4f("heightMapSize", heightMapSize);
            shader.SetUniform4f("heightMapProperty", heightMapProperty);
            shader.SetUniform3(gl, "LightPosition", 0.2f, 1, 0.3f);

            shader.SetUniform3(gl, "DiffuseMaterial", 0, 0.1f, 0.3f);
            shader.SetUniform3(gl, "AmbientMaterial", 0, 0.2f, 0.5f);
            shader.SetUniform3(gl, "SpecularMaterial", 0.3f, 0.3f, 0.3f);
            var camP = cameraTransform.position;
            shader.SetUniform3(gl, "CameraPosition", camP.X, camP.Y, camP.Z);
            shader.SetUniform1(gl, "Shininess", 0.1f);
            shader.SetUniform1(gl, "Reflectance", 0.1f);


            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            heightMap[0].Bind(gl);
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            heightMap[2].Bind(gl);
            foreach (var mesh in meshes)
            {
                var vertexBufferArray = mesh.vertexBufferArray;
                vertexBufferArray.Bind(gl);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, mesh.vertexCount);
            }
            shader.Unbind(gl);
            heightMap[0].Unbind(gl);
            heightMap[2].Unbind(gl);
        }
        public void ToggleErosion(bool s) => processErosion = s ? !processErosion : processErosion;
        void GetGradient(OpenGL gl, Texture2D gradientTarget)
        {
            var gradient_map = ShaderContainer.GetShader("gradient_map");

            gradient_map.Bind(gl);
            gradient_map.SetUniform4f("heightMapSize", heightMapSize);
            gradient_map.SetUniform4f("heightMapProperty", heightMapProperty);
            frameBuffer.SetRenderTarget(gradientTarget);
            frameBuffer.Render();
            gradient_map.Unbind(gl);
        }
    }
}
