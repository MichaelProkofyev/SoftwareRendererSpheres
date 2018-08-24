using System;
using System.Threading;

namespace ManagedSphereDataViewer
{
	public class FrameBuffer
	{
		private byte[] _buffer;
        private float[] _zBuffer;
        private readonly int _width;
		private readonly int _height;
		private readonly int _bytesPerPixel;
        private readonly int _stride;

        public int renderedPixels;
        public int culledPixels;

        private object lockObject = new object();

        public FrameBuffer(int width, int height, int bytesPerPixel)
		{
			_width = width;
			_height = height;
			_bytesPerPixel = bytesPerPixel;
            _stride = _width * _bytesPerPixel;
            _buffer = new byte[width * height * bytesPerPixel];
            _zBuffer = new float[width * height];
        }

		public byte[] GetBuffer()
		{
			return _buffer;
		}

		public void Clear()
		{
            //Array.Clear() shouldn't be faster
            for (int i = 0; i < _buffer.Length; ++i)
			{
				_buffer[i] = 0;
			}

            for (int i = 0; i < _zBuffer.Length; ++i)
            {
                _zBuffer[i] = float.MaxValue;
            }

            renderedPixels = 0;
            culledPixels = 0;
        }

        public struct Pixel
        {
            public int x;
            public int y;
            public byte r;
            public byte b;
            public byte g;

            public Pixel(int x, int y, byte r, byte b, byte g)
            {
                this.x = x;
                this.y = y;
                this.r = r;
                this.b = b;
                this.g = g;
            }
        }

        public void RenderSphere(float screenX, float screenY, float screenZ, float screenRadius, uint color, Vector3 lightDir)
		{
            uint colorComponentRed = ((color & 0xFF0000) >> 16);
            uint colorComponentGreen = ((color & 0x00FF00) >> 8);
            uint colorComponentBlue = ((color & 0x0000FF) >> 0);


            int centerY = (int)(screenY * _width / 2 + _width / 2);
            int centerX = (int)(screenX * _width / 2 + _width / 2);

            //For square 1024x1024 buffer Rx == Ry
			int R = (int)(screenRadius * _width / 2);

            int Rsquared = R * R;
            int maxX = centerX + R * 2;
            int maxY = centerY + R * 2;
            //QUESTIONABLE AMOUNT OF LOOPING
            for (int x = centerX - R * 2; x <= maxX; ++x)
			{
                if (x < 0 || x >= _width)
                    continue;
                for (int y = centerY - R * 2; y <= maxY; ++y)
				{
					if(y < 0 || y >= _height)
						continue;

					int dx = x - centerX;
					int dy = y - centerY;

                    //Pixel too small to fit on a sphere
					if(dx * dx + dy * dy > Rsquared)
						continue;

                    //Skip the current pixel, if its obscured by other sphere
                    bool pixelVisible;
                    //lock (_zBuffer)
                    {
                        pixelVisible = screenZ < _zBuffer[x + y * _width];
                    }
                    if (pixelVisible)
                    {
                        ++renderedPixels;
                        //lock (_zBuffer)
                        {
                            _zBuffer[x + y * _width] = screenZ;
                        }

                        // Phong shading
                        {
                            Vector3 vec_normal = new Vector3();
                            vec_normal.x = dx;
                            vec_normal.y = dy;
                            vec_normal.z = (float)Math.Sqrt((Rsquared) - (dx * dx + dy * dy));
                            vec_normal.Normalize();

                            float NdotL = lightDir.Dot(vec_normal);
                            if (NdotL > 0)
                            {
                                Vector3 vec_eye = new Vector3();
                                vec_eye.x = lightDir.x + screenX;
                                vec_eye.y = lightDir.y + screenY;
                                vec_eye.z = lightDir.z + 1.0f;
                                vec_eye.Normalize();

                                float NdotHV = vec_eye.Dot(vec_normal);
                                float specular = (float)Math.Pow(NdotHV, 9); // shininess=9
                                float alpha = (NdotL + specular);
                                alpha = Math.Min(alpha, 1.0f); //Math.Min is slightly faster than if

                                byte r = (byte)(colorComponentRed * alpha);
                                byte g = (byte)(colorComponentGreen * alpha);
                                byte b = (byte)(colorComponentBlue * alpha);
                                r = Math.Min(r, byte.MaxValue);
                                g = Math.Min(g, byte.MaxValue);
                                b = Math.Min(b, byte.MaxValue);

                                SetPixel(x, y, b, g, r);
                            }
                            else
                            {
                                //Unlit pixel
                                SetPixel(x, y, 0, 0, 0);
                            }
                        }
                    }
                    else
                    {
                        ++culledPixels;
                    }
				}
			}
		}

		private void SetPixel(int x, int y, byte b, byte g, byte r)
		{
			x *= _bytesPerPixel;
			int index = x + y * _stride;
			_buffer[index] = b;
			_buffer[index + 1] = g;
			_buffer[index + 2] = r;
			_buffer[index + 3] = byte.MaxValue;
		}
	}
}
