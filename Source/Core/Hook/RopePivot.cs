using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Celeste.Mod.Aqua.Core
{
    public enum Cornors
    {
        Free,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public struct RopePivot : IEquatable<RopePivot>
    {
        public Vector2 point;
        public Cornors direction;
        public Entity entity;

        private static string[] DIRECTION_STRINGS = { "F", "TL", "TR", "BL", "BR", };

        public RopePivot(Vector2 pt, Cornors cn, Entity e = null)
        {
            point = pt;
            direction = cn;
            entity = e;
        }

        public Vector2 OffsetPoint(float offset)
        {
            Vector2 off = Vector2.Zero;
            switch (direction)
            {
                case Cornors.TopLeft:
                    off = new Vector2(-1.0f, -1.0f);
                    break;
                case Cornors.TopRight:
                    off = new Vector2(1.0f, -1.0f);
                    break;
                case Cornors.BottomLeft:
                    off = new Vector2(-1.0f, 1.0f);
                    break;
                case Cornors.BottomRight:
                    off = new Vector2(1.0f, 1.0f);
                    break;
            }
            return point + off * offset;
        }

        public override string ToString()
        {
            return $"{DIRECTION_STRINGS[(int)direction]}{point}";
        }

        public bool Equals(RopePivot other)
        {
            return point == other.point && direction == other.direction && entity == other.entity;
        }

        public override bool Equals(object obj)
        {
            return obj is RopePivot && Equals((RopePivot)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = point.GetHashCode();
                hashCode = (hashCode * 397) ^ direction.GetHashCode();
                if (entity != null)
                {
                    hashCode = (hashCode * 397) ^ entity.GetHashCode();
                }
                return hashCode;
            }
        }

        public static bool operator ==(RopePivot left, RopePivot right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RopePivot left, RopePivot right)
        {
            return !left.Equals(right);
        }
    }
}
