using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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
                //int result = x.perpDistance.CompareTo(y.perpDistance);
                //if (result == 0)
                //{
                //    result = x.alongDistance.CompareTo(y.alongDistance);
                //}
                //return result;
                return x.alongDistance.CompareTo(y.alongDistance);
            }
        }

        private static readonly PotentialPointComparer PPComparer = new PotentialPointComparer();

        public float MaxLength { get; private set; }
        public Vector2 CurrentDirection { get; set; }
        public float EmitSpeed { get; set; }

        public Vector2 TopPivot => _pivots[0].point;
        public Vector2 BottomPivot => _pivots[_pivots.Count - 1].point;
        public float SwingRadius
        {
            get
            {
                if (Scene == null)
                {
                    throw new AquaException("Invalid call timing.", "HookRope.SwingRadius");
                }

                Player player = Scene.Tracker.GetEntity<Player>();
                if (player == null) return 0.0f;
                return (BottomPivot - player.Center).Length();
            }
        }

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
            if (player == null) return false;
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
                _justReleased.Add(_pivots[_pivots.Count - 1]);
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
            if (player == null) return hook.Position;
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
                if (player == null) return;
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

            return CheckCollisionPlayer(ropeSeg, playerSeg, _pivots.Count - 1);
        }

        public void CheckCollision2(Segment playerSeg)
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.CheckCollision");
            }

            UpdatePivots(playerSeg.Point1);
            List<RopePivot> addedPivots = new List<RopePivot>(8);
            for (int i = 1; i < _pivots.Count;)
            {
                Segment ropeSeg = new Segment(_pivots[i - 1].point, _pivots[i].point);
                addedPivots.Clear();
                if (CheckCollisionSolids(ropeSeg, addedPivots))
                {
                    for (int j = addedPivots.Count - 1; j >= 0; --j)
                    {
                        _pivots.Insert(i, addedPivots[j]);
                        AquaLogger.LogInfo("Add Pivot {0}", addedPivots[j]);
                    }
                    i += addedPivots.Count - 1;
                }
                else
                {
                    ++i;
                }
            }

            {
                Segment ropeSeg = new Segment(BottomPivot, playerSeg.Point1);
                addedPivots.Clear();
                if (CheckCollisionSolids(ropeSeg, addedPivots))
                {
                    for (int i = 0; i < addedPivots.Count; ++i)
                    {
                        _pivots.Add(addedPivots[i]);
                        AquaLogger.LogInfo("Add Pivot {0}", addedPivots[i]);
                    }
                }
            }
        }

        public bool CheckCollisionOfSolidMovement(Solid solid, Segment movement)
        {

            return false;
        }

        public bool EnforcePlayer(Player player, Segment playerSeg, float breakSpeed)
        {
            if (player.StateMachine.State != (int)AquaStates.StHanging)
            {
                if (AquaMaths.IsApproximateZero(playerSeg.Vector))
                {
                    return false;
                }
                Vector2 velocity = playerSeg.Vector / Engine.DeltaTime;
                float speed = velocity.Length();
                float length = CalculateRopeLength(playerSeg.Point2);
                if (length > MaxLength)
                {
                    if (speed >= breakSpeed) return true;
                    Vector2 movement = -playerSeg.Vector;
                    if (!AquaMaths.IsApproximateZero(movement.X))
                    {
                        player.MoveH(movement.X, PlayerCollideSolid);
                    }
                    if (!AquaMaths.IsApproximateZero(movement.Y))
                    {
                        player.MoveV(movement.Y, PlayerCollideSolid);
                    }
                    if (_collidedWhenUndo) return true;
                    UndoPivots();
                }
            }

            return false;
        }

        public void HookAttachEntity(Entity entity)
        {
            RopePivot pivot = _pivots[0];
            pivot.entity = entity;
            _pivots[0] = pivot;
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);

            _pivots.Clear();
        }

        public override void EntityAwake()
        {
            RopePivot hookPivot = new RopePivot(Entity.Position, Cornors.Free, null);
            _pivots.Add(hookPivot);
            _prevLength = 0.0f;
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = Scene.Tracker.GetEntity<Player>();
            RopePivot pivot = _pivots[0];
            pivot.point = hook.Position;
            _pivots[0] = pivot;
            if (hook.State == GrapplingHook.HookStates.Fixed)
            {
            }
            _prevLength = CalculateRopeLength(player.Center);
            _justReleased.Clear();
            _justAdded.Clear();
            _collidedWhenUndo = false;
        }

        public override void Render()
        {
            Color lineColor = Color.White;
            for (int i = 0; i < _pivots.Count; i++)
            {
                if (i == _pivots.Count - 1)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player == null) return;
                    Draw.Line(_pivots[i].point, player.Center, lineColor);
                }
                else
                {
                    Draw.Line(_pivots[i].point, _pivots[i + 1].point, lineColor);
                }
            }
        }

        private void UpdatePivots(Vector2 playerPos)
        {
            for (int i = 0; i < _pivots.Count;)
            {
                RopePivot pivot = _pivots[i];
                Solid solid = pivot.entity as Solid;
                if (solid != null)
                {
                    if (solid.Position != solid.GetPreviousPosition())
                    {
                        Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
                        Segment movement = new Segment(pivot.point, pivot.point + moveVec);
                        Vector2 nextPos = i == _pivots.Count - 1 ? playerPos : _pivots[i + 1].point;
                        Segment bound = new Segment((i == 0 ? nextPos : _pivots[i - 1].point), nextPos);
                        if (i > 0 && WillReleasePivot(pivot, bound, movement))
                        {
                            _pivots.RemoveAt(i);
                            AquaLogger.LogInfo("Release Pivot {0}", pivot);
                        }
                        else
                        {
                            RopePivot oldPivot = pivot;
                            Hitbox box = solid.Collider as Hitbox;
                            switch (pivot.direction)
                            {
                                case Cornors.Free:
                                    pivot.point += moveVec;
                                    break;
                                case Cornors.TopLeft:
                                    pivot.point = new Vector2(box.AbsoluteLeft, box.AbsoluteTop);
                                    break;
                                case Cornors.TopRight:
                                    pivot.point = new Vector2(box.AbsoluteRight, box.AbsoluteTop);
                                    break;
                                case Cornors.BottomLeft:
                                    pivot.point = new Vector2(box.AbsoluteLeft, box.AbsoluteBottom);
                                    break;
                                case Cornors.BottomRight:
                                    pivot.point = new Vector2(box.AbsoluteRight, box.AbsoluteBottom);
                                    break;
                            }
                            _pivots[i] = pivot;
                            if (i == 0)
                            {
                                Entity.Position = pivot.point;
                            }
                            //AquaLogger.LogInfo("Change Pivot {0} -> {1}", oldPivot.point, pivot.point);
                            ++i;
                        }
                    }
                    else
                    {
                        ++i;
                    }
                }
                else
                {
                    ++i;
                }
            }
        }

        private bool CheckCollisionSolids(Segment ropeSeg, IList<RopePivot> addedPivots)
        {
            if (AquaMaths.IsApproximateZero(ropeSeg.Length))
            {
                return false;
            }

            Vector2 alongRope = Vector2.Normalize(ropeSeg.Vector);
            List<Entity> solids = Scene.Tracker.GetEntities<Solid>();
            SortedSet<PotentialPoint> pps = new SortedSet<PotentialPoint>(PPComparer);
            foreach (Solid solid in solids)
            {
                if (!solid.Collidable || solid.Collider == null) continue;
                if (solid.GetPreviousPosition() == solid.Position) continue;
                if (!Collide.CheckLine(solid, ropeSeg.Point1, ropeSeg.Point2)) continue;
                CheckCollisionSolid(ropeSeg, solid, pps);
            }
            if (pps.Count > 0)
            {
                RopePivot pivot = pps.Min.pivot;
                addedPivots.Add(pivot);
                ropeSeg.Point1 = pivot.point;
                CheckCollisionSolids(ropeSeg, addedPivots);
            }
            return addedPivots.Count > 0;
        }

        private void CheckCollisionSolid(Segment ropeSeg, Solid solid, SortedSet<PotentialPoint> potentialPoints)
        {
            Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
            Vector2 alongRope = Vector2.Normalize(ropeSeg.Vector);
            Collider collider = solid.Collider;
            if (collider is Hitbox box)
            {
                Vector2 tl = new Vector2(box.AbsoluteLeft, box.AbsoluteTop);
                Vector2 tr = new Vector2(box.AbsoluteRight, box.AbsoluteTop);
                Vector2 bl = new Vector2(box.AbsoluteLeft, box.AbsoluteBottom);
                Vector2 br = new Vector2(box.AbsoluteRight, box.AbsoluteBottom);
                unsafe
                {
                    Vector2* pts = stackalloc Vector2[4] { tl, tr, bl, br };
                    Cornors* cornors = stackalloc Cornors[4] { Cornors.TopLeft, Cornors.TopRight, Cornors.BottomLeft, Cornors.BottomRight };
                    for (int i = 0; i < 4; ++i)
                    {
                        Vector2 prevPt = pts[i] - moveVec;
                        Vector2 curPt = pts[i];
                        if (curPt == ropeSeg.Point1 || curPt == ropeSeg.Point2) continue;
                        Segment movement = new Segment(prevPt, curPt);
                        RopePivot pivot = new RopePivot(curPt, cornors[i], solid);
                        if (WillAddPivot(pivot, ropeSeg, movement))
                        {
                            Vector2 toCurrent = movement.Point2 - ropeSeg.Point1;
                            float alongDis = Vector2.Dot(toCurrent, alongRope);
                            PotentialPoint pp = new PotentialPoint(alongDis, 0.0f, pivot);
                            potentialPoints.Add(pp);
                        }
                    }
                }
            }
            else if (collider is Circle circle)
            {
                // IDK if there will be a circle collider, TODO
            }
            else if (collider is Grid grid)
            {
                // Moveable Solid will never own a grid collider.
            }
        }

        private bool CheckCollisionPlayer(Segment ropeSeg, Segment playerSeg, int insertIndex)
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
                    if (AquaMaths.IsVectorInsideTwoVectors(tl.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(tl.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, insertIndex, ref pp))
                    {
                        pp.pivot = tl;
                        potentialPoints.Add(pp);
                        _justAdded.Add(pp.pivot);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(tr.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(tr.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, insertIndex, ref pp))
                    {
                        pp.pivot = tr;
                        potentialPoints.Add(pp);
                        _justAdded.Add(pp.pivot);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(bl.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(bl.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, insertIndex, ref pp))
                    {
                        pp.pivot = bl;
                        potentialPoints.Add(pp);
                        _justAdded.Add(pp.pivot);
                    }
                    if (AquaMaths.IsVectorInsideTwoVectors(br.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(br.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, insertIndex, ref pp))
                    {
                        pp.pivot = br;
                        potentialPoints.Add(pp);
                        _justAdded.Add(pp.pivot);
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
                        if (AquaMaths.IsVectorInsideTwoVectors(pt.point - ropeSeg.Point2, -alongRope, -alongCurrent) && IsSatisfiedPoint(pt.point, ropeSeg.Point1, alongRope, alongRange, perpRope, perpRange, insertIndex, ref pp))
                        {
                            pp.pivot = pt;
                            potentialPoints.Add(pp);
                            _justAdded.Add(pp.pivot);
                        }
                    }
                }
            }
            if (potentialPoints.Count > 0)
            {
                PotentialPoint minPP = potentialPoints.Min;
                _pivots.Add(minPP.pivot);
                ropeSeg = new Segment(ropeSeg.Point1, minPP.pivot.point);
                CheckCollisionPlayer(ropeSeg, playerSeg, _pivots.Count - 1);
                return true;
            }
            return false;
        }

        private bool WillAddPivot(RopePivot pivot, Segment bound, Segment movement)
        {
            Vector2 toPrev = bound.Point1 - movement.Point1;
            Vector2 toNext = bound.Point2 - movement.Point1;
            bool sameLine = AquaMaths.Cross(toPrev, toNext) == 0.0f;
            if (sameLine && Vector2.Dot(toPrev, toNext) == 1.0f) return false;
            if (!sameLine && !AquaMaths.IsVectorInsideTwoVectors(movement.Vector, toPrev, toNext, false)) return false;
            Vector2 threshold = Vector2.Normalize(bound.Vector);
            Vector2 perpThreshold = new Vector2(threshold.Y, -threshold.X);
            if (Vector2.Dot(movement.Vector, perpThreshold) < 0.0f)
            {
                perpThreshold = -perpThreshold;
            }
            Vector2 vec = movement.Point2 - bound.Point1;
            float perpDis = Vector2.Dot(vec, perpThreshold);
            if (AquaMaths.IsApproximateZero(perpDis)) return false;
            return IsMoveDirectionCorrect(movement.Vector, pivot.direction) && perpDis > 0.0f;
        }

        private bool WillReleasePivot(RopePivot pivot, Segment bound, Segment movement)
        {
            Vector2 threshold = Vector2.Normalize(bound.Vector);
            Vector2 perpThreshold = new Vector2(threshold.Y, -threshold.X);
            if (Vector2.Dot(movement.Vector, perpThreshold) < 0.0f)
            {
                perpThreshold = -perpThreshold;
            }
            Vector2 vec = movement.Point2 - bound.Point1;
            float perpDis = Vector2.Dot(vec, perpThreshold);
            if (AquaMaths.IsApproximateZero(perpDis)) return true;
            return !IsMoveDirectionCorrect(movement.Vector, pivot.direction) && perpDis > 0.0f;
        }

        private bool IsMoveDirectionCorrect(Vector2 alongMove, Cornors cornor)
        {
            switch (cornor)
            {
                case Cornors.TopLeft:
                    return !AquaMaths.IsVectorInsideTwoVectors(alongMove, Vector2.UnitX, Vector2.UnitY);
                case Cornors.TopRight:
                    return !AquaMaths.IsVectorInsideTwoVectors(alongMove, -Vector2.UnitX, Vector2.UnitY);
                case Cornors.BottomLeft:
                    return !AquaMaths.IsVectorInsideTwoVectors(alongMove, -Vector2.UnitY, Vector2.UnitX);
                case Cornors.BottomRight:
                    return !AquaMaths.IsVectorInsideTwoVectors(alongMove, -Vector2.UnitY, -Vector2.UnitX);
            }
            return false;
        }

        private bool IsSatisfiedPoint(Vector2 pt, Vector2 pivot, Vector2 alongProj, float alongRange, Vector2 perpProj, float perpRange, int index, ref PotentialPoint pp)
        {
            if (_justReleased.Any(pvt => pvt.point == pt)) return false;
            //if (_pivots.Count > 1 && _pivots[index].point == pt) return false;
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

        private void UndoPivots()
        {
            _pivots.RemoveRange(_pivots.Count - _justAdded.Count, _justAdded.Count);
            for (int i = _justReleased.Count - 1; i >= 0; i--)
            {
                _pivots.Add(_justReleased[i]);
            }
            _justAdded.Clear();
            _justReleased.Clear();
        }

        private void PlayerCollideSolid(CollisionData collisionData)
        {
            _collidedWhenUndo = true;
        }

        private float CalculateRopeLength(Vector2 playerPosition)
        {
            float length = 0.0f;
            if (_pivots.Count <= 1)
            {
                return (BottomPivot - playerPosition).Length();
            }

            for (int i = 1; i < _pivots.Count; ++i)
            {
                Segment seg = new Segment(_pivots[i].point, _pivots[i - 1].point);
                length += seg.Length;
            }
            length += (BottomPivot - playerPosition).Length();
            return length;
        }

        private List<RopePivot> _pivots = new List<RopePivot>(8);
        private List<RopePivot> _justReleased = new List<RopePivot>(8);
        private List<RopePivot> _justAdded = new List<RopePivot>(8);
        private float _prevLength;
        private bool _collidedWhenUndo = false;
    }
}
