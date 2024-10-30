using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public class HookRope : Component
    {
        private struct PotentialPoint
        {
            public float alongDistance;
            public float perpDistance;
            public RopePivot pivot;

            public PotentialPoint(float alongDis, float perpDis, RopePivot p)
            {
                alongDistance = alongDis;
                perpDistance = perpDis;
                pivot = p;
            }
        }

        private class PotentialPointComparer : IComparer<PotentialPoint>
        {
            public int Compare(PotentialPoint x, PotentialPoint y)
            {
                int perpResult = x.perpDistance.CompareTo(y.perpDistance);
                if (perpResult == 0)
                {
                    return -x.alongDistance.CompareTo(y.alongDistance);
                }
                return perpResult;
            }
        }

        private static readonly PotentialPointComparer PPComparer = new PotentialPointComparer();

        public float MaxLength { get; private set; }
        public Vector2 CurrentDirection { get; set; }
        public float EmitSpeed { get; set; }

        public Vector2 TopPivot => _pivots[0].point;
        public Vector2 BottomPivot => _pivots[_pivots.Count - 1].point;

        public HookRope(float maxLength) : base(false, false)
        {
            MaxLength = maxLength;
        }

        public bool ReleasePivotsIfPossible()
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.DetectHookNextPosition");
            }

            Player player = Scene.Tracker.GetEntity<Player>();
            Vector2 prevPosition = player.PreviousPosition + player.Center - player.Position;
            Vector2 currentPosition = player.Center;
            if (prevPosition == currentPosition) return false;

            bool ret = false;
            while (_pivots.Count >= 2)
            {
                RopePivot currentPivot = _pivots[_pivots.Count - 1];
                Vector2 currentPt = currentPivot.point;
                Vector2 prevPt = _pivots[_pivots.Count - 2].point;
                Vector2 v1 = prevPt - prevPosition;
                Vector2 v2 = currentPt - prevPosition;
                Vector2 v3 = prevPt - currentPosition;
                Vector2 v4 = currentPt - currentPosition;
                float crossPrev = MathF.Sign(AquaMaths.Cross(v1, v2));
                float crossCur = MathF.Sign(AquaMaths.Cross(v3, v4));
                int signPrev = MathF.Sign(crossPrev);
                int signCur = MathF.Sign(crossCur);
                if (signPrev == signCur || signCur == 0) break;
                if (signPrev == 0)
                {
                    Vector2 playerVec = currentPosition - prevPosition;
                    int sign = MathF.Sign(AquaMaths.Cross(playerVec, v2));
                    if (sign == 0) break;
                    switch (currentPivot.direction)
                    {
                        case Cornors.TopLeft:
                            sign *= (v2.X == 0.0f ? MathF.Sign(v2.Y) : -MathF.Sign(v2.X));
                            break;
                        case Cornors.TopRight:
                            sign *= (v2.X == 0.0f ? -MathF.Sign(v2.Y) : -MathF.Sign(v2.X));
                            break;
                        case Cornors.BottomLeft:
                            sign *= (v2.X == 0.0f ? MathF.Sign(v2.Y) : MathF.Sign(v2.X));
                            break;
                        case Cornors.BottomRight:
                            sign *= (v2.X == 0.0f ? -MathF.Sign(v2.Y) : MathF.Sign(v2.X));
                            break;
                    }
                    if (sign > 0) break;
                }
                _justReleased.Add(BottomPivot);
                AquaLogger.LogInfo("[{0}] Pivot Remove {1}", Engine.FrameCounter, BottomPivot);
                _pivots.RemoveAt(_pivots.Count - 1);
                ret = true;
            }
            if (ret)
            {
            }
            return ret;
        }

        public Vector2 DetectHookNextPosition(float dt, bool revoking, out bool changeState)
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.DetectHookNextPosition");
            }

            changeState = false;
            float hookMovement = EmitSpeed * dt;
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = Scene.Tracker.GetEntity<Player>();
            float currentLength = _prevLength;
            if (revoking)
            {
                currentLength -= hookMovement;
                if (currentLength <= 0.0f)
                {
                    currentLength = 0.0f;
                    changeState = true;
                }
            }
            else
            {
                currentLength += hookMovement;
                if (currentLength >= MaxLength)
                {
                    currentLength = MaxLength;
                    changeState = true;
                }
            }
            if (_pivots.Count <= 1)
            {
                return player.Center + CurrentDirection * currentLength;
            }

            Vector2 nextPosition = Vector2.Zero;
            Vector2 lastPos = player.Center;
            float ropeLength = 0.0f, lastRopeLength = 0.0f;
            int toRmIdx = -1;
            for (int i = _pivots.Count - 1; i >= 1; i--)
            {
                Vector2 pivot = _pivots[i].point;
                ropeLength += (pivot - lastPos).Length();
                if (ropeLength >= currentLength)
                {
                    toRmIdx = i;
                    CurrentDirection = Vector2.Normalize(pivot - lastPos);
                    nextPosition = lastPos + CurrentDirection * (currentLength - lastRopeLength);
                    break;
                }
                else
                {
                    lastPos = pivot;
                }
                lastRopeLength = ropeLength;
            }
            if (toRmIdx < 0)
            {
                nextPosition = lastPos + CurrentDirection * (currentLength - ropeLength);
            }
            else if (toRmIdx > 0)
            {
                _pivots.RemoveRange(1, toRmIdx);
            }
            return nextPosition;
        }

        public void UpdateCurrentDirection()
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.DetectHookNextPosition");
            }

            if (_pivots.Count <= 1)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                CurrentDirection = Vector2.Normalize(TopPivot - player.Center);
            }
            else
            {
                CurrentDirection = Vector2.Normalize(TopPivot - _pivots[1].point);
            }
        }

        public bool CheckCollision(Segment ropeSeg, Segment playerSeg)
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.CheckCollision");
            }

            return CheckCollisionInternal(ropeSeg, playerSeg);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);

            _pivots.Clear();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
        }

        public override void EntityAwake()
        {
            RopePivot hookPivot = new RopePivot(Entity.Position, Cornors.Free, Entity);
            _pivots.Add(hookPivot);
            _prevLength = 0.0f;
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            RopePivot pivot = _pivots[0];
            pivot.point = hook.Position;
            _pivots[0] = pivot;
            if (hook.State == GrapplingHook.HookStates.Fixed)
            {
            }
            _prevLength = CalculateRopeLength();
            _justReleased.Clear();
        }

        public override void Render()
        {
            Color lineColor = Color.White;
            for (int i = 0; i < _pivots.Count; i++)
            {
                if (i == _pivots.Count - 1)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    Draw.Line(_pivots[i].point, player.Center, lineColor);
                }
                else
                {
                    Draw.Line(_pivots[i].point, _pivots[i + 1].point, lineColor);
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
        }

        public override void HandleGraphicsCreate()
        {
            base.HandleGraphicsCreate();
        }

        private float CalculateRopeLength()
        {
            float length = 0.0f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (_pivots.Count <= 1)
            {
                return (BottomPivot - player.Center).Length();
            }

            for (int i = 1; i < _pivots.Count; ++i)
            {
                Segment seg = new Segment(_pivots[i].point, _pivots[i - 1].point);
                length += seg.Length;
            }
            length += (BottomPivot - player.Center).Length();
            return length;
        }

        private bool CheckCollisionInternal(Segment ropeSeg, Segment playerSeg)
        {
            if (AquaMaths.IsApproximateZero(ropeSeg.Length) || AquaMaths.IsApproximateZero(playerSeg.Length))
            {
                return false;
            }

            Vector2 alongRope = Vector2.Normalize(ropeSeg.Vector);
            Vector2 alongPlayer = Vector2.Normalize(playerSeg.Vector);
            float crossProduct = AquaMaths.Cross(alongRope, alongPlayer);
            if (AquaMaths.IsApproximateZero(crossProduct))
            {
                return false;
            }

            Vector2 perpRope = alongRope.Rotate(MathF.PI * 0.5f);
            if (Vector2.Dot(alongPlayer, perpRope) < 0.0f)
            {
                perpRope = -perpRope;
            }
            float alongRange = ropeSeg.Length;
            float perpRange = Vector2.Dot(playerSeg.Vector, perpRope);
            SortedSet<PotentialPoint> potentialPoints = new SortedSet<PotentialPoint>(PPComparer);
            List<Entity> solids = Scene.Tracker.GetEntities<Solid>();
            foreach (Entity solid in solids)
            {
                if (!solid.Collidable || solid.Collider == null) continue;
                Segment currentSeg = new Segment(playerSeg.Point2, ropeSeg.Point2);
                Vector2 alongCurrent = Vector2.Normalize(currentSeg.Vector);
                Collider collider = solid.Collider;
                if (collider is Hitbox box)
                {
                    PotentialPoint pp = new PotentialPoint();
                    RopePivot tl = new RopePivot(new Vector2(box.AbsoluteLeft, box.AbsoluteTop), Cornors.TopLeft, solid);
                    RopePivot tr = new RopePivot(new Vector2(box.AbsoluteRight, box.AbsoluteTop), Cornors.TopRight, solid);
                    RopePivot bl = new RopePivot(new Vector2(box.AbsoluteLeft, box.AbsoluteBottom), Cornors.BottomLeft, solid);
                    RopePivot br = new RopePivot(new Vector2(box.AbsoluteRight, box.AbsoluteBottom), Cornors.BottomRight, solid);
                    if (AquaMaths.IsVectorInsideTwoVectors(tl.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(tl.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, ref pp))
                    {
                        pp.pivot = tl;
                        potentialPoints.Add(pp);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(tr.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(tr.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, ref pp))
                    {
                        pp.pivot = tr;
                        potentialPoints.Add(pp);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(bl.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(bl.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, ref pp))
                    {
                        pp.pivot = bl;
                        potentialPoints.Add(pp);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(br.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(br.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, ref pp))
                    {
                        pp.pivot = br;
                        potentialPoints.Add(pp);
                    }
                }
                else if (collider is Circle circle)
                {
                    // IDK if there will be a circle collider, TODO
                }
                else if (collider is Grid grid)
                {
                    List<RopePivot> convexPoints = DynamicData.For(grid).Get<List<RopePivot>>("convex_points");
                    if (convexPoints == null)
                    {
                        convexPoints = grid.FindConvexPoints();
                        DynamicData.For(grid).Set("convex_points", convexPoints);
                    }
                    PotentialPoint pp = new PotentialPoint();
                    foreach (RopePivot pt in convexPoints)
                    {
                        if (AquaMaths.IsVectorInsideTwoVectors(pt.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(pt.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, ref pp))
                        {
                            pp.pivot = pt;
                            potentialPoints.Add(pp);
                        }
                    }
                }
            }
            if (potentialPoints.Count > 0)
            {
                PotentialPoint minPP = potentialPoints.Min;
                _pivots.Add(minPP.pivot);
                AquaLogger.LogInfo("[{0}] Pivot Add {1}", Engine.FrameCounter, minPP.pivot.point);
                ropeSeg = new Segment(ropeSeg.Point1, minPP.pivot.point);
                CheckCollisionInternal(ropeSeg, playerSeg);
                return true;
            }
            return false;
        }

        private bool IsSatisfiedPoint(Vector2 pt, Vector2 pivot, Vector2 alongProj, float alongRange, Vector2 perpProj, float perpRange, ref PotentialPoint pp)
        {
            if (_justReleased.Contains(pt)) return false;
            if (_pivots.Count > 1 && BottomPivot == pt) return false;
            Vector2 vec = pt - pivot;
            float disAlong = Vector2.Dot(vec, alongProj);
            float disPerp = Vector2.Dot(vec, perpProj);
            if (disAlong >= 0.0f && disAlong <= alongRange && disPerp >= 0.0f && disPerp <= perpRange)
            {
                pp.alongDistance = disAlong;
                pp.perpDistance = disPerp;
                return true;
            }
            return false;
        }

        private List<RopePivot> _pivots = new List<RopePivot>(8);
        private HashSet<Vector2> _justReleased = new HashSet<Vector2>(8);
        private float _prevLength;
    }
}
