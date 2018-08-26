using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
    public class SphereData
    {
        public List<SphereElement> Spheres { get; private set; }

        public SphereData(string filename, bool isXmlFile)
        {
            Random rand = new Random(1);
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            //Make sure that '.' is used as separator on local machine before parsing
            ci.NumberFormat.NumberDecimalSeparator = ".";

            SphereElement CoordinatesToSphere(string xString, string yString, string zString)
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

                return new SphereElement(x, y, z, r, 0);
            }

            Spheres = new List<SphereElement>();


            if (isXmlFile)
            {
                var xmlDoc = new XmlDocument();
                var filePath = Path.GetFullPath(filename);
                xmlDoc.Load(filePath);
                foreach (XmlNode vectorNode in xmlDoc.DocumentElement.ChildNodes)
                {
                    var coordinates = vectorNode.ChildNodes;
                    var newSphere = CoordinatesToSphere(coordinates[0].InnerText, coordinates[1].InnerText, coordinates[2].InnerText);
                    Spheres.Add(newSphere);
                }
            }
            else
            {
                char[] splitChar = new char[] { ' ' };
                var lines = ReadLines(() => new FileStream(filename, FileMode.Open), Encoding.UTF8).ToList();
                foreach (var line in lines)
                {
                    string[] coordinates = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
                    var newSphere = CoordinatesToSphere(coordinates[0], coordinates[1], coordinates[2]);
                    Spheres.Add(newSphere);
                }
            }
		}

        private IEnumerable<string> ReadLines(Func<System.IO.Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new System.IO.StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
