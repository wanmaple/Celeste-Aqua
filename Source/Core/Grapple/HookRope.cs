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
        private struct PivotCandidate
        {
            public float positiveCosValue;
            public float distanceSquareToPivot;
            public RopePivot pivot;

            public PivotCandidate(float cosVal, float disSq, RopePivot p)
            {
                positiveCosValue = cosVal;
                distanceSquareToPivot = disSq;
                pivot = p;
            }
        }

        private class PivotCandidateComparer : IComparer<PivotCandidate>
        {
            public int Compare(PivotCandidate x, PivotCandidate y)
            {
                int result = x.positiveCosValue.CompareTo(y.positiveCosValue);
                if (result == 0)
                {
                    result = x.distanceSquareToPivot.CompareTo(y.distanceSquareToPivot);
                }
                return result;
            }
        }

        private static readonly PivotCandidateComparer PivotComparer = new PivotCandidateComparer();

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
                return hook.ExactPosition;
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
                CurrentDirection = Calc.SafeNormalize(TopPivot.point - player.Center);
            }
            else
            {
                CurrentDirection = Calc.SafeNormalize(TopPivot.point - _pivots[1].point);
            }
        }

        public void PivotsFollowAttachment(Entity entity, Vector2 movement)
        {
            if (movement != Vector2.Zero)
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
        }

        public void PrepareCheckCollision(Segment playerSeg)
        {
            _lastPivots.RecordPivots(_pivots, playerSeg.Point1);
            UpdatePivots(playerSeg);
        }

        public void CheckCollision(Segment playerSeg)
        {
            int lastIdx = 0;
            for (int i = 1; i < _pivots.Count;)
            {
                RopePivot prevPivot = _pivots[i - 1];
                RopePivot curPivot = _pivots[i];
                _lastPivots.GetSegments(lastIdx, _lastSegments);
                _addedPivots.Clear();
                if (CheckCollisionHangables(prevPivot, curPivot, _lastSegments, _addedPivots))
                {
                    for (int j = _addedPivots.Count - 1; j >= 0; --j)
                    {
                        _pivots.Insert(i, _addedPivots[j]);
                        AquaDebugger.LogInfo("Add Pivot {0}", _addedPivots[j]);
                    }
                    i += _addedPivots.Count;
                }
                else
                {
                    ++i;
                    ++lastIdx;
                }
            }

            if (!AquaMaths.IsApproximateEqual(playerSeg.Point2, _pivots[_pivots.Count - 1].point))
            {
                _lastPivots.GetSegments(lastIdx, _lastSegments);
                _addedPivots.Clear();
                if (CheckCollisionHangables(_pivots[_pivots.Count - 1], new RopePivot(playerSeg.Point2, Cornors.Free, null), _lastSegments, _addedPivots))
                {
                    for (int i = 0; i < _addedPivots.Count; ++i)
                    {
                        _pivots.Add(_addedPivots[i]);
                        AquaDebugger.LogInfo("Add Pivot {0}", _addedPivots[i]);
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
                float afterLength = CalculateRopeLength(player.Center);
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
            RopePivot hookPivot = new RopePivot(grapple.Position, Cornors.Free, null);
            _pivots.Add(hookPivot);
            _prevRopePivot = hookPivot;
            _prevLength = 0.0f;
            _lockLength = MaxLength;
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = (Entity as GrapplingHook).Owner;
            _prevLength = CalculateRopeLength(player.Center);
            _renderer.Update(Engine.DeltaTime);
        }

        public override void Render()
        {
            Player player = (Entity as GrapplingHook).Owner;
            _segments.Clear();
            for (int i = 0; i < _pivots.Count; ++i)
            {
                Vector2 pt1 = _pivots[i].point;
                Vector2 pt2 = i == _pivots.Count - 1 ? player.Center : _pivots[i + 1].point;
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
                            AquaDebugger.LogInfo("Remove Pivot {0}", pivot);
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

        private bool CheckCollisionHangables(RopePivot prevPivot, RopePivot currentPivot, IList<RopeSegment> lastSegments, IList<RopePivot> addedPivots)
        {
            RopeSegment ropeSeg = new RopeSegment(prevPivot, currentPivot);
            if (AquaMaths.IsApproximateZero(ropeSeg.Length))
            {
                return false;
            }

            List<Entity> solids = Scene.Tracker.GetEntities<Solid>();
            SortedSet<PivotCandidate> potentials = new SortedSet<PivotCandidate>(PivotComparer);
            foreach (Solid solid in solids)
            {
                if (!solid.Collidable || solid.Collider == null || !solid.IsHookable()) continue;
                CheckCollisionSolid(ropeSeg, lastSegments, solid, potentials);
            }
            while (potentials.Count > 0)
            {
                PivotCandidate min = potentials.Min;
                RopePivot pivot = potentials.Min.pivot;
                if (!addedPivots.Contains(pivot))
                {
                    addedPivots.Add(pivot);
                    ropeSeg.Pivot1 = pivot;
                    CheckCollisionHangables(pivot, currentPivot, lastSegments, addedPivots);
                    break;
                }
                else
                {
                    potentials.Remove(min);
                }
            }
            return addedPivots.Count > 0;
        }

        private void CheckCollisionSolid(RopeSegment ropeSeg, IList<RopeSegment> lastSegments, Solid solid, SortedSet<PivotCandidate> potentials)
        {
            Collider collider = solid.Collider;
            if (collider is ColliderList list)
            {
                foreach (Collider single in list.colliders)
                {
                    HandleSingleCollider(single, ropeSeg, lastSegments, solid, potentials);
                }
            }
            else
            {
                HandleSingleCollider(collider, ropeSeg, lastSegments, solid, potentials);
            }
        }

        private void HandleSingleCollider(Collider collider, RopeSegment ropeSeg, IList<RopeSegment> lastSegments, Solid solid, SortedSet<PivotCandidate> potentials)
        {
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
                        TryAddPotentialPoint(pts[i], cornors[i], ropeSeg, solid, lastSegments, potentials);
                    }
                }
            }
            else if (collider is Circle circle)
            {
                // I guess I won't handle this.
            }
            else if (collider is Grid grid)
            {
                IReadOnlyList<RopePivot> convexPoints = grid.ConvexPoints();
                foreach (RopePivot pt in convexPoints)
                {
                    TryAddPotentialPoint(pt.point, pt.direction, ropeSeg, solid, lastSegments, potentials);
                }
            }
        }

        private void TryAddPotentialPoint(Vector2 pt, Cornors direction, RopeSegment ropeSeg, Solid solid, IList<RopeSegment> lastSegments, SortedSet<PivotCandidate> potentials)
        {
            Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
            RopePivot prevPivot = ropeSeg.Pivot1;
            RopePivot currentPivot = ropeSeg.Pivot2;
            Vector2 prevPt = pt - moveVec;
            Vector2 curPt = pt;
            if (solid == prevPivot.entity && AquaMaths.IsApproximateEqual(curPt, prevPivot.point))
                return;
            if (solid == currentPivot.entity && AquaMaths.IsApproximateEqual(curPt, currentPivot.point))
                return;
            for (int i = 1; i < lastSegments.Count; ++i)
            {
                if (solid == lastSegments[i].Pivot1.entity && AquaMaths.IsApproximateEqual(curPt, lastSegments[i].Pivot1.point))
                    return;
            }
            RopePivot pivot = new RopePivot(curPt, direction, solid);
            if (WillHitPivot(prevPt, pivot, lastSegments, ropeSeg))
            {
#if DEBUG
                GrappleUnitTest test = new GrappleUnitTest();
                test.PreviousPivots = new List<Vector2>();
                test.PreviousPivots.Add(lastSegments[0].Pivot1.point);
                for (int i = 0; i < lastSegments.Count; i++)
                {
                    test.PreviousPivots.Add(lastSegments[i].Pivot2.point);
                }
                test.CurrentPivot1 = ropeSeg.Pivot1.point;
                test.CurrentPivot2 = ropeSeg.Pivot2.point;
                test.PreviousPoint = prevPt;
                test.CurrentPoint = curPt;
                using (var fs = new System.IO.FileStream("d:/messy/grapple_test.yaml", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    test.Write(fs);
                }
#endif
                Vector2 dir = Calc.SafeNormalize(curPt - ropeSeg.Pivot1.point);
                float cosValue = Vector2.Dot(dir, ropeSeg.Direction);
                float disSq = Vector2.DistanceSquared(curPt, ropeSeg.Pivot1.point);
                PivotCandidate pp = new PivotCandidate(cosValue, disSq, pivot);
                potentials.Add(pp);
            }
        }

#if DEBUG
        public bool DebugTest(GrappleUnitTest test)
        {
            Vector2 prevPt = test.PreviousPoint;
            RopePivot pivot = new RopePivot(test.CurrentPoint, Cornors.Free, null);
            RopeSegment ropeSeg = new RopeSegment(new RopePivot(test.CurrentPivot1, Cornors.Free, null), new RopePivot(test.CurrentPivot2, Cornors.Free, null));
            List<RopeSegment> lastSegs = new List<RopeSegment>();
            if (test.PreviousPivots.Count == 2)
            {
                lastSegs.Add(new RopeSegment(new RopePivot(test.PreviousPivots[0], Cornors.Free, null), new RopePivot(test.PreviousPivots[1], Cornors.Free, null)));
            }
            else
            {
                Vector2 prev = test.PreviousPivots[0];
                for (int i = 1; i < test.PreviousPivots.Count; ++i)
                {
                    Vector2 cur = test.PreviousPivots[i];
                    lastSegs.Add(new RopeSegment(new RopePivot(prev, Cornors.Free, null), new RopePivot(cur, Cornors.Free, null)));
                    prev = cur;
                }
            }
            return WillHitPivot(prevPt, pivot, lastSegs, ropeSeg);
        }
#endif

        private bool WillHitPivot(Vector2 prevPt, RopePivot pivot, IList<RopeSegment> lastSegments, RopeSegment ropeSeg)
        {
            Vector2 currentPt = pivot.point;
            Vector2 movement = currentPt - prevPt;
            unsafe
            {
                int vertNum = lastSegments.Count + 3;
                Vector2* ptrs = stackalloc Vector2[vertNum];
                ptrs[0] = lastSegments[0].Pivot1.point;
                int i = 1;
                for (; i < lastSegments.Count; i++)
                {
                    ptrs[i] = lastSegments[i].Pivot1.point;
                }
                ptrs[i++] = lastSegments[lastSegments.Count - 1].Pivot2.point;
                ptrs[i++] = ropeSeg.Pivot2.point;
                ptrs[i++] = ropeSeg.Pivot1.point;

                bool isStatic = movement == Vector2.Zero;
                //if (!isStatic)
                //{
                //    isStatic = CheckDuplication(pivot, lastSegments, ropeSeg);
                //}
                if (isStatic)
                {
                    if (AquaMaths.IsPointInsidePolygon(currentPt, ptrs, vertNum, false))
                    {
                        AquaDebugger.LogInfo("Potential IN POLYGON: {0}", pivot);
                        return true;
                    }
                }
                else
                {
                    for (int j = 0; j < lastSegments.Count; j++)
                    {
                        RopeSegment lastSeg = lastSegments[j];
                        Vector2 vec1 = lastSeg.Pivot2.point - lastSeg.Pivot1.point;
                        Vector2 vec2 = ropeSeg.Pivot2.point - ropeSeg.Pivot1.point;
                        Vector2 vec3 = prevPt - lastSeg.Pivot1.point;
                        Vector2 vec4 = currentPt - ropeSeg.Pivot1.point;
                        float cross1 = AquaMaths.Cross(vec3, vec1);
                        float cross2 = AquaMaths.Cross(vec4, vec2);
                        int sign1 = MathF.Sign(cross1);
                        int sign2 = MathF.Sign(cross2);
                        if (sign1 != sign2 && sign1 != 0 && sign2 != 0)
                        {
                            if (AquaMaths.IsSegmentIntersectsSegment(prevPt, currentPt, lastSeg.Pivot1.point, lastSeg.Pivot2.point) || AquaMaths.IsSegmentIntersectsSegment(prevPt, currentPt, ropeSeg.Pivot1.point, ropeSeg.Pivot2.point))
                            {
                                AquaDebugger.LogInfo("Potential MOVE1: {0}", pivot);
                                return true;
                            }
                            if (lastSeg.Pivot1.point != ropeSeg.Pivot1.point || lastSeg.Pivot2.point != ropeSeg.Pivot2.point)
                            {
                                if (AquaMaths.IsPointInsidePolygon(currentPt, ptrs, vertNum, false))
                                {
                                    AquaDebugger.LogInfo("Potential MOVE2", pivot);
                                    return true;
                                }
                            }
                        }
                    }
                }
                bool onLastSeg = false;
                for (int j = 0; j < lastSegments.Count; j++)
                {
                    RopeSegment lastSeg = lastSegments[j];
                    if (j == 0 && prevPt == lastSeg.Pivot1.point && pivot.entity == lastSeg.Pivot1.entity)
                        continue;
                    if (j == lastSegments.Count - 1 && prevPt == lastSeg.Pivot2.point && pivot.entity == lastSeg.Pivot2.entity)
                        continue;
                    if (AquaMaths.IsPointOnSegment(prevPt, lastSeg.Pivot1.point, lastSeg.Pivot2.point))
                    {
                        onLastSeg = true;
                        break;
                    }
                }
                if (onLastSeg)
                {
                    // the trick here won't intersect the pivot if the segments crossed the entity entirely.
                    if (pivot.entity.Collider.CheckLineWithoutEdge(ropeSeg.Pivot1.point, ropeSeg.Pivot2.point))
                    {
                        AquaDebugger.LogInfo("Potential 0 to N0: {0}", pivot);
                        return true;
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

        private bool CheckDuplication(RopePivot pivot, IList<RopeSegment> lastSegments, RopeSegment ropeSeg)
        {
            _setToCheckDuplication.Clear();
            _setToCheckDuplication.Add(pivot.entity);
            foreach (RopeSegment lastSeg in lastSegments)
            {
                if (lastSeg.Pivot1.entity != null)
                    _setToCheckDuplication.Add(lastSeg.Pivot1.entity);
                if (lastSeg.Pivot2.entity != null)
                    _setToCheckDuplication.Add(lastSeg.Pivot2.entity);
            }
            if (ropeSeg.Pivot1.entity != null)
                _setToCheckDuplication.Add(ropeSeg.Pivot1.entity);
            if (ropeSeg.Pivot2.entity != null)
                _setToCheckDuplication.Add(ropeSeg.Pivot2.entity);
            return _setToCheckDuplication.Count == 1;
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
        private List<RopePivot> _addedPivots = new List<RopePivot>(8);
        private List<RopeSegment> _lastSegments = new List<RopeSegment>(8);
        private HashSet<Entity> _setToCheckDuplication = new HashSet<Entity>(8);
        private RopePivot _prevRopePivot;
        internal float _prevLength;
        private float _lockLength;

        private RopeRenderer _renderer;
        private List<Segment> _segments = new List<Segment>(8);

        private const float TOLERANCE = 1.5f;
    }
}
