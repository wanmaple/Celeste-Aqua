using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.Aqua.Core
{
    public static class HitboxExtensions
    {
        public static IReadOnlyList<Edge> Edges(this Hitbox self)
        {
            List<Edge> edges = DynamicData.For(self).Get<List<Edge>>("edges");
            if (edges == null)
            {
                edges = new List<Edge>(4);
                Vector2 tl = new Vector2(self.AbsoluteLeft, self.AbsoluteTop);
                Vector2 tr = new Vector2(self.AbsoluteRight, self.AbsoluteTop);
                Vector2 bl = new Vector2(self.AbsoluteLeft, self.AbsoluteBottom);
                Vector2 br = new Vector2(self.AbsoluteRight, self.AbsoluteBottom);
                edges.Add(new Edge(tl, tr, -Vector2.UnitY));
                edges.Add(new Edge(tl, bl, -Vector2.UnitX));
                edges.Add(new Edge(tr, br, Vector2.UnitX));
                edges.Add(new Edge(bl, br, Vector2.UnitY));
                DynamicData.For(self).Set("edges", edges);
            }
            return edges;
        }

        public static bool CheckLineNotOnEdge(this Hitbox self, Vector2 pt1, Vector2 pt2)
        {
            double x0 = pt1.X;
            double y0 = pt1.Y;
            double x1 = pt2.X;
            double y1 = pt2.Y;
            // Check if either endpoint is inside the rectangle
            bool isInside(double x, double y) => x > self.AbsoluteLeft && x < self.AbsoluteRight && y > self.AbsoluteTop && y < self.AbsoluteBottom;
            if (isInside(x0, y0) || isInside(x1, y1))
            {
                return true;
            }

            // Check if the line intersects any of the rectangle's sides
            bool lineIntersectsLine(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3)
            {
                double denominator = (y3 - y2) * (x1 - x0) - (x3 - x2) * (y1 - y0);
                if (denominator == 0)
                {
                    return false;
                }

                double ua = ((x3 - x2) * (y0 - y2) - (y3 - y2) * (x0 - x2)) / denominator;
                double ub = ((x1 - x0) * (y0 - y2) - (y1 - y0) * (x0 - x2)) / denominator;

                // Check if the intersection point is exactly on the edge
                if ((ua == 0 || ua == 1) && (ub == 0 || ub == 1))
                {
                    return false;
                }

                return ua > 0 && ua < 1 && ub > 0 && ub < 1;
            }
            bool intersects = lineIntersectsLine(x0, y0, x1, y1, self.AbsoluteLeft, self.AbsoluteTop, self.AbsoluteRight, self.AbsoluteTop) ||
            lineIntersectsLine(x0, y0, x1, y1, self.AbsoluteLeft, self.AbsoluteTop, self.AbsoluteLeft, self.AbsoluteBottom) ||
            lineIntersectsLine(x0, y0, x1, y1, self.AbsoluteRight, self.AbsoluteTop, self.AbsoluteRight, self.AbsoluteBottom) ||
            lineIntersectsLine(x0, y0, x1, y1, self.AbsoluteLeft, self.AbsoluteBottom, self.AbsoluteRight, self.AbsoluteBottom);

            return intersects;
        }
    }
}
