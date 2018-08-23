using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
	public struct Vector3
	{
		public float x;
		public float y;
		public float z;

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public void Normalize()
		{
			float d = (float)Math.Sqrt(x * x + y * y + z * z);

			x = x / d;
			y = y / d;
			z = z / d;
		}

		public Vector3 GetNormalized()
		{
			Vector3 value = new Vector3(x, y, z);
			value.Normalize();
			return value;
		}

		public float Dot(Vector3 v2)
		{
			return x * v2.x + y * v2.y + z * v2.z;
		}
	}
}
