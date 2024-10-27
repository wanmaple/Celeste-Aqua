using System;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class Maths
    {
        public const float EPSILON = 0.001f;

        public static int NextPowerOfTwo(int num)
        {
            --num;
            num |= num >> 1;
            num |= num >> 2;
            num |= num >> 4;
            num |= num >> 8;
            num |= num >> 16;
            ++num;
            return num;
        }

        public static bool ApproximateEqual(float num, float compare)
        {
            return MathF.Abs(num - compare) < EPSILON;
        }
    }
}
