﻿using Microsoft.Xna.Framework;
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

        public static bool IsApproximateEqual(float num, float compare)
        {
            return MathF.Abs(num - compare) < EPSILON;
        }

        public static bool IsApproximateEqual(Vector2 v1, Vector2 v2)
        {
            return IsApproximateZero(v1 - v2);
        }

        public static bool IsApproximateZero(float num)
        {
            return IsApproximateEqual(num, 0.0f);
        }

        public static bool IsApproximateZero(Vector2 vec)
        {
            return IsApproximateZero(vec.X) && IsApproximateZero(vec.Y);
        }

        public static float Fract(float num)
        {
            return num - (int)num;
        }

        public static Vector2 Fract(Vector2 vec)
        {
            return new Vector2(Fract(vec.X), Fract(vec.Y));
        }

        public static Vector2 Round(Vector2 vec)
        {
            return new Vector2(MathF.Round(vec.X), MathF.Round(vec.Y));
        }

        public static bool IsPointInsideTriangle(Vector2 pt, Vector2 tri1, Vector2 tri2, Vector2 tri3)
        {
            float d1 = Sign(pt, tri1, tri2);
            float d2 = Sign(pt, tri2, tri3);
            float d3 = Sign(pt, tri3, tri1);
            bool hasNeg = d1 < 0.0f || d2 < 0.0f || d3 < 0.0f;
            bool hasPos = d1 > 0.0f || d2 > 0.0f || d3 > 0.0f;
            return !(hasNeg && hasPos);
        }

        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        public static Vector2 Lerp(Vector2 v1, Vector2 v2, float amount)
        {
            return new Vector2(MathHelper.Lerp(v1.X, v2.X, amount), MathHelper.Lerp(v1.Y, v2.Y, amount));
        }

        public static Vector2 Reflect(Vector2 inDirection, Vector2 axis)
        {
            float proj = Vector2.Dot(inDirection, axis);
            Vector2 doubleAxis = axis * MathF.Abs(proj) * 2.0f;
            return inDirection + doubleAxis;
        }

        public static bool IsVectorInsideTwoVectors(Vector2 v, Vector2 v1, Vector2 v2, bool includeBoundary = true)
        {
            if (IsApproximateZero(v)) return false;
            float cross1 = Cross(v1, v);
            float cross2 = Cross(v, v2);
            float cross3 = Cross(v1, v2);
            bool ret = MathF.Sign(cross1) == MathF.Sign(cross2) && MathF.Sign(cross1) == MathF.Sign(cross3);
            if (includeBoundary)
            {
                ret = ret || Vector2.Dot(v1, v) == 1.0f || Vector2.Dot(v2, v) == 1.0f;
            }
            return ret;
        }

        public static bool IsLineInsideTwoVectors(Vector2 v, Vector2 v1, Vector2 v2, bool includeBoundary = true)
        {
            if (IsApproximateZero(v)) return false;
            float cross1 = Cross(v1, v);
            float cross2 = Cross(v, v2);
            bool ret = MathF.Sign(cross1) == MathF.Sign(cross2);
            if (includeBoundary)
            {
                ret = ret || cross1 == 0.0f || cross2 == 0.0f;
            }
            return ret;
        }


        public static Matrix TRS(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix.CreateTranslation(position) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateScale(scale);
        }

        // 没有2x2矩阵可以用，用4x4的凑合一下，虽然运算量会上来
        public static Matrix BuildMatrix(Vector2 v1, Vector2 v2)
        {
            return new Matrix(
                v1.X, v2.X, 0.0f, 0.0f,
                v1.Y, v2.Y, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
        }

        // XNA真的是太离谱了，没有2x2矩阵就算了，连4x4矩阵乘以向量的功能都没有，真TM服了
        public static Vector4 Matrix4Multiply(ref Matrix matrix, Vector4 v)
        {
            Vector4 row1 = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
            Vector4 row2 = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
            Vector4 row3 = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
            Vector4 row4 = new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
            float x = Vector4.Dot(row1, v);
            float y = Vector4.Dot(row2, v);
            float z = Vector4.Dot(row3, v);
            float w = Vector4.Dot(row4, v);
            return new Vector4(x, y, z, w);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }
    }
}
