using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
	public class SphereElement
	{
		public readonly float x;
		public readonly float y;
		public readonly float z;
		public readonly float r;
        public float screenZ;

		public readonly Vector3Byte colorA;
        public readonly Vector3Byte colorB;

		public SphereElement(float x, float y, float z, float r, float screenZ)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.r = r;
            this.screenZ = screenZ;

            colorA = new Vector3Byte(Helpers.RandomByte(), Helpers.RandomByte(), Helpers.RandomByte());
            colorB = new Vector3Byte(Helpers.RandomByte(), Helpers.RandomByte(), Helpers.RandomByte());
        }
	}
}
