using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.Aqua.Core.GrapplingHook;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
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
                int result = x.alongDistance.CompareTo(y.alongDistance);
                if (result == 0)
                {
                    result = -x.perpDistance.CompareTo(y.perpDistance);
                }
                return result;
            }
        }

        private static readonly PotentialPointComparer AlongPerpComparer = new PotentialPointComparer();

        public float MaxLength { get; private set; }
        public Vector2 CurrentDirection { get; set; }
        public float EmitSpeed { get; set; }
        public bool ElectricShocking
        {
            get => _renderer.ElectricShocking;
            set => _renderer.ElectricShocking = value;
        }

        public IReadOnlyList<RopePivot> AllPivots => _pivots;
        public RopePivot TopPivot => _pivots[0];
        public RopePivot BottomPivot => _pivots[_pivots.Count - 1];
        public float LockedLength => _lockLength;

        public float SwingRadius
        {
            get
            {
                if (Scene == null)
                {
                    throw new AquaException("Invalid call timing.", "HookRope.SwingRadius");
                }

                Player player = (Entity as GrapplingHook).Owner;
                if (player == null) return 0.0f;
                return (BottomPivot.point - player.Center).Length();
            }
        }
        public Vector2 HookDirection
        {
            get
            {
                if (Scene == null)
                {
                    throw new AquaException("Invalid call timing.", "HookRope.HookDirection");
                }

                Player player = (Entity as GrapplingHook).Owner;
                if (_pivots.Count == 1)
                {
                    Vector2 direction = player.Center - TopPivot.point;
                    if (AquaMaths.IsApproximateZero(direction))
                        return -Vector2.UnitY;
                    return Calc.SafeNormalize(direction);
                }
                return Calc.SafeNormalize(_pivots[1].point - TopPivot.point);
            }
        }
        public Vector2 RopeDirection
        {
            get
            {
                if (Scene == null)
                {
                    throw new AquaException("Invalid call timing.", "HookRope.SwingDirection");
                }

                Player player = (Entity as GrapplingHook).Owner;
                if (player == null) return Vector2.UnitX;
                return Calc.SafeNormalize(player.Center - BottomPivot.point);
            }
        }
        public Vector2 SwingDirection
        {
            get
            {
                if (Scene == null)
                {
                    throw new AquaException("Invalid call timing.", "HookRope.SwingDirection");
                }

                Vector2 ropeDirection = RopeDirection;
                return new Vector2(ropeDirection.Y, -ropeDirection.X) * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f);
            }
        }

        static HookRope()
        {
            _methodPlayerNotCollideDreamBlock = typeof(HookRope).GetMethod("CanPlayerCollideDreamBlock", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo _methodPlayerNotCollideDreamBlock;

        public HookRope(float maxLength, RopeMaterial material)
            : base(true, true)
        {
            MaxLength = maxLength;
            ChangeMaterial(material);
        }

        public void ChangeLength(float length)
        {
            MaxLength = length;
        }

        public void ChangeMaterial(RopeMaterial material)
        {
            string spriteName = string.Empty;
            switch (material)
            {
                case RopeMaterial.Metal:
                    spriteName = "Aqua_RopeMetal";
                    break;
                case RopeMaterial.Default:
                default:
                    spriteName = "Aqua_Rope";
                    break;
            }
            _renderer = new RopeRenderer(spriteName);
        }

        public Vector2 DetectHookNextPosition(float dt, bool revoking, float speedCoeff, out bool changeState)
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.DetectHookNextPosition");
            }

            changeState = false;
            float hookMovement = EmitSpeed * speedCoeff * dt;
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = (Entity as GrapplingHook).Owner;
            if (player == null)
                return hook.Position;
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
                return player.ExactCenter() + CurrentDirection * currentLength;
            }

            Vector2 nextPosition = Vector2.Zero;
            Vector2 lastPos = player.ExactCenter();
            float ropeLength = 0.0f, lastRopeLength = 0.0f;
            int toRmIdx = -1;
            for (int i = _pivots.Count - 1; i >= 1; i--)
            {
                Vector2 pivot = _pivots[i].point;
                ropeLength += (pivot - lastPos).Length();
                if (ropeLength >= currentLength)
                {
                    toRmIdx = i;
                    CurrentDirection = Calc.SafeNormalize(pivot - lastPos);
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
                Player player = (Entity as GrapplingHook).Owner;
                if (player == null) return;
                CurrentDirection = Calc.SafeNormalize(TopPivot.point - player.ExactCenter());
            }
            else
            {
                CurrentDirection = Calc.SafeNormalize(TopPivot.point - _pivots[1].point);
            }
        }

        public void PivotsFollowAttachment(Entity entity, Vector2 movement)
        {
            for (int i = 0; i < _pivots.Count; ++i)
            {
                RopePivot pivot = _pivots[i];
                if (pivot.entity == entity)
                {
                    pivot.movement += movement;
                    _pivots[i] = pivot;
                }
            }
        }

        public void PrepareCheckCollision(Segment playerSeg)
        {
            _lastPivots.RecordPivots(_pivots, playerSeg.Point1);
            UpdatePivots(playerSeg);
        }

        public void CheckCollision(Segment playerSeg)
        {
            List<RopePivot> addedPivots = new List<RopePivot>(8);
            int lastIdx = 0;
            for (int i = 1; i < _pivots.Count;)
            {
                RopePivot prevPivot = _pivots[i - 1];
                RopePivot curPivot = _pivots[i];
                List<Segment> lastSegments = _lastPivots.GetSegments(lastIdx);
                addedPivots.Clear();
                if (CheckCollisionHangables(prevPivot, curPivot, lastSegments, addedPivots))
                {
                    for (int j = addedPivots.Count - 1; j >= 0; --j)
                    {
                        _pivots.Insert(i, addedPivots[j]);
                        AquaDebugger.LogInfo("Add Pivot {0}", addedPivots[j]);
                    }
                    i += addedPivots.Count;
                }
                else
                {
                    ++i;
                    ++lastIdx;
                }
            }

            if (!AquaMaths.IsApproximateEqual(playerSeg.Point2, _pivots[_pivots.Count - 1].point))
            {
                List<Segment> lastSegments = _lastPivots.GetSegments(lastIdx);
                addedPivots.Clear();
                if (CheckCollisionHangables(_pivots[_pivots.Count - 1], new RopePivot(playerSeg.Point2, Cornors.Free, null), lastSegments, addedPivots))
                {
                    for (int i = 0; i < addedPivots.Count; ++i)
                    {
                        _pivots.Add(addedPivots[i]);
                        AquaDebugger.LogInfo("Add Pivot {0}", addedPivots[i]);
                    }
                }
            }
        }

        public bool EnforcePlayer(Player player, Segment playerSeg, float dt)
        {
            GrapplingHook hook = Entity as GrapplingHook;
            float length = CalculateRopeLength(playerSeg.Point2);
            if (length > _lockLength)
            {
                if (player.StateMachine.State == (int)AquaStates.StRedDash || player.StateMachine.State == (int)AquaStates.StDreamDash)
                {
                    if (!Input.GrabCheck && !AquaModule.Settings.AutoGrabRopeIfPossible)
                    {
                        return true;
                    }
                }
                float lengthDiff = length - _lockLength;
                Vector2 ropeDirection = Calc.SafeNormalize(BottomPivot.point - playerSeg.Point2);
                Vector2 movement = ropeDirection * MathF.Ceiling(lengthDiff);
                movement.Y *= (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f);
                if (player.StateMachine.State == (int)AquaStates.StRedDash || player.StateMachine.State == (int)AquaStates.StDreamDash)
                {
                    float connectedLen = CalculateLengthConnectedPivots();
                    Vector2 preferPosition = BottomPivot.point - ropeDirection * (_lockLength - connectedLen);
                    movement = preferPosition - playerSeg.Point2;
                }
                // When the player is dream dashing in the DreamBlock, the Move method will always collide the dreamblock which make the movementCounter becoming zero.
                if (player.StateMachine.State == (int)AquaStates.StDreamDash)
                {
                    player.MakeExtraCollideCondition(_methodPlayerNotCollideDreamBlock);
                }
                if (!AquaMaths.IsApproximateZero(movement.X))
                {
                    player.MoveH(movement.X);
                }
                if (!AquaMaths.IsApproximateZero(movement.Y))
                {
                    player.MoveV(movement.Y);
                }
                if (player.StateMachine.State == (int)AquaStates.StDreamDash)
                {
                    player.MakeExtraCollideCondition(null);
                }
                float afterLength = CalculateRopeLength(player.ExactCenter());
                if (afterLength - _lockLength <= TOLERANCE)  // Make sure it's greater than 1.414
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        private static bool CanPlayerCollideDreamBlock(Entity other)
        {
            if (other is DreamBlock)
                return false;
            return true;
        }

        public void HookAttachEntity(Entity entity)
        {
            RopePivot pivot = _pivots[0];
            pivot.entity = entity;
            _pivots[0] = pivot;
        }

        public void UpdateTopPivot(Vector2 position)
        {
            if (_pivots.Count > 0)
            {
                RopePivot pivot = _pivots[0];
                pivot.point = position;
                _pivots[0] = pivot;
            }
        }

        public void SetLengthLocked(bool locked, Vector2 playerPosition)
        {
            if (locked)
            {
                _lockLength = MathF.Min(CalculateRopeLength(playerPosition), MaxLength);
            }
            else
            {
                _lockLength = MaxLength;
            }
        }

        public void SetLengthLocked(bool locked, float length)
        {
            if (locked)
            {
                _lockLength = length;
            }
            else
            {
                _lockLength = MaxLength;
            }
        }

        public void AddLockedLength(float diff)
        {
            _lockLength = Calc.Clamp(_lockLength + diff, 4.0f, MaxLength);
        }

        public bool ReachLockedLength(Vector2 playerPosition)
        {
            float currentLength = CalculateRopeLength(playerPosition);
            return currentLength >= _lockLength - TOLERANCE;
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);
            _pivots.Clear();
        }

        public override void EntityAwake()
        {
            GrapplingHook grapple = Entity as GrapplingHook;
            RopePivot hookPivot = new RopePivot(grapple.ExactPosition, Cornors.Free, null);
            _pivots.Add(hookPivot);
            _prevRopePivot = hookPivot;
            _prevLength = 0.0f;
            _lockLength = MaxLength;
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = (Entity as GrapplingHook).Owner;
            _prevLength = CalculateRopeLength(player.ExactCenter());
            _renderer.Update(Engine.DeltaTime);
        }

        public override void Render()
        {
            Player player = (Entity as GrapplingHook).Owner;
            _segments.Clear();
            for (int i = 0; i < _pivots.Count; ++i)
            {
                Vector2 pt1 = _pivots[i].point;
                Vector2 pt2 = i == _pivots.Count - 1 ? player.ExactCenter() : _pivots[i + 1].point;
                _segments.Add(new Segment(pt1, pt2));
            }
            _renderer.Render(_segments);
        }

        public override void DebugRender(Camera camera)
        {
            foreach (RopePivot pivot in _pivots)
            {
                Vector2 pt = pivot.point;
                Draw.Circle(pt, 4.0f, Color.Yellow, 16);
            }
        }

        private void UpdatePivots(Segment playerSeg)
        {
            for (int i = 0; i < _pivots.Count;)
            {
                RopePivot pivot = _pivots[i];
                if (pivot.entity != null)
                {
                    if (i != 0 && (pivot.entity.Collider == null || !pivot.entity.Collidable || !pivot.entity.IsHookable() || pivot.entity.Scene == null))
                    {
                        _pivots.RemoveAt(i);
                        _lastPivots.Decrease(i - 1);
                        AquaDebugger.LogInfo("Remove Pivot {0}", pivot);
                    }
                    else
                    {
                        pivot.point += pivot.movement;
                        pivot.movement = Vector2.Zero;
                        _pivots[i] = pivot;
                        ++i;
                    }
                }
                else
                {
                    pivot.movement = Vector2.Zero;
                    _pivots[i] = pivot;
                    ++i;
                }
            }

            unsafe
            {
                if (_pivots.Count > 1)
                {
                    int* signs = stackalloc int[_pivots.Count - 1];
                    for (int i = _pivots.Count - 1; i >= 1; --i)
                    {
                        RopePivot pivot = _pivots[i];
                        Vector2 curPt = pivot.point;
                        Vector2 curLastPt = _lastPivots.GetPivotLast(i).point;
                        Vector2 prevPtOld = _lastPivots.GetPivotLast(i - 1).point;
                        Vector2 nextPtOld = i == _pivots.Count - 1 ? playerSeg.Point1 : _lastPivots.GetPivotLast(i + 1).point;
                        Segment prevBound = new Segment(prevPtOld, nextPtOld);
                        Vector2 oldToPrev = curLastPt - prevBound.Point1;
                        float crossOld = AquaMaths.Cross(oldToPrev, prevBound.Vector);
                        int sign = MathF.Sign(crossOld);
                        signs[i - 1] = sign;
                    }
                    for (int i = _pivots.Count - 1; i >= 1; --i)
                    {
                        RopePivot pivot = _pivots[i];
                        Vector2 curPt = pivot.point;
                        Vector2 prevPtNew = _pivots[i - 1].point;
                        Vector2 nextPtNew = i == _pivots.Count - 1 ? playerSeg.Point2 : _pivots[i + 1].point;
                        Segment curBound = new Segment(prevPtNew, nextPtNew);
                        if (WillReleasePivot(pivot, curBound, signs[i - 1]))
                        {
                            _pivots.RemoveAt(i);
                            _lastPivots.Decrease(i - 1);
                            AquaDebugger.LogInfo("Remove Pivot {0}", curPt);
                        }
                    }
                }
            }
        }

        private WallBooster CheckCollideWallBooster()
        {
            List<Entity> belts = Scene.Tracker.GetEntities<WallBooster>();
            foreach (WallBooster belt in belts)
            {
                if (belt.CollideCheck(Entity))
                {
                    return belt;
                }
            }
            return null;
        }

        private bool CheckCollisionHangables(RopePivot prevPivot, RopePivot currentPivot, IList<Segment> lastSegments, IList<RopePivot> addedPivots)
        {
            Segment ropeSeg = new Segment(prevPivot.point, currentPivot.point);
            if (AquaMaths.IsApproximateZero(ropeSeg.Length))
            {
                return false;
            }

            List<Entity> solids = Scene.Tracker.GetEntities<Solid>();
            SortedSet<PotentialPoint> potentials = new SortedSet<PotentialPoint>(AlongPerpComparer);
            foreach (Solid solid in solids)
            {
                if (!solid.Collidable || solid.Collider == null || !solid.IsHookable()) continue;
                CheckCollisionSolid(prevPivot, currentPivot, lastSegments, solid, potentials);
            }
            if (potentials.Count > 0)
            {
                RopePivot pivot = potentials.Min.pivot;
                addedPivots.Add(pivot);
                ropeSeg.Point1 = pivot.point;
                CheckCollisionHangables(pivot, currentPivot, lastSegments, addedPivots);
            }
            return addedPivots.Count > 0;
        }

        private void CheckCollisionSolid(RopePivot prevPivot, RopePivot currentPivot, IList<Segment> lastSegments, Solid solid, SortedSet<PotentialPoint> potentials)
        {
            Collider collider = solid.Collider;
            if (collider is ColliderList list)
            {
                foreach (Collider single in list.colliders)
                {
                    HandleSingleCollider(single, prevPivot, currentPivot, lastSegments, solid, potentials);
                }
            }
            else
            {
                HandleSingleCollider(collider, prevPivot, currentPivot, lastSegments, solid, potentials);
            }
        }

        private void HandleSingleCollider(Collider collider, RopePivot prevPivot, RopePivot currentPivot, IList<Segment> lastSegments, Solid solid, SortedSet<PotentialPoint> potentials)
        {
            Segment curRopeSeg = new Segment(prevPivot.point, currentPivot.point);
            Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
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
                        TryAddPotentialPoint(pts[i], cornors[i], prevPivot, currentPivot, solid, moveVec, lastSegments, curRopeSeg, potentials);
                    }
                }
            }
            else if (collider is Circle circle)
            {
                // IDK if there will be a circle collider, TODO
            }
            else if (collider is Grid grid)
            {
                IReadOnlyList<RopePivot> convexPoints = grid.ConvexPoints();
                foreach (RopePivot pt in convexPoints)
                {
                    TryAddPotentialPoint(pt.point, pt.direction, prevPivot, currentPivot, solid, moveVec, lastSegments, curRopeSeg, potentials);
                }
            }
        }

        private void TryAddPotentialPoint(Vector2 pt, Cornors direction, RopePivot prevPivot, RopePivot currentPivot, Entity solid, Vector2 moveVec, IList<Segment> lastSegments, Segment curRopeSeg, SortedSet<PotentialPoint> potentials)
        {
            Vector2 prevPt = pt - moveVec;
            Vector2 curPt = pt;
            if (curPt == prevPivot.point && solid == prevPivot.entity)
                return;
            if (curPt == currentPivot.point && solid == currentPivot.entity)
                return;
            RopePivot pivot = new RopePivot(curPt, direction, solid);
            Vector2 current = curPt - curRopeSeg.Point1;
            float alongDis = Vector2.Dot(current, curRopeSeg.Direction);
            if (alongDis > 0.0f && alongDis < curRopeSeg.Length)
            {
                if (WillHitPivot(prevPt, pivot, lastSegments, curRopeSeg))
                {
                    Vector2 prev = curPt - curRopeSeg.Point1;
                    Vector2 alongPerp = new Vector2(prev.Y, -prev.X);
                    float perpDis = MathF.Abs(Vector2.Dot(prev, alongPerp));
                    PotentialPoint pp = new PotentialPoint(alongDis, perpDis, pivot);
                    potentials.Add(pp);
                }
            }
        }

        private bool WillHitPivot(Vector2 prevPt, RopePivot pivot, IList<Segment> lastSegments, Segment currentRopeSeg)
        {
            Vector2 currentPt = pivot.point;
            Vector2 movement = currentPt - prevPt;
            if (movement == Vector2.Zero)
            {
                unsafe
                {
                    int vertNum = lastSegments.Count + 3;
                    Vector2* ptrs = stackalloc Vector2[vertNum];
                    ptrs[0] = lastSegments[0].Point1;
                    int i = 1;
                    for (; i < lastSegments.Count; i++)
                    {
                        ptrs[i] = lastSegments[i].Point1;
                    }
                    ptrs[i++] = lastSegments[lastSegments.Count - 1].Point2;
                    ptrs[i++] = currentRopeSeg.Point2;
                    ptrs[i++] = currentRopeSeg.Point1;
                    if (AquaMaths.IsPointInsidePolygon(currentPt, ptrs, vertNum, false))
                        return true;
                    bool onLastSeg = false;
                    for (int j = 0; j < lastSegments.Count; j++)
                    {
                        Segment seg = lastSegments[j];
                        if (currentPt != seg.Point1 && currentPt != seg.Point2 && AquaMaths.IsPointOnSegment(currentPt, seg.Point1, seg.Point2))
                        {
                            onLastSeg = true;
                            break;
                        }
                    }
                    if (onLastSeg && Collide.CheckLine(pivot.entity, currentRopeSeg.Point1, currentRopeSeg.Point2))
                        return true;
                }
            }
            else
            {
                Vector2 newToPrev = currentRopeSeg.Point1 - currentPt;
                Vector2 newToNext = currentRopeSeg.Point2 - currentPt;
                float crossNew = AquaMaths.Cross(newToPrev, newToNext);
                int signNew = MathF.Sign(crossNew);
                if (signNew == 0) return false;
                foreach (Segment lastSeg in lastSegments)
                {
                    Vector2 oldToPrev = lastSeg.Point1 - prevPt;
                    Vector2 oldToNext = lastSeg.Point2 - prevPt;
                    if (oldToPrev != Vector2.Zero && oldToNext != Vector2.Zero)
                    {
                        float crossOld = AquaMaths.Cross(oldToPrev, oldToNext);
                        int signOld = MathF.Sign(crossOld);
                        if (signOld != signNew)
                        {
                            if (signOld == 0)
                            {
                                if (IsMoveDirectionCorrect(movement, pivot.direction))
                                    return true;
                            }
                            if (AquaMaths.IsSegmentIntersectsSegment(prevPt, currentPt, currentRopeSeg.Point1, currentRopeSeg.Point2) || AquaMaths.IsSegmentIntersectsSegment(prevPt, currentPt, lastSeg.Point1, lastSeg.Point2))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool WillReleasePivot(RopePivot pivot, Segment currentBound, int signOld)
        {
            Vector2 currentPt = pivot.point;
            Vector2 newToPrev = currentPt - currentBound.Point1;
            float crossNew = AquaMaths.Cross(newToPrev, currentBound.Vector);
            int signNew = MathF.Sign(crossNew);
            if (signNew == 0) return true;
            if (signOld != signNew)
            {
                return true;
            }
            return false;
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

        public float CalculateRopeLength(Vector2 playerPosition)
        {
            if (_pivots.Count <= 1)
            {
                return (BottomPivot.point - playerPosition).Length();
            }

            float length = CalculateLengthConnectedPivots();
            length += (BottomPivot.point - playerPosition).Length();
            return length;
        }

        public float CalculateLengthConnectedPivots()
        {
            float length = 0.0f;
            for (int i = 1; i < _pivots.Count; ++i)
            {
                Segment seg = new Segment(_pivots[i].point, _pivots[i - 1].point);
                length += seg.Length;
            }
            return length;
        }

        private List<RopePivot> _pivots = new List<RopePivot>(8);
        private ExpiredPivotList _lastPivots = new ExpiredPivotList();
        private RopePivot _prevRopePivot;
        internal float _prevLength;
        private float _lockLength;

        private RopeRenderer _renderer;
        private List<Segment> _segments = new List<Segment>(8);

        private const float TOLERANCE = 1.5f;
    }
}
