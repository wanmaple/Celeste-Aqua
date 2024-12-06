using Celeste.Mod.Aqua.Debug;
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

                Player player = Scene.Tracker.GetEntity<Player>();
                if (player == null) return 0.0f;
                return (BottomPivot.point - player.ExactCenter()).Length();
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

                Player player = Scene.Tracker.GetEntity<Player>();
                if (_pivots.Count == 1)
                {
                    Vector2 direction = player.ExactCenter() - TopPivot.point;
                    if (AquaMaths.IsApproximateZero(direction))
                        return -Vector2.UnitY;
                    return Vector2.Normalize(direction);
                }
                return Vector2.Normalize(_pivots[1].point - TopPivot.point);
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

                Player player = Scene.Tracker.GetEntity<Player>();
                if (player == null) return Vector2.UnitX;
                return Vector2.Normalize(player.ExactCenter() - BottomPivot.point);
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
                return new Vector2(ropeDirection.Y, -ropeDirection.X);
            }
        }

        public HookRope(float maxLength) : base(false, false)
        {
            MaxLength = maxLength;

            MTexture ropeTexture = GFX.Game["objects/hook/rope"];
            _renderer = new RopeRenderer(ropeTexture);
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
                CurrentDirection = Vector2.Normalize(TopPivot.point - player.ExactCenter());
            }
            else
            {
                CurrentDirection = Vector2.Normalize(TopPivot.point - _pivots[1].point);
            }
        }

        public void CheckCollision(Segment playerSeg)
        {
            if (Scene == null)
            {
                throw new AquaException("Invalid call timing.", "HookRope.CheckCollision");
            }

            _lastPivots.RecordPivots(_pivots, playerSeg.Point1);
            UpdatePivots(playerSeg);
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
            Vector2 hookVelocity = hook.Velocity;
            Vector2 velocity = playerSeg.Vector / dt;
            Vector2 relativeVelocity = velocity - hookVelocity;
            float speed = relativeVelocity.Length();
            float length = CalculateRopeLength(playerSeg.Point2);
            if (length > _lockLength)
            {
                float lengthDiff = length - _lockLength;
                Vector2 ropeDirection = Vector2.Normalize(BottomPivot.point - playerSeg.Point2);
                Vector2 movement = ropeDirection * MathF.Ceiling(lengthDiff);
                player.movementCounter = Vector2.Zero;
                if (!AquaMaths.IsApproximateZero(movement.X))
                {
                    player.MoveH(movement.X);
                }
                if (!AquaMaths.IsApproximateZero(movement.Y))
                {
                    player.MoveV(movement.Y);
                }
                float afterLength = CalculateRopeLength(player.ExactCenter());
                if (afterLength - _lockLength <= 1.5f)  // 保证大于1.414即可
                {
                    return false;
                }
                return true;
            }
            else if (player.StateMachine.State == (int)AquaStates.StHanging && length < _lockLength - 1.5f)
            {
                float lengthDiff = length - _lockLength;
                Vector2 ropeDirection = Vector2.Normalize(BottomPivot.point - playerSeg.Point2);
                Vector2 movement = ropeDirection * -MathF.Ceiling(-lengthDiff);
                player.movementCounter = Vector2.Zero;
                if (!AquaMaths.IsApproximateZero(movement.X))
                {
                    player.MoveH(movement.X);
                }
                if (!AquaMaths.IsApproximateZero(movement.Y))
                {
                    player.MoveV(movement.Y);
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

        public void AddLockedLength(float diff)
        {
            _lockLength = Calc.Clamp(_lockLength + diff, 4.0f, MaxLength);
        }

        public bool ReachLockedLength(Vector2 playerPosition)
        {
            float currentLength = CalculateRopeLength(playerPosition);
            return currentLength >= _lockLength - 1.0f;
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
            _lockLength = MaxLength;
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            Player player = Scene.Tracker.GetEntity<Player>();
            RopePivot pivot = _pivots[0];
            pivot.point = hook.Position;
            _pivots[0] = pivot;
            _prevLength = CalculateRopeLength(player.ExactCenter());
        }

        public override void Render()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            _segments.Clear();
            for (int i = 0; i < _pivots.Count; ++i)
            {
                Vector2 pt1 = _pivots[i].point;
                Vector2 pt2 = i == _pivots.Count - 1 ? player.ExactCenter() : _pivots[i + 1].point;
                _segments.Add(new Segment(pt1, pt2));
            }
            _renderer.Render(_segments);
            //Color lineColor = Color.White;
            //for (int i = 0; i < _pivots.Count; i++)
            //{
            //    if (i == _pivots.Count - 1)
            //    {
            //        Player player = Scene.Tracker.GetEntity<Player>();
            //        if (player == null) return;
            //        Draw.Line(_pivots[i].point, player.Center, lineColor);
            //    }
            //    else
            //    {
            //        Draw.Line(_pivots[i].point, _pivots[i + 1].point, lineColor);
            //    }
            //}
        }

        private void UpdatePivots(Segment playerSeg)
        {
            WallBooster belt = CheckCollideWallBooster();
            for (int i = 0; i < _pivots.Count;)
            {
                RopePivot pivot = _pivots[i];
                Platform solid = pivot.entity as Platform;
                if (solid != null)
                {
                    if (i != 0 && (solid.Collider == null || !solid.Collidable))
                    {
                        _pivots.RemoveAt(i);
                        _lastPivots.Increase(i);
                        AquaDebugger.LogInfo("Remove Pivot {0}", pivot);
                    }
                    else if (i == 0 && belt != null)
                    {
                        GrapplingHook hook = Entity as GrapplingHook;
                        Hitbox collider = belt.Collider as Hitbox;
                        Vector2 pt = pivot.point;
                        if (belt.IceMode)
                        {
                            if (hook.AlongRopeSpeed > 0.0f)
                            {
                                float speedY = Vector2.Dot(-CurrentDirection * hook.AlongRopeSpeed, Vector2.UnitY) * 2.0f;
                                pt.Y = Calc.Clamp(pt.Y + speedY * Engine.DeltaTime, collider.AbsoluteTop, collider.AbsoluteBottom);
                            }
                        }
                        else
                        {
                            pt.Y = Calc.Clamp(pt.Y + Player.WallBoosterSpeed * Engine.DeltaTime, collider.AbsoluteTop, collider.AbsoluteBottom);
                        }
                        pivot.point = hook.Position = pt;
                        _pivots[i] = pivot;
                        _lastPivots.Increase(i);
                        ++i;
                    }
                    else
                    {
                        Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
                        pivot.point += moveVec;
                        _pivots[i] = pivot;
                        _lastPivots.Increase(i);
                        if (i == 0)
                        {
                            Entity.Position = pivot.point;
                        }
                        ++i;
                    }
                }
                else
                {
                    _lastPivots.Increase(i);
                    ++i;
                }
            }

            for (int i = 1; i < _pivots.Count;)
            {
                RopePivot pivot = _pivots[i];
                Vector2 curPt = pivot.point;
                Vector2 curLastPt = _lastPivots.GetPivotLast(i).point;
                Vector2 prevPtNew = _pivots[i - 1].point;
                Vector2 nextPtNew = i == _pivots.Count - 1 ? playerSeg.Point2 : _pivots[i + 1].point;
                Vector2 prevPtOld = _lastPivots.GetPivotLast(i - 1).point;
                Vector2 nextPtOld = i == _pivots.Count - 1 ? playerSeg.Point1 : _lastPivots.GetPivotLast(i + 1).point;
                Segment prevBound = new Segment(prevPtOld, nextPtOld);
                Segment curBound = new Segment(prevPtNew, nextPtNew);
                if (WillReleasePivot(curLastPt, pivot, prevBound, curBound))
                {
                    _pivots.RemoveAt(i);
                    _lastPivots.Increase(i - 1);
                    AquaDebugger.LogInfo("Remove Pivot {0}", curPt);
                }
                else
                {
                    ++i;
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
                if (!solid.Collidable || solid.Collider == null) continue;
                CheckCollisionSolid(prevPivot, currentPivot, lastSegments, solid, potentials);
            }
            List<Entity> jumps = Scene.Tracker.GetEntities<JumpThru>();
            foreach (JumpThru jump in jumps)
            {
                if (!jump.Collidable || jump.Collider == null) continue;
                CheckCollisionJumpThru(prevPivot, currentPivot, lastSegments, jump, potentials);
            }
            List<Bumper> bumpers = Scene.Entities.FindAll<Bumper>();
            foreach (Bumper bumper in bumpers)
            {
                if (!bumper.Collidable || bumper.Collider == null) continue;
                CheckCollisionBumper(prevPivot, currentPivot, lastSegments, bumper, potentials);
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
            Segment curRopeSeg = new Segment(prevPivot.point, currentPivot.point);
            Vector2 moveVec = solid.Position - solid.GetPreviousPosition();
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

        private void CheckCollisionJumpThru(RopePivot prevPivot, RopePivot currentPivot, IList<Segment> lastSegments, JumpThru jump, SortedSet<PotentialPoint> potentials)
        {
            Segment curRopeSeg = new Segment(prevPivot.point, currentPivot.point);
            Collider collider = jump.Collider;
            if (collider is Hitbox box)
            {
                Vector2 tl = new Vector2(box.AbsoluteLeft, box.AbsoluteTop);
                Vector2 tr = new Vector2(box.AbsoluteRight, box.AbsoluteTop);
                if (prevPivot.point.X >= tl.X && prevPivot.point.X <= tr.X)
                    return;
                Vector2 alongJump = tr - tl;
                float crossJump = AquaMaths.Cross(alongJump, curRopeSeg.Vector);
                if (MathF.Sign(crossJump) < 0)
                    return;
                unsafe
                {
                    Vector2* pts = stackalloc Vector2[2] { tl, tr };
                    Cornors* cornors = stackalloc Cornors[2] { Cornors.TopLeft, Cornors.TopRight };
                    for (int i = 0; i < 2; ++i)
                    {
                        TryAddPotentialPoint(pts[i], cornors[i], prevPivot, currentPivot, jump, Vector2.Zero, lastSegments, curRopeSeg, potentials);
                    }
                }
            }
        }

        private void CheckCollisionBumper(RopePivot prevPivot, RopePivot currentPivot, IList<Segment> lastSegments, Bumper bumper, SortedSet<PotentialPoint> potentials)
        {
            Segment curRopeSeg = new Segment(prevPivot.point, currentPivot.point);
            Collider collider = bumper.Collider;
            if (collider is Circle circle)
            {
                List<RopePivot> pivots = circle.SimulatePivots();
                for (int i = 0; i <  pivots.Count; ++i)
                {
                    RopePivot pivot = pivots[i];
                    pivot.point += bumper.Position;
                    TryAddPotentialPoint(pivot.point, pivot.direction, prevPivot, currentPivot, bumper, Vector2.Zero, lastSegments, curRopeSeg, potentials);
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
            Vector2 newToPrev = currentRopeSeg.Point1 - currentPt;
            Vector2 newToNext = currentRopeSeg.Point2 - currentPt;
            float crossNew = AquaMaths.Cross(newToPrev, newToNext);
            int signNew = MathF.Sign(crossNew);
            if (signNew == 0) return false;
            foreach (Segment lastSeg in lastSegments)
            {
                Vector2 oldToPrev = lastSeg.Point1 - prevPt;
                Vector2 oldToNext = lastSeg.Point2 - prevPt;
                float alongValue = Vector2.Dot(-oldToPrev, lastSeg.Direction);
                if (!AquaMaths.IsApproximateZero(oldToPrev) && !AquaMaths.IsApproximateZero(oldToNext) && alongValue > 0.0f && alongValue < lastSeg.Length)
                {
                    float crossOld = AquaMaths.Cross(oldToPrev, oldToNext);
                    int signOld = MathF.Sign(crossOld);
                    if (signOld != signNew)
                    {
                        if (signOld == 0)
                        {
                            Vector2 movement = currentPt - prevPt;
                            if (!AquaMaths.IsApproximateZero(movement))
                            {
                                return IsMoveDirectionCorrect(movement, pivot.direction);
                            }
                            // 不知道这里这样处理行不行，主要是为了防止两个挂点属于同一个Entity
                            return Collide.CheckLine(pivot.entity, currentRopeSeg.Point1 + currentRopeSeg.Direction * 0.01f, currentRopeSeg.Point2);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool WillReleasePivot(Vector2 prevPt, RopePivot pivot, Segment prevBound, Segment currentBound)
        {
            Vector2 currentPt = pivot.point;
            Vector2 oldToPrev = prevPt - prevBound.Point1;
            Vector2 newToPrev = currentPt - currentBound.Point1;
            float crossOld = AquaMaths.Cross(oldToPrev, prevBound.Vector);
            float crossNew = AquaMaths.Cross(newToPrev, currentBound.Vector);
            int signOld = MathF.Sign(crossOld);
            int signNew = MathF.Sign(crossNew);
            if (signNew == 0) return true;
            if (signOld != signNew)
            {
                //if (signOld == 0)
                //{
                //    // 在绕圈的时候可能出现这种特殊情况
                //    Vector2 movement = currentPt - prevPt;
                //    if (!AquaMaths.IsApproximateZero(movement))
                //    {
                //        return !IsMoveDirectionCorrect(movement, pivot.direction);
                //    }
                //    return !Collide.CheckLine(pivot.entity, currentBound.Point1 + currentBound
                //        .Direction * 0.01f, currentPt);
                //}
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

        private Vector2 TotalMovementRelativeToPivot(Vector2 pt, Segment ropeSeg, Vector2 prevPivotMovement, Vector2 prevRopeVec, Vector2 nextPivotMovement, Vector2 nextRopeVec)
        {
            Vector2 relativePrev = MovementRelativeToPivot(prevPivotMovement, prevRopeVec, pt - ropeSeg.Point1);
            Vector2 relativeNext = MovementRelativeToPivot(nextPivotMovement, nextRopeVec, pt - ropeSeg.Point2);
            return relativePrev + relativeNext;
        }

        private Vector2 MovementRelativeToPivot(Vector2 pivotMovement, Vector2 ropeVec, Vector2 relativeVec)
        {
            if (AquaMaths.IsApproximateZero(AquaMaths.Cross(pivotMovement, ropeVec)))
                return Vector2.Zero;
            pivotMovement = Vector2.Normalize(pivotMovement);
            ropeVec = Vector2.Normalize(ropeVec);
            Matrix mat = AquaMaths.BuildMatrix(pivotMovement, ropeVec);
            mat = Matrix.Invert(mat);
            Vector4 v = AquaMaths.Matrix4Multiply(ref mat, new Vector4(relativeVec, 0.0f, 0.0f));
            return v.X * pivotMovement;
        }

        private float CalculateRopeLength(Vector2 playerPosition)
        {
            float length = 0.0f;
            if (_pivots.Count <= 1)
            {
                return (BottomPivot.point - playerPosition).Length();
            }

            for (int i = 1; i < _pivots.Count; ++i)
            {
                Segment seg = new Segment(_pivots[i].point, _pivots[i - 1].point);
                length += seg.Length;
            }
            length += (BottomPivot.point - playerPosition).Length();
            return length;
        }

        private List<RopePivot> _pivots = new List<RopePivot>(8);
        private ExpiredPivotList _lastPivots = new ExpiredPivotList();
        internal float _prevLength;
        private float _lockLength;

        private RopeRenderer _renderer;
        private List<Segment> _segments = new List<Segment>(8);
    }
}
