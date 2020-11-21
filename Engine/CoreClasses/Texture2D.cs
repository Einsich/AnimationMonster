using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SharpGL.Texture
{
    public class Texture2D
    {
        public void Create(OpenGL gl)
        {
            //  Generate the texture object array.
            uint[] ids = new uint[1];
            gl.GenTextures(1, ids);
            textureObject = ids[0];
        }

        public void Delete(OpenGL gl)
        {
            gl.DeleteTextures(1, new[] { textureObject });
            textureObject = 0;
        }

        public void Bind(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureObject);
        }

        public void Unbind(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }

        public void SetParameter(OpenGL gl, uint parameterName, uint parameterValue)
        {
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, parameterName, parameterValue);
        }

        /// <summary>
        /// This function creates the texture from an image.
        /// </summary>
        /// <param name="gl">The OpenGL object.</param>
        /// <param name="image">The image.</param>
        /// <returns>True if the texture was successfully loaded.</returns>
        public void SetImage(OpenGL gl, Bitmap image, bool mipmap)
        {
            //	Get the maximum texture size supported by OpenGL.


            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            Width = (uint)image.Width;
            Height = (uint)image.Height;

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureObject);

            if (mipmap)
                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, OpenGL.GL_RGBA,
                    (int)Width, (int)Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                    bitmapData.Scan0);
            else
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA,
                    (int)Width, (int)Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                    bitmapData.Scan0);

            image.UnlockBits(bitmapData);
            if (mipmap)
            {
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
            }
            else
            {
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            }
        }
        public void SetImage(OpenGL gl, uint width, uint height, uint clamp)
        {
            Width = width;
            Height = height;
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureObject);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA32F,
                (int)Width, (int)Height, 0, OpenGL.GL_RGBA, OpenGL.GL_FLOAT,
                IntPtr.Zero);

            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, clamp);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, clamp);
            
        }

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public uint textureObject { get; private set; }
    }
    public class CubeMap
    {
        public void Create(OpenGL gl, string[] faces, bool mipmap)
        {
            uint[] ids = new uint[1];
            gl.GenTextures(1, ids);
            textureObject = ids[0];
            if (faces.Length != 6)
                throw new Exception("Not enough faces in cubeMap");

            gl.BindTexture(OpenGL.GL_TEXTURE_CUBE_MAP, textureObject);

            for (uint i = 0; i < faces.Length; i++)
            {
                string fileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Models\tex\" + faces[i]);
                Bitmap image = new Bitmap(fileName);
                BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);



                if (mipmap)
                    gl.Build2DMipmaps(OpenGL.GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, OpenGL.GL_RGBA,
                        image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                        bitmapData.Scan0);
                else
                    gl.TexImage2D(OpenGL.GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, OpenGL.GL_RGBA,
                        image.Width, image.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                        bitmapData.Scan0);

                image.UnlockBits(bitmapData);
                image.Dispose();
            }
            if (mipmap)
            {

                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_CLAMP_TO_EDGE);
            }
            else
            {
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
                gl.TexParameter(OpenGL.GL_TEXTURE_CUBE_MAP, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_CLAMP_TO_EDGE);
            }
        }

        public void Delete(OpenGL gl)
        {
            gl.DeleteTextures(1, new[] { textureObject });
            textureObject = 0;
        }

        public void Bind(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_CUBE_MAP, textureObject);
        }

        public void Unbind(OpenGL gl)
        {
            gl.BindTexture(OpenGL.GL_TEXTURE_CUBE_MAP, 0);
        }

        public uint textureObject { get; private set; }
    }
}
