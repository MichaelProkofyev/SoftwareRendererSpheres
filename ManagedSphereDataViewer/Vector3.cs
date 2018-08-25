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

    public struct Vector3Byte
    {
        public byte x;
        public byte y;
        public byte z;

        public Vector3Byte(byte x, byte y, byte z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Normalize()
        {
            byte d = (byte)Math.Sqrt(x * x + y * y + z * z);

            x = (byte)(x / d);
            y = (byte)(y / d);
            z = (byte)(z / d);
        }


        public byte Dot(Vector3Byte v2)
        {
            return (byte)(x * v2.x + y * v2.y + z * v2.z);
        }

        public static Vector3Byte Lerp(Vector3Byte a, Vector3Byte b, float t)
        {
            t = Helpers.Clamp(t, 0f, 1f);
            byte xResult = (byte)(a.x * t + b.x * (1 - t));
            byte yResult = (byte)(a.y * t + b.y * (1 - t));
            byte zResult = (byte)(a.z * t + b.z * (1 - t));
            return new Vector3Byte(xResult, yResult, zResult);
        }
    }
}
