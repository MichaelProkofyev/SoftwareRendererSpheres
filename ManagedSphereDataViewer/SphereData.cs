using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Globalization;

namespace ManagedSphereDataViewer
{
	public class SphereData
	{
		private List<SphereElement> _spheres;
		private Vector3 _lightDirection;

		public SphereData(string filename)
		{
			_spheres = new List<SphereElement>();

			using(StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open)))
			{
				string line = null;
				char[] splitChar = new char[] { ' ' };
				Random rand = new Random(1);

                CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                ci.NumberFormat.CurrencyDecimalSeparator = ".";

                while (true)
				{
					line = reader.ReadLine();
					if(line == null)
					{
						break;
					}

					string[] coordinates = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
					float x = float.Parse(coordinates[0], NumberStyles.Any, ci);
					float y = float.Parse(coordinates[1], NumberStyles.Any, ci);
					float z = float.Parse(coordinates[2], NumberStyles.Any, ci);

					y -= 60.0f;
					z -= 50.0f;

					x *= 0.01f;
					y *= 0.01f;
					z *= 0.01f;

					float r = 5.0f + 5.0f * (float)rand.NextDouble();
					r *= 0.004f;

					uint BGRA = uint.MaxValue;

					BGRA = (BGRA << 8) | ((uint)rand.Next() & 0xff);
					BGRA = (BGRA << 8) | ((uint)rand.Next() & 0xff);
					BGRA = (BGRA << 8) | ((uint)rand.Next() & 0xff);

					var sphere = new SphereElement(x, y, z, r, 0, BGRA);
					_spheres.Add(sphere);
                }
			}

			_lightDirection = new Vector3(1.0f, -0.5f, 0.7f);
			_lightDirection.Normalize();
		}

		public void Render(FrameBuffer frameBuffer, float rotation)
		{
			float s = (float)Math.Sin(rotation);
			float c = (float)Math.Cos(rotation);

            double minScreenZ = double.MaxValue;
            double maxScreenZ = double.MinValue;
            foreach (var sphere in _spheres)
			{
				sphere.screenZ = sphere.X * c + sphere.Z * s;
                if (sphere.screenZ < minScreenZ)
                {
                    minScreenZ = sphere.screenZ;
                }
                if (sphere.screenZ > maxScreenZ)
                {
                    maxScreenZ = sphere.screenZ;
                }
            }


            _spheres.AsEnumerable()
                .AsParallel()
                .ForAll((sphere) => PrepareSphere(frameBuffer, sphere, s, c));

            //Single-threaded
            //_spheres.Sort(CompareSpheres);
            //_spheres.ForEach((sphere) => PrepareSphere(frameBuffer, sphere, s, c));
        }

        private void PrepareSphere(FrameBuffer frameBuffer, SphereElement sphere, float s, float c)
        {
            float fX = sphere.X * s - sphere.Z * c;
            float fY = sphere.Y;
            float fZ = sphere.screenZ;

            fZ += 1.5f;

            //Skip the sphere that's too close to camera frustum
            if (fZ < 0.001f)
                return;

            //Weak perspective projection
            float fScreenX = fX / fZ;
            float fScreenY = fY / fZ;
            float fScreenZ = fZ;
            frameBuffer.RenderSphere(fScreenX, fScreenY, fScreenZ, sphere.R / fZ, sphere.Color, _lightDirection);
        }

		private int CompareSpheres(SphereElement s1, SphereElement s2)
		{
			return Math.Sign(s2.screenZ - s1.screenZ);
		}
	}
}
