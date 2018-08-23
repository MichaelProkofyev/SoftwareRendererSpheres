using System;

namespace ManagedSphereDataViewer
{
	public class FrameBuffer
	{
		private byte[] _buffer;
        private double[] _zBuffer;
        private readonly int _width;
		private readonly int _height;
		private readonly int _bytesPerPixel;

        public int renderedPixels;
        public int culledPixels;

        private object lockObject = new object();

        public FrameBuffer(int width, int height, int bytesPerPixel)
		{
			_width = width;
			_height = height;
			_bytesPerPixel = bytesPerPixel;
			_buffer = new byte[width * height * bytesPerPixel];
            _zBuffer = new double[width * height];
        }

		public byte[] GetBuffer()
		{
			return _buffer;
		}

		public void Clear()
		{
            //May be even slower
            //Array.Clear(_buffer, 0, _buffer.Length);

            for (int i = 0; i < _buffer.Length; ++i)
			{
				_buffer[i] = 0;
			}

            for (int i = 0; i < _zBuffer.Length; ++i)
            {
                _zBuffer[i] = double.MaxValue;
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

		public void RenderSphere(float screenX, float screenY, double screenZ, float screenRadius, uint color, Vector3 lightDir)
		{
            int centerX = (int)(screenX * _width / 2 + _width / 2);
			int centerY = (int)(screenY * _width / 2 + _width / 2);

			int RX = (int)(screenRadius * _width / 2);
			int RY = (int)(screenRadius * _width / 2);

            //int sphereWidth = RX * 4;
            //int sphereHeight = RY * 4;
            //Pixel[] spherePixels = new Pixel[sphereWidth * sphereHeight];
            //int pixNumber = 0;

            //QUESTIONABLE AMOUNT OF LOOPING
            for (int x = centerX - RX * 2; x <= centerX + RX * 2; ++x)
			{
				for(int y = centerY - RY * 2; y <= centerY + RY * 2; ++y)
				{
					int dx = x - centerX;
					int dy = y - centerY;

                    //Pixel too smal to fit on a sphere
					if(dx * dx + dy * dy > RX * RY)
						continue;
					if(y < 0 || y >= _height)
						continue;
					if(x < 0 || x >= _width)
						continue;

                    if (screenZ < _zBuffer[x + y * _width])
                    {
                        ++renderedPixels;
                        _zBuffer[x + y * _width] = screenZ;

                        // Phong shading
                        {
                            Vector3 vec_normal = new Vector3();
                            vec_normal.x = dx;
                            vec_normal.y = dy;
                            vec_normal.z = (float)Math.Sqrt((RX * RY) - (dx * dx + dy * dy));
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
                                if (alpha > 1.0f)
                                {
                                    alpha = 1.0f;
                                }


                                byte b = (byte)(((color & 0x0000FF) >> 0) * alpha);
                                byte g = (byte)(((color & 0x00FF00) >> 8) * alpha);
                                byte r = (byte)(((color & 0xFF0000) >> 16) * alpha);
                                r = Math.Min(r, byte.MaxValue);
                                g = Math.Min(g, byte.MaxValue);
                                b = Math.Min(b, byte.MaxValue);

                                //Pixel newPixel = new Pixel(x, y, r, b, g);
                                //spherePixels[pixNumber++] = newPixel;
                                SetPixel(x, y, b, g, r);
                            }
                            else
                            {
                                //Do we need this?
                                //SetPixel(x, y, 0, 0, 0);
                            }
                        }
                    }
                    else
                    {
                        //var asdf = _zBuffer[x + y];
                        ++culledPixels;
                    }
				}
			}
            //return spherePixels;
		}

        public void SetSphereToBuffer(Pixel[] pixels)
        {
            for (int i = 0; i < pixels.Length; ++i)
            {
                Pixel p = pixels[i];
                SetPixel(p.x, p.y, p.b, p.g, p.r);
            }
        }

		private void SetPixel(int x, int y, byte b, byte g, byte r)
		{
			x *= _bytesPerPixel;
			int stride = _width * _bytesPerPixel;
			int index = x + y * stride;
			_buffer[index] = b;
			_buffer[index + 1] = g;
			_buffer[index + 2] = r;
			_buffer[index + 3] = byte.MaxValue;
		}
	}
}
