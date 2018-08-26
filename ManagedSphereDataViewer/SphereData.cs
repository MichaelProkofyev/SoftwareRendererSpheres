using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Xml;

namespace ManagedSphereDataViewer
{
	public class SphereData
	{
		private List<SphereElement> _spheres;

        public SphereData(string filename, bool isXmlFile)
        {
            Random rand = new Random(1);
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            void CoordinatesToSphere(string xString, string yString, string zString)
            {
                float x = float.Parse(xString, NumberStyles.Any, ci);
                float y = float.Parse(yString, NumberStyles.Any, ci);
                float z = float.Parse(zString, NumberStyles.Any, ci);

                y -= 60.0f;
                z -= 50.0f;

                x *= 0.01f;
                y *= 0.01f;
                z *= 0.01f;

                float r = 5.0f + 5.0f * (float)rand.NextDouble();
                r *= 0.004f;

                var sphere = new SphereElement(x, y, z, r, 0);
                _spheres.Add(sphere);
            }

            _spheres = new List<SphereElement>();


            if (isXmlFile)
            {
                var xmlDoc = new XmlDocument();
                var filePath = Path.GetFullPath(filename);
                xmlDoc.Load(filePath);
                foreach (XmlNode vectorNode in xmlDoc.DocumentElement.ChildNodes)
                {
                    var coordinates = vectorNode.ChildNodes;
                    CoordinatesToSphere(coordinates[0].InnerText, coordinates[1].InnerText, coordinates[2].InnerText);
                }
            }
            else
            {
                char[] splitChar = new char[] { ' ' };
                var lines = Helpers.ReadLines(() => new FileStream(filename, FileMode.Open), Encoding.UTF8).ToList();
                foreach (var line in lines)
                {
                    string[] coordinates = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    CoordinatesToSphere(coordinates[0], coordinates[1], coordinates[2]);
                }
            }
		}

		public void Render(FrameBuffer frameBuffer, float rotation, float colorLerpProgress, Vector3 lightDirection)
		{
			float rotationSin = (float)Math.Sin(rotation);
			float rotationCos = (float)Math.Cos(rotation);

            float minScreenZ = float.MaxValue;
            float maxScreenZ = float.MinValue;
            foreach (var sphere in _spheres)
			{
				sphere.screenZ = sphere.x * rotationCos + sphere.z * rotationSin;
                if (sphere.screenZ < minScreenZ)
                {
                    minScreenZ = sphere.screenZ;
                }
                if(maxScreenZ < sphere.screenZ)
                {
                    maxScreenZ = sphere.screenZ;
                }
            }

            _spheres.AsEnumerable()
                .AsParallel()
                .ForAll((sphere) =>
                {
                    float fX = sphere.x * rotationSin - sphere.z * rotationCos;
                    float fY = sphere.y;
                    float fZ = sphere.screenZ;

                    fZ += 1.5f;

                    //Skip the sphere that's too close to camera frustum
                    if (fZ < 0.001f)
                        return;

                    //Weak perspective projection
                    float fScreenX = fX / fZ;
                    float fScreenY = fY / fZ;
                    float fScreenZ = fZ;

                    //float fogAmount = fZ * .8f;
                    Vector3Byte sphereColor = Vector3Byte.Lerp(sphere.colorA, sphere.colorB, colorLerpProgress);
                    frameBuffer.RenderSphere(fScreenX, fScreenY, fScreenZ, sphere.r / fZ, sphereColor, lightDirection);
                });

            //Single-threaded
            //_spheres.Sort(CompareSpheres);
            //_spheres.ForEach((sphere) => PrepareSphere(frameBuffer, sphere, s, c));
        }

		private int CompareSpheres(SphereElement s1, SphereElement s2)
		{
			return Math.Sign(s2.screenZ - s1.screenZ);
		}
	}
}
