using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public static class GridExtensions
    {
        private const int BIT_TL = 0x1;
        private const int BIT_TR = 0x2;
        private const int BIT_BL = 0x4;
        private const int BIT_BR = 0x8;
        private const int BIT_T = BIT_TL | BIT_TR;
        private const int BIT_B = BIT_BL | BIT_BR;
        private const int BIT_L = BIT_TL | BIT_BL;
        private const int BIT_R = BIT_TR | BIT_BR;
        private const int BIT_ALL = BIT_TL | BIT_TR | BIT_BL | BIT_BR;

        private static readonly Vector2 ANCHOR_TL = Vector2.Zero;
        private static readonly Vector2 ANCHOR_TR = Vector2.UnitX;
        private static readonly Vector2 ANCHOR_BL = Vector2.UnitY;
        private static readonly Vector2 ANCHOR_BR = Vector2.One;

        public static IReadOnlyList<RopePivot> ConvexPoints(this Grid self)
        {
            List<RopePivot> pivots = DynamicData.For(self).Get<List<RopePivot>>("convex_points");
            if (pivots == null)
            {
                pivots = self.FindConvexPoints();
                DynamicData.For(self).Set("convex_points", pivots);
            }
            return pivots;
        }

        public static IReadOnlyList<Edge> Edges(this Grid self)
        {
            List<Edge> edges = DynamicData.For(self).Get<List<Edge>>("edges");
            if (edges == null)
            {
                edges = self.FindEdges();
                DynamicData.For(self).Set("edges", edges);
            }
            return edges;
        }

        private static bool HasUp(this Grid grid, Point point)
        {
            return point.Y > 0 && grid[point.X, point.Y - 1];
        }

        private static bool HasDown(this Grid grid, Point point)
        {
            return point.Y < grid.CellsY - 1 && grid[point.X, point.Y + 1];
        }

        private static bool HasLeft(this Grid grid, Point point)
        {
            return point.X > 0 && grid[point.X - 1, point.Y];
        }

        private static bool HasRight(this Grid grid, Point point)
        {
            return point.X < grid.CellsX - 1 && grid[point.X + 1, point.Y];
        }

        private static List<RopePivot> FindConvexPoints(this Grid grid)
        {
            List<RopePivot> points = new List<RopePivot>(32);
            bool[,] closeFlags = new bool[grid.CellsX, grid.CellsY];
            Stack<Point> openlist = new Stack<Point>(32);
            HashSet<Point> closelist = new HashSet<Point>();
            for (int x = 0; x < grid.CellsX; ++x)
            {
                for (int y = 0; y < grid.CellsY; ++y)
                {
                    if (closeFlags[x, y]) continue;
                    if (grid[x, y])
                    {
                        openlist.Clear();
                        closelist.Clear();
                        Point location = new Point(x, y);
                        openlist.Push(location);
                        while (openlist.Count > 0)
                        {
                            Point current = openlist.Pop();
                            closelist.Add(current);
                            if (closeFlags[current.X, current.Y]) continue;
                            closeFlags[current.X, current.Y] = true;
                            closelist.Add(current);
                            bool hasUp = grid.HasUp(current);
                            bool hasDown = grid.HasDown(current);
                            bool hasLeft = grid.HasLeft(current);
                            bool hasRight = grid.HasRight(current);
                            int bit = BIT_ALL;       // use 4 bits, from low to high: TL, TR, BL, BR,
                            if (hasUp) bit &= BIT_B;
                            if (hasDown) bit &= BIT_T;
                            if (hasLeft) bit &= BIT_R;
                            if (hasRight) bit &= BIT_L;
                            if ((bit & BIT_TL) > 0)
                                points.Add(new RopePivot(grid.CalculateAbsolutePoint(current, ANCHOR_TL), Cornors.TopLeft));
                            if ((bit & BIT_TR) > 0)
                                points.Add(new RopePivot(grid.CalculateAbsolutePoint(current, ANCHOR_TR), Cornors.TopRight));
                            if ((bit & BIT_BL) > 0)
                                points.Add(new RopePivot(grid.CalculateAbsolutePoint(current, ANCHOR_BL), Cornors.BottomLeft));
                            if ((bit & BIT_BR) > 0)
                                points.Add(new RopePivot(grid.CalculateAbsolutePoint(current, ANCHOR_BR), Cornors.BottomRight));
                            if (hasUp)
                            {
                                Point up = new Point(current.X, current.Y - 1);
                                if (!closelist.Contains(up))
                                {
                                    openlist.Push(up);
                                }
                            }
                            if (hasDown)
                            {
                                Point down = new Point(current.X, current.Y + 1);
                                if (!closelist.Contains(down))
                                {
                                    openlist.Push(down);
                                }
                            }
                            if (hasLeft)
                            {
                                Point left = new Point(current.X - 1, current.Y);
                                if (!closelist.Contains(left))
                                {
                                    openlist.Push(left);
                                }
                            }
                            if (hasRight)
                            {
                                Point right = new Point(current.X + 1, current.Y);
                                if (!closelist.Contains(right))
                                {
                                    openlist.Push(right);
                                }
                            }
                        }
                    }
                }
            }
            return points;
        }

        private static List<Edge> FindEdges(this Grid grid)
        {
            HashSet<Edge> edges = new HashSet<Edge>(32);
            bool[,] closeFlags = new bool[grid.CellsX, grid.CellsY];
            bool hasLeft = false, hasRight = false, hasUp = false, hasDown = false;
            Point? pt = null;
            while ((pt = grid.FindFirstEdgeLocation(closeFlags, ref hasLeft, ref hasRight, ref hasUp, ref hasDown)) != null)
            {
                Point start = pt.Value;
                if (!hasLeft && !hasUp)
                {
                    {
                        // Find along down direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(0, 1), grid.HasDown, grid.HasLeft);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_TL), grid.CalculateAbsolutePoint(end, ANCHOR_BL));
                        Edge edge = new Edge(seg, -Vector2.UnitX);
                        edges.Add(edge);
                    }
                    {
                        // Find along right direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(1, 0), grid.HasRight, grid.HasUp);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_TL), grid.CalculateAbsolutePoint(end, ANCHOR_TR));
                        Edge edge = new Edge(seg, -Vector2.UnitY);
                        edges.Add(edge);
                    }
                }
                if (!hasLeft && !hasDown)
                {
                    {
                        // Find along up direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(0, -1), grid.HasUp, grid.HasLeft);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_BL), grid.CalculateAbsolutePoint(end, ANCHOR_TL));
                        Edge edge = new Edge(seg, -Vector2.UnitX);
                        edges.Add(edge);
                    }
                    {
                        // Find along right direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(1, 0), grid.HasRight, grid.HasDown);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_BL), grid.CalculateAbsolutePoint(end, ANCHOR_BR));
                        Edge edge = new Edge(seg, Vector2.UnitY);
                        edges.Add(edge);
                    }
                }
                if (!hasRight && !hasUp)
                {
                    {
                        // Find along down direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(0, 1), grid.HasDown, grid.HasRight);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_TR), grid.CalculateAbsolutePoint(end, ANCHOR_BR));
                        Edge edge = new Edge(seg, Vector2.UnitX);
                        edges.Add(edge);
                    }
                    {
                        // Find along left direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(-1, 0), grid.HasLeft, grid.HasUp);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_TR), grid.CalculateAbsolutePoint(end, ANCHOR_TL));
                        Edge edge = new Edge(seg, Vector2.UnitY);
                        edges.Add(edge);
                    }
                }
                if (!hasRight && !hasDown)
                {
                    {
                        // Find along up direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(0, -1), grid.HasUp, grid.HasRight);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_BR), grid.CalculateAbsolutePoint(end, ANCHOR_TR));
                        Edge edge = new Edge(seg, Vector2.UnitX);
                        edges.Add(edge);
                    }
                    {
                        // Find along left direction
                        Point end = grid.FindEdgeEndingAlong(start, new Point(-1, 0), grid.HasLeft, grid.HasDown);
                        Segment seg = new Segment(grid.CalculateAbsolutePoint(start, ANCHOR_BR), grid.CalculateAbsolutePoint(end, ANCHOR_BL));
                        Edge edge = new Edge(seg, -Vector2.UnitY);
                        edges.Add(edge);
                    }
                }
            }
            return edges.ToList();
        }

        private static Point? FindFirstEdgeLocation(this Grid grid, bool[,] closeFlags, ref bool hasLeft, ref bool hasRight, ref bool hasUp, ref bool hasDown)
        {
            for (int i = 0; i < grid.CellsX; i++)
            {
                for (int j = 0; j < grid.CellsY; j++)
                {
                    if (grid[i, j])
                    {
                        if (closeFlags[i, j])
                            continue;
                        closeFlags[i, j] = true;
                        Point pt = new Point(i, j);
                        hasLeft = grid.HasLeft(pt);
                        hasRight = grid.HasRight(pt);
                        hasUp = grid.HasUp(pt);
                        hasDown = grid.HasDown(pt);
                        int sum = Convert.ToInt32(hasLeft) + Convert.ToInt32(hasRight) + Convert.ToInt32(hasUp) + Convert.ToInt32(hasDown);
                        if (sum >= 3)
                        {
                            continue;
                        }
                        return pt;
                    }
                }
            }
            return null;
        }

        private static Point FindEdgeEndingAlong(this Grid grid, Point start, Point step, Func<Point, bool> seeker, Func<Point, bool> predicate)
        {
            Point current = start;
            do
            {
                if (!seeker.Invoke(current))
                    return current;
                Point neighbor = current + step;
                if (predicate.Invoke(neighbor))
                    return current;
                current = neighbor;
            } while (true);
        }

        private static Vector2 CalculateAbsolutePoint(this Grid grid, Point pt, Vector2 anchor)
        {
            Vector2 origin = new Vector2(grid.AbsoluteLeft, grid.AbsoluteTop);
            Vector2 size = new Vector2(grid.CellWidth, grid.CellHeight);
            return origin + (new Vector2(pt.X, pt.Y) + anchor) * size;
        }
    }
}
