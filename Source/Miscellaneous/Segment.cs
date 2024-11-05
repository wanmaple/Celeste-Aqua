using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public struct Segment
    {
        public Vector2 Point1 { get; set; }
        public Vector2 Point2 { get; set; }

        public Vector2 Vector => Point2 - Point1;
        public Vector2 Direction => Vector2.Normalize(Vector);
        public float Length => Vector.Length();
        public bool InFirstQuadrant => Vector.X > 0.0f && Vector.Y >= 0.0f;
        public bool InSecondQuadrant => Vector.X <= 0.0f && Vector.Y > 0.0f;
        public bool InThirdQuadrant => Vector.X < 0.0f && Vector.Y <= 0.0f;
        public bool InFourthQuadrant => Vector.X >= 0.0f && Vector.Y < 0.0f;

        public Segment(Vector2 pt1, Vector2 pt2)
        {
            Point1= pt1;
            Point2= pt2;
        }

        public override string ToString()
        {
            return $"{Point1} -> {Point2}";
        }
    }
}
