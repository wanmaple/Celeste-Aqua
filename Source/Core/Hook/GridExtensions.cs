using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

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

        public static bool HasUp(this Grid grid, Point point)
        {
            return point.Y > 0 && grid[point.X, point.Y - 1];
        }

        public static bool HasDown(this Grid grid, Point point)
        {
            return point.Y < grid.CellsY - 1 && grid[point.X, point.Y + 1];
        }

        public static bool HasLeft(this Grid grid, Point point)
        {
            return point.X > 0 && grid[point.X - 1, point.Y];
        }

        public static bool HasRight(this Grid grid, Point point)
        {
            return point.X < grid.CellsX - 1 && grid[point.X + 1, point.Y];
        }

        public static List<RopePivot> FindConvexPoints(this Grid grid)
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
                            if ((bit & BIT_TL) > 0) points.Add(new RopePivot(new Vector2(grid.AbsoluteLeft + current.X * grid.CellWidth, grid.AbsoluteTop + current.Y * grid.CellHeight), Cornors.TopLeft));
                            if ((bit & BIT_TR) > 0) points.Add(new RopePivot(new Vector2(grid.AbsoluteLeft + (current.X + 1) * grid.CellWidth, grid.AbsoluteTop + current.Y * grid.CellHeight), Cornors.TopRight));
                            if ((bit & BIT_BL) > 0) points.Add(new RopePivot(new Vector2(grid.AbsoluteLeft + current.X * grid.CellWidth, grid.AbsoluteTop + (current.Y + 1) * grid.CellHeight), Cornors.BottomLeft));
                            if ((bit & BIT_BR) > 0) points.Add(new RopePivot(new Vector2(grid.AbsoluteLeft + (current.X + 1) * grid.CellWidth, grid.AbsoluteTop + (current.Y + 1) * grid.CellHeight), Cornors.BottomRight));
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
    }
}
