﻿using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public class GrapplingHook : Entity
    {
        public enum HookStates : byte
        {
            None = 0,
            Emitting,
            Revoking,
            Fixed,
        }

        public float HookSize { get; private set; }   // 爪子的边长，碰撞箱是正方形

        public HookStates State { get; private set; } = HookStates.None;
        public Vector2 Velocity { get; private set; }
        public bool Revoked { get; private set; } = false;
        public bool JustFixed { get; private set; } = false;
        public float AlongRopeSpeed { get; set; } = 0.0f;

        public float LockedRadius => Get<HookRope>().LockedLength;
        public float SwingRadius => Get<HookRope>().SwingRadius;
        public Vector2 RopeDirection => Get<HookRope>().RopeDirection;
        public Vector2 SwingDirection => Get<HookRope>().SwingDirection;
        public Vector2 PlayerPreviousPosition => _playerPrevPosition;

        public GrapplingHook(float size, float length)
            : base(Vector2.Zero)
        {
            HookSize = size;
            Active = false;

            Collider = new Hitbox(size, size, -size * 0.5f, -size * 0.5f);

            Add(new HookRope(length));
            Add(_sprite = new HookSprite());
            AddTag(Tags.Global);
        }

        public void Emit(Vector2 direction, float speed)
        {
            HookRope rope = Get<HookRope>();
            rope.CurrentDirection = direction;
            rope.EmitSpeed = speed;
            Revoked = false;

            _sprite.Play(HookSprite.Emit, true);
            _sprite.Rotation = direction.Angle();
        }

        public void Revoke()
        {
            State = HookStates.Revoking;
            HookRope rope = Get<HookRope>();
            Entity attachedEntity = rope.TopPivot.entity;
            if (attachedEntity != null)
            {
                attachedEntity.SetHookAttached(false);
            }
            rope.HookAttachEntity(null);

            _sprite.Play(HookSprite.Revoke, true);
        }

        public void Fix()
        {
            State = HookStates.Fixed;
            JustFixed = true;
            _fixElapsed = _elapsed;

            _sprite.Play(HookSprite.Hit, true);
        }

        public void SetRopeLengthLocked(bool locked, Vector2 playerPosition)
        {
            if (locked && _lengthLocked == locked) return;
            _lengthLocked = locked;
            HookRope rope = Get<HookRope>();
            rope.SetLengthLocked(locked, playerPosition);
        }

        public bool ReachLockedLength(Vector2 playerPosition)
        {
            HookRope rope = Get<HookRope>();
            return rope.ReachLockedLength(playerPosition);
        }

        public bool EnforcePlayer(Player player, Segment playerSeg, float dt)
        {
            HookRope rope = Get<HookRope>();
            return rope.EnforcePlayer(player, playerSeg, dt);
        }

        public void RecordEmitElapsed()
        {
            _lastEmitElapsed = _elapsed;
        }

        public bool CanFlyToward()
        {
            float range = AquaModule.Settings.HookSettings.HookFlyTowardDuration;
            if (JustFixed)
            {
                if (_elapsed - _lastEmitElapsed <= range)
                {
                    _lastEmitElapsed = float.MinValue;
                    return true;
                }
            }
            else
            {
                if (_elapsed - _fixElapsed <= range)
                {
                    _fixElapsed = float.MinValue;
                    return true;
                }
            }
            return false;
        }

        public bool IsRopeIntersectsWith(Entity entity)
        {
            if (entity.Collider == null)
            {
                return false;
            }

            HookRope rope = Get<HookRope>();
            IReadOnlyList<RopePivot> pivots = rope.AllPivots;
            for (int i = 0; i < pivots.Count - 1; i++)
            {
                Vector2 pt1 = pivots[i].point;
                Vector2 pt2 = pivots[i + 1].point;
                if (entity.Collider.Collide(pt1, pt2))
                {
                    return true;
                }
            }
            Player player = Scene.Tracker.GetEntity<Player>();
            if (entity.Collider.Collide(pivots[pivots.Count - 1].point, player.ExactCenter()))
            {
                return true;
            }
            return false;
        }

        public override void Added(Scene scene)
        {
            Player madeline = scene.Tracker.GetEntity<Player>();
            Position = madeline.Center;
            State = HookStates.Emitting;
            Active = true;
            HookRope rope = Get<HookRope>();
            rope.Active = rope.Visible = true;

            base.Added(scene);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);

            State = HookStates.None;
            Active = false;
            JustFixed = false;
            HookRope rope = Get<HookRope>();
            rope.Active = rope.Visible = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Velocity = Vector2.Zero;
            Player player = Scene.Tracker.GetEntity<Player>();
            _playerPrevPosition = player.ExactCenter();
            _elapsed = 0.0f;
            _lastEmitElapsed = float.MinValue;
            _fixElapsed = float.MinValue;
        }

        public override void Update()
        {
            float dt = Engine.DeltaTime;
            _elapsed += dt;
            JustFixed = false;
            HookRope rope = Get<HookRope>();
            Player player = Scene.Tracker.GetEntity<Player>();
            Segment playerSeg = new Segment(_playerPrevPosition, player.ExactCenter());
            Vector2 prevPosition = Position;
            Vector2 nextPosition = Position;
            Entity attachEntity = rope.TopPivot.entity;
            if (attachEntity != null && (!attachEntity.Collidable || attachEntity.Collider == null || !attachEntity.IsHookable()))
            {
                Revoke();
                rope.CheckCollision(playerSeg);
                rope.UpdateCurrentDirection();
            }
            else
            {
                switch (State)
                {
                    case HookStates.Emitting:
                        rope.CheckCollision(playerSeg);
                        Velocity = Position - prevPosition;
                        bool changeState;
                        nextPosition = rope.DetectHookNextPosition(dt, false, out changeState);
                        Vector2 movement = nextPosition - prevPosition;
                        Velocity += movement;
                        bool collided = false;
                        if (!AquaMaths.IsApproximateZero(movement.X))
                        {
                            collided = collided || MoveH(movement.X, OnCollideEntity);
                        }
                        if (!AquaMaths.IsApproximateZero(movement.Y))
                        {
                            collided = collided || MoveV(movement.Y, OnCollideEntity);
                        }
                        if (collided && !_hitUnhookable)
                        {
                            Fix();
                        }
                        else if (changeState || _hitUnhookable)
                        {
                            Revoke();
                        }
                        break;
                    case HookStates.Revoking:
                        rope.CheckCollision(playerSeg);
                        Velocity = Position - prevPosition;
                        bool revokeHook;
                        nextPosition = rope.DetectHookNextPosition(dt, true, out revokeHook);
                        Velocity += nextPosition - prevPosition;
                        Revoked = revokeHook;
                        Position = nextPosition;
                        break;
                    case HookStates.Fixed:
                        if (WillHitSolids())
                        {
                            Revoke();
                        }
                        rope.CheckCollision(playerSeg);
                        Velocity = Position - prevPosition;
                        rope.UpdateCurrentDirection();
                        break;
                    default:
                        break;
                }
            }

            Velocity /= dt;
            _playerPrevPosition = player.ExactCenter();
            CheckInteractables();
            base.Update();
            if (State == HookStates.Emitting || State == HookStates.Revoking)
            {
                _sprite.Rotation = rope.CurrentDirection.Angle();
            }
        }

        private void CheckInteractables()
        {
            foreach (HookCollider com in Scene.Tracker.GetComponents<HookCollider>())
            {
                com.Check(this);
            }
        }

        private void OnCollideEntity(CollisionData collisionData)
        {
            HookRope rope = Get<HookRope>();
            Entity hitEntity = collisionData.Hit;
            if (hitEntity.IsHookable())
            {
                rope.HookAttachEntity(hitEntity);
                hitEntity.SetHookAttached(true);
                _hitUnhookable = false;
            }
            else
            {
                _hitUnhookable = true;
            }
        }

        private bool WillHitSolids()
        {
            HookRope rope = Get<HookRope>();
            List<Entity> solids = Scene.Tracker.GetEntities<Solid>();
            foreach (Entity entity in solids)
            {
                if (entity != rope.TopPivot.entity && entity.Collider != null && entity.Collidable)
                {
                    if (entity.CollideCheck(this))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool MoveH(float moveH, Collision onCollide = null)
        {
            _movementCounter.X += moveH;
            int num = (int)Math.Round(_movementCounter.X, MidpointRounding.ToEven);
            if (num != 0)
            {
                _movementCounter.X -= num;
                return MoveHExact(num, onCollide);
            }

            return false;
        }

        private bool MoveV(float moveV, Collision onCollide = null)
        {
            _movementCounter.Y += moveV;
            int num = (int)Math.Round(_movementCounter.Y, MidpointRounding.ToEven);
            if (num != 0)
            {
                _movementCounter.Y -= num;
                return MoveVExact(num, onCollide);
            }

            return false;
        }

        private bool MoveHExact(int moveH, Collision onCollide = null)
        {
            Vector2 targetPosition = Position + Vector2.UnitX * moveH;
            int num = Math.Sign(moveH);
            int num2 = 0;
            while (moveH != 0)
            {
                Solid solid = CollideFirst<Solid>(Position + Vector2.UnitX * num);
                if (solid != null)
                {
                    _movementCounter.X = 0f;
                    onCollide?.Invoke(new CollisionData
                    {
                        Direction = Vector2.UnitX * num,
                        Moved = Vector2.UnitX * num2,
                        TargetPosition = targetPosition,
                        Hit = solid,
                        Pusher = null,
                    });
                    return true;
                }

                num2 += num;
                moveH -= num;
                base.X += num;
            }

            return false;
        }

        private bool MoveVExact(int moveV, Collision onCollide = null)
        {
            Vector2 targetPosition = Position + Vector2.UnitY * moveV;
            int num = Math.Sign(moveV);
            int num2 = 0;
            while (moveV != 0)
            {
                Platform platform = CollideFirst<Solid>(Position + Vector2.UnitY * num);
                CollisionData data;
                if (platform != null)
                {
                    _movementCounter.Y = 0f;
                    if (onCollide != null)
                    {
                        data = new CollisionData
                        {
                            Direction = Vector2.UnitY * num,
                            Moved = Vector2.UnitY * num2,
                            TargetPosition = targetPosition,
                            Hit = platform,
                            Pusher = null,
                        };
                        onCollide(data);
                    }

                    return true;
                }

                if (moveV > 0)
                {
                    platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * num);
                    if (platform != null)
                    {
                        _movementCounter.Y = 0f;
                        if (onCollide != null)
                        {
                            data = new CollisionData
                            {
                                Direction = Vector2.UnitY * num,
                                Moved = Vector2.UnitY * num2,
                                TargetPosition = targetPosition,
                                Hit = platform,
                                Pusher = null,
                            };
                            onCollide(data);
                        }

                        return true;
                    }
                }

                num2 += num;
                moveV -= num;
                base.Y += num;
            }

            return false;
        }

        private Vector2 _movementCounter = Vector2.Zero;
        private Vector2 _playerPrevPosition;    // Player的PreviousPosition貌似在相对速度为0的情况下和当前Position相同
        private bool _lengthLocked = false;
        private float _elapsed = 0.0f;
        private float _lastEmitElapsed;
        private float _fixElapsed;
        private bool _hitUnhookable = false;

        private HookSprite _sprite;
    }
}