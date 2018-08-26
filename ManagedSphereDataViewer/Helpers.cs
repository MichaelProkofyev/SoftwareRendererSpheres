using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedSphereDataViewer
{
    static class Helpers
    {
        public const float twoPi = 6.283185f;

        private static Random rand = new Random();

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

        static float RandomFloat()
        {
            double mantissa = (rand.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, rand.Next(-126, 128));
            return (float)(mantissa * exponent);
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
    }
}
