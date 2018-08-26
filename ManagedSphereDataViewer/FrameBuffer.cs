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

        private object lockObject = new object();
        private const int shininess = 9;

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
            for (int i = 0; i < _buffer.Length; ++i)
            {
                _buffer[i] = 0;
            }

            for (int i = 0; i < _zBuffer.Length; ++i)
            {
                _zBuffer[i] = float.MaxValue;
            }
        }

        public void RenderSphere(SphereElement sphere, float rotationSin, float rotationCos, float colorLerpProgress,  Vector3 lightDir)
        {
            float fX = sphere.x * rotationSin - sphere.z * rotationCos;
            float fY = sphere.y;
            float fZ = sphere.screenZ;

            fZ += 1.5f;

            //Skip the sphere that's too close to camera frustum
            if (fZ < 0.001f)
                return;

            //Weak perspective projection
            float screenX = fX / fZ;
            float screenY = fY / fZ;
            float screenZ = fZ;
            float screenRadius = sphere.r / fZ;

            Vector3Byte sphereColor = Vector3Byte.Lerp(sphere.colorA, sphere.colorB, colorLerpProgress);

            int centerY = (int)(screenY * _width / 2 + _width / 2);
            int centerX = (int)(screenX * _width / 2 + _width / 2);

            //For square 1024x1024 buffer - Rx == Ry
            int R = (int)(screenRadius * _width / 2);

            int Rsquared = R * R;
            int minX = Math.Max(centerX - R * 2, 0);
            int maxX = Math.Min(centerX + R * 2, _width - 1);
            int minY = Math.Max(centerY - R * 2, 0);
            int maxY = Math.Min(centerY + R * 2, _height - 1);

            for (int x = minX; x <= maxX; ++x)
            {
                for (int y = minY; y <= maxY; ++y)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    int dx2 = dx * dx;
                    int dy2 = dy * dy;

                    //Skip pixels which are not part of a circle
                    if (dx2 + dy2 > Rsquared)
                        continue;

                    bool pixelVisible;
                    //lock (_zBuffer)
                    {
                        pixelVisible = screenZ < _zBuffer[x + y * _width];
                    }
                    if (pixelVisible)
                    {
                        //lock (_zBuffer)
                        {
                            //Writing a float to _zBuffer array is atomic, no danger of corruption
                            _zBuffer[x + y * _width] = screenZ;
                        }

                        // Phong shading
                        {
                            Vector3 vec_normal = new Vector3();
                            vec_normal.x = dx;
                            vec_normal.y = dy;
                            vec_normal.z = (float)Math.Sqrt(Rsquared - (dx2 + dy2));
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
                                float specular = NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV; // shininess=9
                                float alpha = NdotL + specular;
                                alpha = Math.Min(alpha, 1.0f);

                                byte r = (byte)(sphereColor.x * alpha);
                                byte g = (byte)(sphereColor.y * alpha);
                                byte b = (byte)(sphereColor.z * alpha);

                                SetPixel(x, y, b, g, r);
                            }
                            else
                            {
                                //Unlit pixel
                                SetPixel(x, y, 0, 0, 0);
                            }
                        }
                    }
                }
            }
        }


        public void RenderSphere(float screenX, float screenY, float screenZ, float screenRadius, Vector3Byte sphereColor, Vector3 lightDir)
		{
            int centerY = (int)(screenY * _width / 2 + _width / 2);
            int centerX = (int)(screenX * _width / 2 + _width / 2);

            //For square 1024x1024 buffer - Rx == Ry
			int R = (int)(screenRadius * _width / 2);

            int Rsquared = R * R;
            int minX = Math.Max(centerX - R * 2, 0);
            int maxX = Math.Min(centerX + R * 2, _width - 1);
            int minY = Math.Max(centerY - R * 2, 0);
            int maxY = Math.Min(centerY + R * 2, _height - 1);

            for (int x = minX; x <= maxX; ++x)
			{
                for (int y = minY; y <= maxY; ++y)
				{
					int dx = x - centerX;
					int dy = y - centerY;
                    int dx2 = dx * dx;
                    int dy2 = dy * dy;

                    //Skip pixels which are not part of a circle
					if(dx2 + dy2 > Rsquared)
						continue;

                    bool pixelVisible;
                    //lock (_zBuffer)
                    {
                        pixelVisible = screenZ < _zBuffer[x + y * _width];
                    }
                    if (pixelVisible)
                    {
                        //lock (_zBuffer)
                        {
                            //Writing a float to _zBuffer array is atomic, no danger of corruption
                            _zBuffer[x + y * _width] = screenZ;
                        }

                        // Phong shading
                        {
                            Vector3 vec_normal = new Vector3();
                            vec_normal.x = dx;
                            vec_normal.y = dy;
                            vec_normal.z = (float)Math.Sqrt(Rsquared - (dx2 + dy2));
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
                                float specular = NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV * NdotHV; // shininess=9
                                float alpha = NdotL + specular;
                                alpha = Math.Min(alpha, 1.0f);

                                byte r = (byte)(sphereColor.x * alpha);
                                byte g = (byte)(sphereColor.y * alpha);
                                byte b = (byte)(sphereColor.z * alpha);

                                SetPixel(x, y, b, g, r);
                            }
                            else
                            {
                                //Unlit pixel
                                SetPixel(x, y, 0, 0, 0);
                            }
                        }
                    }
				}
			}
		}

        private void SetPixel(int x, int y, byte b, byte g, byte r)
		{
			x *= _bytesPerPixel;
			int index = x + y * _stride;
            //lock (_buffer)
            {
			    _buffer[index] = b;
			    _buffer[index + 1] = g;
			    _buffer[index + 2] = r;
			    _buffer[index + 3] = byte.MaxValue;
            }
		}
	}
}
