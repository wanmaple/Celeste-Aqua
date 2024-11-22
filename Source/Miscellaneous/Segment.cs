using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public struct Segment : IEquatable<Segment>
    {
        public Vector2 Point1 { get; set; }
        public Vector2 Point2 { get; set; }

        public Vector2 Vector => Point2 - Point1;
        public Vector2 Direction => Vector2.Normalize(Vector);
        public float Length => Vector.Length();

        public Segment(Vector2 pt1, Vector2 pt2)
        {
            Point1= pt1;
            Point2= pt2;
        }

        public static Segment operator+(Segment seg1, Segment seg2)
        {
            return new Segment(seg1.Point1 + seg2.Point1, seg1.Point2 + seg2.Point2);
        }

        public static Segment operator-(Segment seg1, Segment seg2)
        {
            return new Segment(seg1.Point1 - seg2.Point1, seg1.Point2 - seg2.Point2);
        }

        public static bool operator==(Segment seg1, Segment seg2)
        {
            return (seg1.Point1 == seg2.Point1 && seg1.Point2 == seg2.Point2) || (seg1.Point1 == seg2.Point2 && seg1.Point2 == seg2.Point1);
        }

        public static bool operator!=(Segment seg1, Segment seg2)
        {
            return !(seg1 == seg2);
        }

        public bool Equals(Segment other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Segment && Equals((Segment)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Point1.GetHashCode();
                hashCode = (hashCode * 397) ^ Point2.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Point1} -> {Point2}";
        }
    }
}
