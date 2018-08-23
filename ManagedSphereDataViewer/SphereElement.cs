using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
	public class SphereElement
	{
		public readonly float X;
		public readonly float Y;
		public readonly float Z;
		public readonly float R;
		public readonly uint Color;

        public double screenZ;

		public SphereElement(float x, float y, float z, float r, double screenZ, uint color)
		{
			X = x;
			Y = y;
			Z = z;
			R = r;
            this.screenZ = screenZ;
			Color = color;
		}
	}
}
