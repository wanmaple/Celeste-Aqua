using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public struct Edge : IEquatable<Edge>
    {
        public Segment Segment { get; set; }
        public Vector2 Normal { get; set; }

        public Edge(Segment seg, Vector2 normal)
        {
            Segment = seg;
            Normal = normal;
        }

        public Edge(Vector2 pt1, Vector2 pt2, Vector2 normal)
            : this(new Segment(pt1, pt2), normal)
        {
        }

        public static bool operator==(Edge lhs, Edge rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(Edge lhs, Edge rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(Edge other)
        {
            return Segment == other.Segment && Normal == other.Normal;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge && Equals((Edge)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Segment.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                return hashCode;
            }
        }
    }
}
