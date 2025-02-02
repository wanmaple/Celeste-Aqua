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

        public static Vector2 Abs(Vector2 vec)
        {
            return new Vector2(MathF.Abs(vec.X), MathF.Abs(vec.Y));
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

        public static Vector2 TurnToDirection8(Vector2 direction)
        {
            float half45 = MathF.PI / 8.0f;
            float cosHalf45 = MathF.Cos(half45);
            float dotX = Vector2.Dot(direction, Vector2.UnitX);
            if (MathF.Abs(dotX) >= cosHalf45)
            {
                return dotX > 0.0f ? Vector2.UnitX : -Vector2.UnitX;
            }
            float dotY = Vector2.Dot(direction, Vector2.UnitY);
            if (MathF.Abs(dotY) >= cosHalf45)
            {
                return dotY > 0.0f ? Vector2.UnitY : -Vector2.UnitY;
            }
            Vector2 ret;
            if (direction.X > 0.0f && direction.Y > 0.0f)
            {
                ret = new Vector2(1.0f, 1.0f);
            }
            else if (direction.X > 0.0f && direction.Y < 0.0f)
            {
                ret = new Vector2(1.0f, -1.0f);
            }
            else if (direction.X < 0.0f && direction.Y > 0.0f)
            {
                ret = new Vector2(-1.0f, 1.0f);
            }
            else
            {
                ret = new Vector2(-1.0f, -1.0f);
            }
            ret.Normalize();
            return ret;
        }

        public static float TriangleArea(Vector2 pt1, Vector2 pt2, Vector2 pt3)
        {
            return 0.5f * MathF.Abs(pt1.X * (pt2.Y - pt3.Y) + pt2.X * (pt3.Y - pt1.Y) + pt3.X * (pt1.Y - pt2.Y));
        }

        public static bool IsPointOnSegment(Vector2 pt, Vector2 seg1, Vector2 seg2)
        {
            if (seg1 == seg2) return false;
            float minX = MathF.Min(seg1.X, seg2.X);
            float maxX = MathF.Max(seg1.X, seg2.X);
            return Cross(pt - seg1, seg2 - seg1) == 0.0f && pt.X >= minX && pt.X <= maxX;
        }

        public static bool IsPointInsideTriangle(Vector2 pt, Vector2 tri1, Vector2 tri2, Vector2 tri3, bool includeEdge = false)
        {
            if (IsPointOnSegment(pt, tri1, tri2) || IsPointOnSegment(pt, tri2, tri3) || IsPointOnSegment(pt, tri3, tri1))
                return includeEdge;
            float d1 = Sign(pt, tri1, tri2);
            float d2 = Sign(pt, tri2, tri3);
            float d3 = Sign(pt, tri3, tri1);
            bool hasNeg = d1 < 0.0f || d2 < 0.0f || d3 < 0.0f;
            bool hasPos = d1 > 0.0f || d2 > 0.0f || d3 > 0.0f;
            return !(hasNeg && hasPos);
        }

        public static bool IsPointInsidePolygon(Vector2 pt, bool includeEdge, params Vector2[] points)
        {
            unsafe
            {
                fixed (Vector2* ptr = points)
                {
                    return IsPointInsidePolygon(pt, ptr, points.Length, includeEdge);
                }
            }
        }

        public static unsafe bool IsPointInsidePolygon(Vector2 pt, Vector2* ptr, int num, bool includeEdge)
        {
            // Odd-Even rule
            int intersects = 0;
            for (int i = 0; i < num; i++)
            {
                Vector2 p1 = ptr[i];
                Vector2 p2 = ptr[(i + 1) % num];
                if (IsPointOnSegment(pt, p1, p2))
                    return includeEdge;
                if (CheckHorizontalLineIntersectsWithSegment(pt, p1, p2, 1))
                    ++intersects;
            }
            return intersects % 2 == 1;
        }

        public static bool IsSegmentIntersectsSegment(Segment seg1, Segment seg2)
        {
            return IsSegmentIntersectsSegment(seg1.Point1, seg1.Point2, seg2.Point1, seg2.Point2);
        }

        public static bool IsSegmentIntersectsSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return CCW(a, c, d) != CCW(b, c, d) && CCW(a, b, c) != CCW(a, b, d);
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

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool CheckHorizontalLineIntersectsWithSegment(Vector2 pt, Vector2 pt1, Vector2 pt2, int sign)
        {
            if (pt1.Y == pt2.Y)
            {
                return false;
            }
            Vector2 lowerPt = pt1.Y < pt2.Y ? pt1 : pt2;
            Vector2 higherPt = lowerPt == pt1 ? pt2 : pt1;
            int side = MathF.Sign(Cross(pt - lowerPt, higherPt - lowerPt));
            if (side == sign)
            {
                return pt.Y >= lowerPt.Y && pt.Y < higherPt.Y;
            }
            return false;
        }

        private static bool CCW(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
        }
    }
}
