using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
    static class Helpers
    {
        public const float twoPi = 6.2831f;

        private static Random rand = new Random();

        public static float PowOfNine(float x)
        {
            return x * x * x * x * x * x * x * x * x;
        }

        public static uint Lerp(uint a, uint b, float progress)
        {
            return (uint)(a * progress + b * (1 - progress));
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static byte RandomByte()
        {
            return (byte)rand.Next(byte.MinValue, byte.MaxValue);
        }

        //Ping-pongs the value between 0 & length
        public static float PingPong(float value, float maxValue)
        {
            float l = 2f * maxValue;
            float t = value % l;

            if (0 <= t && t < maxValue)
            {
                return t;
            }
            else
            {
                return l - t;
            }
        }

        public static IEnumerable<string> ReadLines(Func<System.IO.Stream> streamProvider,
                                     Encoding encoding)
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
