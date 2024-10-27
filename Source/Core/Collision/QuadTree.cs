using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class QuadTree
    {
        public class QuadTreeNode
        {
            public Rectangle Bounds { get; internal set; }

            public QuadTreeNode Parent { get; private set; }
            public QuadTreeNode[] Children { get; private set; } = new QuadTreeNode[4];
            public IList<Entity> Entities { get; private set; }

            public bool IsLeaf => TopLeft == null && TopRight == null && BottomLeft == null && BottomRight == null;
            public bool IsEmpty => (IsLeaf && Entities.Count <= 0) || (!IsLeaf && TopLeft.IsEmpty && TopRight.IsEmpty && BottomLeft.IsEmpty && BottomRight.IsEmpty);

            public QuadTreeNode TopLeft
            {
                get => Children[0];
                set => Children[0] = value;
            }
            public QuadTreeNode TopRight
            {
                get => Children[1];
                set => Children[1] = value;
            }
            public QuadTreeNode BottomLeft
            {
                get => Children[2];
                set => Children[2] = value;
            }
            public QuadTreeNode BottomRight
            {
                get => Children[3];
                set => Children[3] = value;
            }

            internal QuadTreeNode(Rectangle bounds, QuadTreeNode parent = null)
            {
                Bounds = bounds;
                Parent = parent;
                Entities = new List<Entity>(4);
            }

            public void ClearChildren()
            {
                for (int i = 0; i < 4; ++i)
                {
                    Children[i] = null;
                }
            }
        }

        public Rectangle Bounds
        {
            get => _root.Bounds;
            set
            {
                if (_root != null)
                {
                    Logger.Log(LogLevel.Error, ModConstants.MOD_NAME, "Unable to set bounds to a running QuadTree.");
                    throw new AquaException("Unable to set bounds to a running QuadTree.", "QuadTree.Bounds");
                }

                Rectangle bounds = value;
                bounds.Width = Maths.NextPowerOfTwo(bounds.Width);
                bounds.Height = Maths.NextPowerOfTwo(bounds.Height);
                _root = new QuadTreeNode(bounds);
            }
        }

        public QuadTreeNode Root => _root;
        public bool IsValid => _root != null;

        public QuadTree() { }

        public void AddEntity(Entity entity)
        {
            if (_root == null)
            {
                Logger.Log(LogLevel.Error, ModConstants.MOD_NAME, "Attempted to work on a invalid QuadTree.");
                throw new AquaException("Attempted to work on a invalid QuadTree.", "QuadTree.AddEntity");
            }

            if (entity.Collider == null) return;
            AddEntityInternal(_root, entity);
        }

        public void RemoveEntity(Entity entity)
        {
            if (_root == null)
            {
                Logger.Log(LogLevel.Error, ModConstants.MOD_NAME, "Attempted to work on a invalid QuadTree.");
                throw new AquaException("Attempted to work on a invalid QuadTree.", "QuadTree.RemoveEntity");
            }

            if (entity.Collider == null) return;
            RemoveEntityInternal(_root, entity);
        }

        public void Clear()
        {
            _root = null;
        }

        private void AddEntityInternal(QuadTreeNode node, Entity entity)
        {
            if (!entity.Collider.Collide(node.Bounds)) return;
            Logger.Log(LogLevel.Info, ModConstants.MOD_NAME, string.Format("Add {0}", entity.GetType()));
            if (node.IsLeaf && ((node.Bounds.Width <= 4 || node.Bounds.Height <= 4) || node.Entities.Count < 4))
            {
                node.Entities.Add(entity);
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    QuadTreeNode child = node.Children[i];
                    if (child == null)
                    {
                        Point halfSize = new Point(node.Bounds.Width / 2, node.Bounds.Height / 2);
                        Point orig = node.Bounds.Location + new Point((i % 2) * halfSize.X, (i / 2) * halfSize.Y);
                        node.Children[i] = child = new QuadTreeNode(new Rectangle(orig.X, orig.Y, halfSize.X, halfSize.Y), node);
                        foreach (Entity e in node.Entities)
                        {
                            AddEntityInternal(child, e);
                        }
                    }
                    AddEntityInternal(child, entity);
                }
                node.Entities.Clear();
            }
        }

        private void RemoveEntityInternal(QuadTreeNode node, Entity entity)
        {
            if (!entity.Collider.Collide(node.Bounds)) return;
            Logger.Log(LogLevel.Info, ModConstants.MOD_NAME, string.Format("Rm {0}", entity.GetType()));
            if (node.IsLeaf)
            {
                int index = node.Entities.IndexOf(entity);
                if (index >= 0)
                {
                    if (index != node.Entities.Count - 1)
                    {
                        node.Entities[index] = node.Entities[node.Entities.Count - 1];
                    }
                    node.Entities.RemoveAt(node.Entities.Count - 1);
                    QuadTreeNode current = node.Parent;
                    while (current != null && current.IsEmpty)
                    {
                        current.ClearChildren();
                        current = current.Parent;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; ++i)
                {
                    QuadTreeNode child = node.Children[i];
                    RemoveEntityInternal(child, entity);
                }
            }
        }

        private QuadTreeNode _root = null;
    }
}
