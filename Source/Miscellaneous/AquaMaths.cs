using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class AquaMaths
    {
        private const float EPSILON = 0.001f;

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

        public static bool IsApproximateZero(float num)
        {
            return ApproximateEqual(num, 0.0f);
        }

        public static bool IsApproximateZero(Vector2 vec)
        {
            return IsApproximateZero(vec.X) && IsApproximateZero(vec.Y);
        }

        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        public static bool IsVectorInsideTwoVectors(Vector2 v, Vector2 v1, Vector2 v2)
        {
            if (IsApproximateZero(v)) return false;
            float cross1 = Cross(v1, v);
            float cross2 = Cross(v, v2);
            return IsApproximateZero(cross1) || IsApproximateZero(cross2) || (MathF.Sign(cross1) == MathF.Sign(cross2));
        }
    }
}
