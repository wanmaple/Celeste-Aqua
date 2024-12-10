using Monocle;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class BarrierRenderer : Entity
    {
        public class Edge
        {
            public IBarrierRenderable Parent;
            public bool Visible;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;
            public Color Color;

            public Edge(IBarrierRenderable parent, Vector2 a, Vector2 b)
            {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                Normal = (b - a).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (a - b).Length();
                Color = parent.Color;
            }

            public void UpdateWave(float time)
            {
                if (Wave == null || (float)Wave.Length <= Length)
                {
                    Wave = new float[(int)Length + 2];
                }

                for (int i = 0; (float)i <= Length; i++)
                {
                    Wave[i] = GetWaveAt(time, i, Length);
                }
            }

            public float GetWaveAt(float offset, float along, float length)
            {
                if (along <= 1f || along >= length - 1f)
                {
                    return 0f;
                }

                if (Parent.Solidify >= 1f)
                {
                    return 0f;
                }

                float num = offset + along * 0.25f;
                float num2 = (float)(Math.Sin(num) * 2.0 + Math.Sin(num * 0.25f));
                return (1f + num2 * Ease.SineInOut(Calc.YoYo(along / length))) * (1f - Parent.Solidify);
            }

            public bool InView(ref Rectangle view)
            {
                if ((float)view.Left < Parent.X + Max.X && (float)view.Right > Parent.X + Min.X && (float)view.Top < Parent.Y + Max.Y)
                {
                    return (float)view.Bottom > Parent.Y + Min.Y;
                }

                return false;
            }
        }

        public BarrierRenderer()
        {
            base.Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
            base.Depth = 0;
            Add(new CustomBloom(OnRenderBloom));
        }

        public void Track(IBarrierRenderable block)
        {
            _list.Add(block);
            if (_tiles == null)
            {
                _levelTileBounds = (base.Scene as Level).TileBounds;
                _tiles = new VirtualMap<bool>(_levelTileBounds.Width, _levelTileBounds.Height, emptyValue: false);
            }

            for (int i = (int)block.X / 8; (float)i < block.Right / 8f; i++)
            {
                for (int j = (int)block.Y / 8; (float)j < block.Bottom / 8f; j++)
                {
                    _tiles[i - _levelTileBounds.X, j - _levelTileBounds.Y] = true;
                }
            }

            _dirty = true;
        }

        public void Untrack(IBarrierRenderable block)
        {
            _list.Remove(block);
            if (_list.Count <= 0)
            {
                _tiles = null;
            }
            else
            {
                for (int i = (int)block.X / 8; (float)i < block.Right / 8f; i++)
                {
                    for (int j = (int)block.Y / 8; (float)j < block.Bottom / 8f; j++)
                    {
                        _tiles[i - _levelTileBounds.X, j - _levelTileBounds.Y] = false;
                    }
                }
            }

            _dirty = true;
        }

        public override void Update()
        {
            if (_dirty)
            {
                RebuildEdges();
            }

            UpdateEdges();
        }

        private void UpdateEdges()
        {
            Camera camera = (base.Scene as Level).Camera;
            Rectangle view = new Rectangle((int)camera.Left - 4, (int)camera.Top - 4, (int)(camera.Right - camera.Left) + 8, (int)(camera.Bottom - camera.Top) + 8);
            for (int i = 0; i < _edges.Count; i++)
            {
                if (_edges[i].Visible)
                {
                    if (base.Scene.OnInterval(0.25f, (float)i * 0.01f) && !_edges[i].InView(ref view))
                    {
                        _edges[i].Visible = false;
                    }
                }
                else if (base.Scene.OnInterval(0.05f, (float)i * 0.01f) && _edges[i].InView(ref view))
                {
                    _edges[i].Visible = true;
                }

                if (_edges[i].Visible && (base.Scene.OnInterval(0.05f, (float)i * 0.01f) || _edges[i].Wave == null))
                {
                    _edges[i].UpdateWave(base.Scene.TimeActive * 3f);
                }
            }
        }

        private void RebuildEdges()
        {
            _dirty = false;
            _edges.Clear();
            if (_list.Count <= 0)
            {
                return;
            }

            Level obj = base.Scene as Level;
            _ = obj.TileBounds.Left;
            _ = obj.TileBounds.Top;
            _ = obj.TileBounds.Right;
            _ = obj.TileBounds.Bottom;
            Point[] array = new Point[4]
            {
                new Point(0, -1),
                new Point(0, 1),
                new Point(-1, 0),
                new Point(1, 0)
            };
            foreach (IBarrierRenderable item in _list)
            {
                for (int i = (int)item.X / 8; (float)i < item.Right / 8f; i++)
                {
                    for (int j = (int)item.Y / 8; (float)j < item.Bottom / 8f; j++)
                    {
                        Point[] array2 = array;
                        for (int k = 0; k < array2.Length; k++)
                        {
                            Point point = array2[k];
                            Point point2 = new Point(-point.Y, point.X);
                            if (!Inside(i + point.X, j + point.Y) && (!Inside(i - point2.X, j - point2.Y) || Inside(i + point.X - point2.X, j + point.Y - point2.Y)))
                            {
                                Point point3 = new Point(i, j);
                                Point point4 = new Point(i + point2.X, j + point2.Y);
                                Vector2 vector = new Vector2(4f) + new Vector2(point.X - point2.X, point.Y - point2.Y) * 4f;
                                while (Inside(point4.X, point4.Y) && !Inside(point4.X + point.X, point4.Y + point.Y))
                                {
                                    point4.X += point2.X;
                                    point4.Y += point2.Y;
                                }

                                Vector2 a = new Vector2(point3.X, point3.Y) * 8f + vector - item.Position;
                                Vector2 b = new Vector2(point4.X, point4.Y) * 8f + vector - item.Position;
                                _edges.Add(new Edge(item, a, b));
                            }
                        }
                    }
                }
            }
        }

        private bool Inside(int tx, int ty)
        {
            return _tiles[tx - _levelTileBounds.X, ty - _levelTileBounds.Y];
        }

        private void OnRenderBloom()
        {
            Camera camera = (base.Scene as Level).Camera;
            new Rectangle((int)camera.Left, (int)camera.Top, (int)(camera.Right - camera.Left), (int)(camera.Bottom - camera.Top));
            foreach (IBarrierRenderable item in _list)
            {
                if (item.Visible)
                {
                    Draw.Rect(item.X, item.Y, item.Width, item.Height, item.Color);
                }
            }

            foreach (Edge edge in _edges)
            {
                if (edge.Visible)
                {
                    Vector2 vector = edge.Parent.Position + edge.A;
                    _ = edge.Parent.Position + edge.B;
                    for (int i = 0; (float)i <= edge.Length; i++)
                    {
                        Vector2 vector2 = vector + edge.Normal * i;
                        Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[i], edge.Color);
                    }
                }
            }
        }

        public override void Render()
        {
            if (_list.Count <= 0)
            {
                return;
            }

            Color value = Color.White * 0.25f;
            foreach (IBarrierRenderable item in _list)
            {
                if (item.Visible)
                {
                    Draw.Rect(item.Collider, item.Color);
                }
            }

            if (_edges.Count <= 0)
            {
                return;
            }

            foreach (Edge edge in _edges)
            {
                if (edge.Visible)
                {
                    Vector2 vector = edge.Parent.Position + edge.A;
                    _ = edge.Parent.Position + edge.B;
                    Color.Lerp(value, Color.White, edge.Parent.Flash);
                    for (int i = 0; (float)i <= edge.Length; i++)
                    {
                        Vector2 vector2 = vector + edge.Normal * i;
                        Draw.Line(vector2, vector2 + edge.Perpendicular * edge.Wave[i], edge.Color);
                    }
                }
            }
        }

        private List<IBarrierRenderable> _list = new List<IBarrierRenderable>();
        private List<Edge> _edges = new List<Edge>();
        private VirtualMap<bool> _tiles;
        private Rectangle _levelTileBounds;
        private bool _dirty;
    }
}
