﻿using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Module;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class GrapplingHook : Entity
    {
        private struct CustomCollisionData
        {
            public Vector2 Direction;
            public Entity Hit;
        }

        private struct MoveResult
        {
            public bool Hit;
            public bool Moved;
        }

        public enum HookStates : byte
        {
            None = 0,
            Emitting,
            Revoking,
            Bouncing,
            Fixed,
            Attracted,
        }

        public enum RopeMaterial
        {
            Default,
            Metal,
        }

        public enum GameplayMode
        {
            Default,
            ShootCounter,
        }

        public const float HOOK_SIZE = 8.0f;
        public const float BOUNCE_SPEED_ADDITION = 300.0f;

        public Player Owner { get; set; }
        public float HookSize { get; private set; }
        public HookSprite Sprite => _sprite;
        public bool ElectricShocking { get; set; } = false;
        public RopeMaterial Material
        {
            get => _material;
            set
            {
                if (_material != value)
                {
                    Get<HookRope>().ChangeMaterial(value);
                    _material = value;
                }
            }
        }

        public GameplayMode Mode
        {
            get => _mode;
        }
        public HookStates State { get; private set; } = HookStates.None;
        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public bool Revoked { get; private set; } = false;
        public bool JustFixed { get; private set; } = false;
        public float AlongRopeSpeed { get; set; } = 0.0f;
        public Vector2 BouncingVelocity { get; set; }
        public float EmitSpeedCoefficient { get; private set; }
        public float EmitSpeedMultiplier { get; set; } = 1.0f;
        public Entity AttachedEntity => Get<HookRope>().TopPivot.entity;
        public GrappleAttractor CurrentAttractor { get; set; }

        public float MaxLength => Get<HookRope>().MaxLength;
        public float LockedRadius => Get<HookRope>().LockedLength;
        public float SwingRadius => Get<HookRope>().SwingRadius;
        public Vector2 HookDirection => Get<HookRope>().HookDirection;
        public Vector2 RopeDirection => Get<HookRope>().RopeDirection;
        public Vector2 SwingDirection => Get<HookRope>().SwingDirection;
        public Vector2 PlayerPreviousPosition => _playerPrevPosition;
        public Vector2 ShootDirection => Get<HookRope>().CurrentDirection;

        // Misc stuffs
        public float UserLockedLength { get; set; }

        public GrapplingHook(float size, float length, RopeMaterial material = RopeMaterial.Default)
            : base(Vector2.Zero)
        {
            HookSize = size;
            _material = material;
            Active = false;

            Collider = new Hitbox(size, size, -size * 0.5f, -size * 0.5f);

            Add(new HookRope(length, material));
            Add(_sprite = new HookSprite(HookSprite.HookSpriteMode.Default));
            Add(_elecShockSprite = new Sprite());
            GFX.SpriteBank.CreateOn(_elecShockSprite, "Aqua_HookElectricShock");
            AddTag(Tags.Global);

            this.MakeExtraCollideCondition();
        }

        public void ChangeGameplayMode(GameplayMode mode, Level level, int initialCounter)
        {
            _mode = mode;
            switch (_mode)
            {
                case GameplayMode.Default:
                    level.GetState().RestShootCount = int.MaxValue;
                    break;
                case GameplayMode.ShootCounter:
                    level.GetState().RestShootCount = initialCounter;
                    break;
            }
        }

        public void SetRopeLength(float length)
        {
            HookRope rope = Get<HookRope>();
            rope.ChangeLength(length);
        }

        public void SetStyle(int style)
        {
            if (style >= 1 && style <= 4)
            {
                string sprite = "Aqua_HookStyle" + style.ToString();
                _sprite.ChangeStyle(sprite);
            }
            else
            {
                _sprite.ChangeStyle("Aqua_Hook");
            }
        }

        public bool CanCollide(Entity other)
        {
            return other.IsHookable();
        }

        public void SetPositionRounded(Vector2 position, bool updatePivot = true)
        {
            Position = AquaMaths.Round(position);
            HookRope rope = Get<HookRope>();
            if (rope != null && updatePivot)
            {
                rope.UpdateTopPivot(Position);
            }
        }

        public bool CanEmit(Level level)
        {
            switch (Mode)
            {
                case GameplayMode.Default:
                default:
                    return true;
                case GameplayMode.ShootCounter:
                    return level.GetState().RestShootCount > 0;
            }
        }

        public void Emit(Level level, Vector2 direction, float speed, float speedCoeff)
        {
            AlongRopeSpeed = 0.0f;
            HookRope rope = Get<HookRope>();
            rope.CurrentDirection = direction;
            rope.EmitSpeed = speed;
            EmitSpeedCoefficient = speedCoeff;
            EmitSpeedMultiplier = 1.0f;
            Revoked = false;
            if (Mode == GameplayMode.ShootCounter)
            {
                level.GetState().RestShootCount = Math.Max(level.GetState().RestShootCount - 1, 0);
            }

            _sprite.Play(HookSprite.Emit, true);
            _sprite.Rotation = _elecShockSprite.Rotation = direction.Angle();
        }

        public void Revoke()
        {
            State = HookStates.Revoking;
            HookRope rope = Get<HookRope>();
            Entity attachedEntity = rope.TopPivot.entity;
            if (attachedEntity != null)
            {
                attachedEntity.SetHookAttached(false, this);
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
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        public void AttractTo(GrappleAttractor attractor)
        {
            State = HookStates.Attracted;
            CurrentAttractor = attractor;
        }

        public void CancelAttraction()
        {
            State = HookStates.Emitting;
            CurrentAttractor = null;
        }

        public bool Bounce(Vector2 axis, float speed)
        {
            axis = Calc.SafeNormalize(axis);
            HookRope rope = Get<HookRope>();
            BouncingVelocity = State == HookStates.Bouncing ? BouncingVelocity : rope.CurrentDirection * rope.EmitSpeed * (1.0f + EmitSpeedCoefficient);
            float proj = Vector2.Dot(BouncingVelocity, axis);
            if (proj > 0.0f)
                return false;

            State = HookStates.Bouncing;
            if (AquaMaths.IsApproximateZero(proj))
            {
                BouncingVelocity += speed * axis;
            }
            else
            {
                Vector2 doubleAxis = axis * MathF.Abs(proj) * 2.0f;
                BouncingVelocity += doubleAxis;
            }
            BouncingVelocity = Calc.SafeNormalize(BouncingVelocity) * rope.EmitSpeed * (1.0f + EmitSpeedCoefficient);
            return true;
        }

        public void BounceTo(Vector2 direction)
        {
            HookRope rope = Get<HookRope>();
            BouncingVelocity = direction * rope.EmitSpeed * (1.0f + EmitSpeedCoefficient);
            State = HookStates.Bouncing;
        }

        public void SetHookVisible(bool visible)
        {
            _sprite.Visible = visible;
        }

        public float CalculateRopeLength(Vector2 playerPosition)
        {
            HookRope rope = Get<HookRope>();
            return rope.CalculateRopeLength(playerPosition);
        }

        public void SetRopeLengthLocked(bool locked, Vector2 playerPosition)
        {
            _lengthLocked = locked;
            HookRope rope = Get<HookRope>();
            rope.SetLengthLocked(locked, playerPosition);
        }

        public void SetLockedLengthDirectly(float length)
        {
            _lengthLocked = true;
            HookRope rope = Get<HookRope>();
            rope.SetLengthLocked(true, length);
        }

        public void AddLockedRopeLength(float diff)
        {
            if (_lengthLocked)
            {
                HookRope rope = Get<HookRope>();
                rope.AddLockedLength(diff);
            }
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

        public bool CanGrappleBoost()
        {
            if (JustFixed)
            {
                if (_elapsed - _lastEmitElapsed <= 0.2f)
                {
                    return true;
                }
            }
            else
            {
                if (_elapsed - _fixElapsed <= 0.15f)
                {
                    return true;
                }
            }
            return false;
        }

        public void ResetFlyTowardThings()
        {
            _lastEmitElapsed = float.MinValue;
            _fixElapsed = float.MinValue;
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
            Player player = Owner;
            if (entity.Collider.Collide(pivots[pivots.Count - 1].point, player.ExactCenter()))
            {
                return true;
            }
            return false;
        }

        public override void Added(Scene scene)
        {
            Player madeline = Owner;
            Position = _prevPosition = AquaMaths.Round(madeline.ExactCenter());
            State = HookStates.Emitting;
            Active = true;
            HookRope rope = Get<HookRope>();
            base.Added(scene);
            // Collision check
            UpdateColliderOffset(-rope.CurrentDirection);
            Vector2 rounded = AquaMaths.Round(rope.CurrentDirection);
            CheckCollisionWhileShooting(rounded);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            State = HookStates.None;
            Velocity = Acceleration = Vector2.Zero;
            Active = false;
            JustFixed = false;
            UserLockedLength = 0.0f;
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            State = HookStates.None;
            Velocity = Acceleration = Vector2.Zero;
            Active = false;
            JustFixed = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            Player player = Owner;
            _playerPrevPosition = player.ExactCenter();
            _elapsed = 0.0f;
            _lastEmitElapsed = float.MinValue;
            _fixElapsed = float.MinValue;
        }

        public override void Update()
        {
            HookRope rope = Get<HookRope>();
            Player player = Owner;
            float dt = Engine.DeltaTime;
            _elapsed += dt;
            JustFixed = false;
            _elecShockSprite.Visible = ElectricShocking;
            if (player.StateMachine.State == (int)AquaStates.StElectricShock)
            {
                _elecShockSprite.Visible = true;
                rope.ElectricShocking = true;
                base.Update();
                return;
            }

            UpdateColliderOffset(-rope.CurrentDirection);
            Segment playerSeg = new Segment(_playerPrevPosition, player.ExactCenter());
            Vector2 prevPosition = _prevPosition;
            Vector2 nextPosition = Position;
            Vector2 lastVelocity = Velocity;
            HookStates lastState = State;
            Entity attachEntity = rope.TopPivot.entity;
            if (attachEntity != null && (!attachEntity.Collidable || attachEntity.Collider == null || !attachEntity.IsHookable()))
            {
                Revoke();
                rope.CheckCollision(playerSeg);
                rope.UpdateCurrentDirection();
            }
            else
            {
                bool collided = false;
                bool changeState = false;
                Vector2 movement = Vector2.Zero;
                switch (State)
                {
                    case HookStates.Emitting:
                        nextPosition = rope.DetectHookNextPosition(dt, false, CalculateSpeedCoefficient(false), out changeState);
                        movement = nextPosition - prevPosition;
                        collided = BresenhamMove(movement, OnCollideEntity);
                        rope.UpdateTopPivot(Position);
                        rope.CheckCollision(playerSeg);
                        if (collided && _hitInteractable)
                        {
                            break;
                        }
                        if (collided && !_hitUnhookable)
                        {
                            Fix();
                        }
                        else if (changeState)
                        {
                            Revoke();
                        }
                        break;
                    case HookStates.Revoking:
                        rope.CheckCollision(playerSeg);
                        bool revokeHook;
                        nextPosition = rope.DetectHookNextPosition(dt, true, CalculateSpeedCoefficient(true), out revokeHook);
                        Revoked = revokeHook;
                        Position = nextPosition;
                        break;
                    case HookStates.Bouncing:
                        movement = BouncingVelocity * dt;
                        float currentLength = rope._prevLength + movement.Length();
                        if (currentLength > rope.MaxLength)
                        {
                            movement = Calc.SafeNormalize(movement) * (movement.Length() - (currentLength - rope.MaxLength));
                            changeState = true;
                        }
                        collided = BresenhamMove(movement, OnCollideEntity);
                        rope.UpdateTopPivot(Position);
                        rope.CheckCollision(playerSeg);
                        if (collided && _hitInteractable)
                        {
                            break;
                        }
                        if (collided && !_hitUnhookable)
                        {
                            Fix();
                        }
                        else if (changeState || _hitUnhookable)
                        {
                            Revoke();
                        }
                        rope.UpdateCurrentDirection();
                        break;
                    case HookStates.Attracted:
                        float speedCoeff = CalculateSpeedCoefficient(false);
                        float distance = (CurrentAttractor.AttractionTarget - Position).Length();
                        if (distance > CurrentAttractor.MaxRange)
                        {
                            speedCoeff *= 0.5f;
                        }
                        else if (distance < CurrentAttractor.MinRange)
                        {
                            speedCoeff *= 1.0f;
                        }
                        else
                        {
                            float coeff = MathHelper.Clamp((distance - CurrentAttractor.MinRange) / (CurrentAttractor.MaxRange - CurrentAttractor.MinRange), 0.0f, 1.0f);
                            coeff = MathHelper.Lerp(1.0f, 0.5f, coeff);
                            speedCoeff *= coeff;
                        }
                        float shootSpeed = rope.EmitSpeed * speedCoeff;
                        float t = (CurrentAttractor.AttractionTarget - Position).Length() / (shootSpeed * dt);
                        nextPosition = Position + Calc.SafeNormalize(CurrentAttractor.AttractionTarget - Position) * MathHelper.Clamp(t, 0.0f, 1.0f) * shootSpeed * dt;
                        movement = nextPosition - prevPosition;
                        collided = BresenhamMove(movement, OnCollideEntity);
                        rope.UpdateTopPivot(Position);
                        rope.CheckCollision(playerSeg);
                        if (collided && _hitInteractable)
                        {
                            break;
                        }
                        if (collided && !_hitUnhookable)
                        {
                            Fix();
                        }
                        else if (t <= 1.0f)
                        {
                            SetPositionRounded(CurrentAttractor.AttractionTarget);
                            rope.HookAttachEntity(CurrentAttractor);
                            CurrentAttractor.SetHookAttached(true, this);
                            Fix();
                        }
                        break;
                    case HookStates.Fixed:
                        if (WillHitSolids())
                        {
                            Revoke();
                        }
                        movement = nextPosition - prevPosition;
                        if (!AquaMaths.IsApproximateZero(movement))
                        {
                            Position = prevPosition;
                            BresenhamMove(movement);
                        }
                        rope.CheckCollision(playerSeg);
                        Velocity = Position - prevPosition;
                        rope.UpdateCurrentDirection();
                        break;
                    default:
                        break;
                }
            }

            _prevPosition = Position;
            _playerPrevPosition = player.ExactCenter();
            if (AquaMaths.IsApproximateZero(dt))
            {
                Velocity = Acceleration = Vector2.Zero;
            }
            else
            {
                Velocity /= dt;
                Acceleration = (Velocity - lastVelocity) / dt;
            }
            CheckHookColliders();
            base.Update();
            if (State == HookStates.Emitting || State == HookStates.Revoking)
            {
                _sprite.Rotation = rope.CurrentDirection.Angle();
            }
            else if (State == HookStates.Bouncing)
            {
                _sprite.Rotation = BouncingVelocity.Angle();
            }
        }

        private void CheckHookColliders()
        {
            foreach (HookCollider com in Scene.Tracker.GetComponents<HookCollider>())
            {
                com.Check(this);
            }
        }

        private void UpdateColliderOffset(Vector2 direction)
        {
            if (State == HookStates.Emitting || State == HookStates.Bouncing)
            {
                Hitbox box = Collider as Hitbox;
                Vector2 orig = new Vector2(-box.Width * 0.5f, -box.Height * 0.5f);
                float cosVal = MathF.Cos(MathF.PI * 0.125f);
                const float offset = 2.0f;
                foreach (Vector2 dir in OFFSET_DIRECTIONS)
                {
                    if (Vector2.Dot(direction, dir) >= cosVal)
                    {
                        box.Position = AquaMaths.Round(orig - dir * offset);
                        break;
                    }
                }
            }
        }

        private void OnCollideEntity(CustomCollisionData collisionData)
        {
            HookRope rope = Get<HookRope>();
            Entity hitEntity = collisionData.Hit;
            if (hitEntity.IsHookable())
            {
                rope.HookAttachEntity(hitEntity);
                hitEntity.SetHookAttached(true, this);
                PlayHitSound(hitEntity);
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

        private float CalculateSpeedCoefficient(bool revoking)
        {
            float coeff = revoking ? 1.0f - EmitSpeedCoefficient : 1.0f + EmitSpeedCoefficient;
            coeff *= EmitSpeedMultiplier;
            Water water = CollideFirst<Water>();
            if (water != null)
            {
                coeff *= 0.5f;
            }
            return coeff;
        }

        private bool BresenhamMove(Vector2 movement, Action<CustomCollisionData> onCollide = null)
        {
            _movementCounter = movement;
            int dx = (int)MathF.Round(_movementCounter.X, MidpointRounding.ToEven);
            int dy = (int)MathF.Round(_movementCounter.Y, MidpointRounding.ToEven);
            if (dx != 0 || dy != 0)
            {
                if (dx == 0)
                {
                    int step = MathF.Sign(dy);
                    while (dy != 0)
                    {
                        MoveResult result = StepMoveV(step, onCollide);
                        if (result.Hit)
                            return true;
                        dy -= step;
                        if (result.Moved)
                            Y += step;
                    }
                }
                else if (dy == 0)
                {
                    int step = MathF.Sign(dx);
                    while (dx != 0)
                    {
                        MoveResult result = StepMoveH(step, onCollide);
                        if (result.Hit)
                            return true;
                        dx -= step;
                        if (result.Moved)
                            X += step;
                    }
                }
                else
                {
                    int absY = Math.Abs(dy);
                    int absX = Math.Abs(dx);
                    bool swapped = false;
                    if (absY > absX)
                    {
                        int tmp = absX;
                        absX = absY;
                        absY = tmp;
                        swapped = true;
                    }

                    int stepX = MathF.Sign(dx);
                    int stepY = MathF.Sign(dy);
                    int error = absX - absY;
                    do
                    {
                        error += 2 * absY;
                        MoveResult result = new MoveResult();
                        if (error >= 2 * absX)
                        {
                            error -= 2 * absX;
                            if ((!swapped && (result = StepMoveV(stepY, onCollide)).Hit) || (swapped && (result = StepMoveH(stepX, onCollide)).Hit))
                            {
                                if (swapped)
                                    _movementCounter.X = 0.0f;
                                else
                                    _movementCounter.Y = 0.0f;
                                return true;
                            }
                            else
                            {
                                if (result.Moved)
                                {
                                    if (!swapped)
                                        Y += stepY;
                                    else
                                        X += stepX;
                                }
                            }
                        }
                        if ((!swapped && (result = StepMoveH(stepX, onCollide)).Hit) || (swapped && (result = StepMoveV(stepY, onCollide)).Hit))
                        {
                            if (swapped)
                                _movementCounter.Y = 0.0f;
                            else
                                _movementCounter.X = 0.0f;
                            return true;
                        }
                        else
                        {
                            if (result.Moved)
                            {
                                if (!swapped)
                                    X += stepX;
                                else
                                    Y += stepY;
                            }
                        }
                        absX--;
                    } while (absX != 0);
                }
            }
            return false;
        }

        private MoveResult StepMoveH(int step, Action<CustomCollisionData> onCollide)
        {
            if (CheckInteractables(Position + Vector2.UnitX * step))
            {
                return new MoveResult
                {
                    Hit = true,
                    Moved = false,
                };
            }

            Entity collideEntity = null;
            if (this.CheckCollidePlatformsAtXDirection(step, out collideEntity))
            {
                AquaDebugger.Assert(collideEntity != null, "Runtime Error");
                _movementCounter.X = 0f;
                CustomCollisionData data;
                return DoCollideOrNot(collideEntity, Vector2.UnitX * step, onCollide, out data);
            }

            _movementCounter.X -= step;
            return new MoveResult { Hit = false, Moved = true, };
        }

        private MoveResult StepMoveV(int step, Action<CustomCollisionData> onCollide)
        {
            if (CheckInteractables(Position + Vector2.UnitY * step))
            {
                return new MoveResult
                {
                    Hit = true,
                    Moved = false,
                };
            }

            Entity collideEntity = null;
            if (this.CheckCollidePlatformsAtYDirection(step, out collideEntity))
            {
                AquaDebugger.Assert(collideEntity != null, "Runtime Error");
                _movementCounter.X = 0f;
                CustomCollisionData data;
                return DoCollideOrNot(collideEntity, Vector2.UnitY * step, onCollide, out data);
            }

            _movementCounter.Y -= step;
            return new MoveResult { Hit = false, Moved = true, };
        }

        private MoveResult DoCollideOrNot(Entity entity, Vector2 direction, Action<CustomCollisionData> onCollide, out CustomCollisionData data)
        {
            data = new CustomCollisionData();
            Vector2 hookDir = Calc.AngleToVector(_sprite.Rotation, 1.0f);
            if (State == HookStates.Attracted || Vector2.Dot(hookDir, direction) > MathF.Cos(Calc.DegToRad * 89.9f))
            {
                if (onCollide != null)
                {
                    data = new CustomCollisionData
                    {
                        Direction = direction,
                        Hit = entity,
                    };
                    onCollide(data);
                }
                return new MoveResult
                {
                    Hit = true,
                    Moved = true,
                };
            }
            return new MoveResult
            {
                Hit = false,
                Moved = false,
            };
        }

        private bool CheckInteractables(Vector2 at)
        {
            _hitInteractable = false;
            List<Component> interactables = Scene.Tracker.GetComponents<HookInteractable>();
            for (int i = 0; i < interactables.Count; i++)
            {
                HookInteractable interactable = interactables[i] as HookInteractable;
                Entity entity = interactable.Entity;
                if (entity != null && interactable.CollideEntity(this, at))
                {
                    if (interactable.OnInteract(this, at))
                    {
                        _hitInteractable = true;
                        break;
                    }
                }
            }
            return _hitInteractable;
        }

        private void PlayHitSound(Entity entity)
        {
            int index = 0;
            if (entity is SolidTiles tiles)
            {
                index = FindHookingIndexOfSolidTiles(tiles);
            }
            else if (entity is Platform platform)
            {
                index = platform.GetLandSoundIndex(this);
            }
            else
            {
                var sidewaysJumpthruTypes = ModInterop.SidewaysJumpthruTypes;
                foreach (Type type in sidewaysJumpthruTypes)
                {
                    if (entity.GetType().IsAssignableTo(type))
                    {
                        FieldInfo field = type.GetField("surfaceIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (field != null)
                        {
                            index = (int)field.GetValue(entity);
                            break;
                        }
                    }
                }
            }
            Audio.Play(SurfaceIndex.GetPathFromIndex(index) + "/hooking", Position, "surface_index", index);
            AquaDebugger.LogInfo("Play Sound {0}", index);
        }

        private int FindHookingIndexOfSolidTiles(SolidTiles tiles)
        {
            float halfSize = MathF.Ceiling(HookSize * 0.5f + 0.5f);
            foreach (var dir in EIGHT_DIRECTIONS)
            {
                Vector2 pos = Center + dir * halfSize;
                int idx = tiles.SurfaceSoundIndexAt(pos);
                if (idx >= 0)
                    return idx;
            }
            return -1;
        }

        private void CheckCollisionWhileShooting(Vector2 rounded)
        {
            // Use some tricks here (Only y coordinate could cause issue while shoot at crouching state)
            bool collided = CollideFirst<Solid>() != null;
            const int OFFSET = 2;
            if (collided && rounded.Y != 0)
            {
                for (int i = 0; i < HookSize / 2 - 3 + OFFSET; i++)
                {
                    Position -= Vector2.UnitY * rounded.Y;
                    if (CollideFirst<Solid>() == null)
                    {
                        collided = false;
                        break;
                    }
                }
            }
            if (collided)
            {
                for (int i = 0; i < OFFSET; i++)
                {
                    Position -= Vector2.UnitX * rounded.X;
                    if (CollideFirst<Solid>() == null)
                    {
                        collided = false;
                        break;
                    }
                }
                if (collided)
                {
                    // Must be a crouching shoot.
                    Position += Vector2.UnitX * OFFSET * rounded.X;
                    Vector2 basePos = Position;
                    for (int i = 1; i <= HookSize / 2 - 1; i++)
                    {
                        Position = basePos - Vector2.UnitY * i;
                        if (CollideFirst<Solid>() == null)
                        {
                            collided = false;
                            break;
                        }
                        for (int j = 0; j < OFFSET; j++)
                        {
                            Position -= Vector2.UnitX * rounded.X;
                            if (CollideFirst<Solid>() == null)
                            {
                                collided = false;
                                break;
                            }
                        }
                        if (collided)
                        {
                            Position = basePos + Vector2.UnitY * i;
                            if (CollideFirst<Solid>() == null)
                            {
                                collided = false;
                                break;
                            }
                            for (int j = 0; j < OFFSET; j++)
                            {
                                Position -= Vector2.UnitX * rounded.X;
                                if (CollideFirst<Solid>() == null)
                                {
                                    collided = false;
                                    break;
                                }
                            }
                            if (!collided)
                                break;
                        }
                    }
                }
            }
            AquaDebugger.Assert(!collided, "Something not handled?");
        }

        private static readonly Vector2[] EIGHT_DIRECTIONS = new Vector2[]
        {
            Vector2.UnitY,
            -Vector2.UnitY,
            Vector2.UnitX,
            -Vector2.UnitX,
            new Vector2(-1.0f, -1.0f),
            new Vector2(-1.0f, 1.0f),
            new Vector2(1.0f, -1.0f),
            Vector2.One,
        };

        private GameplayMode _mode = GameplayMode.Default;
        private RopeMaterial _material;
        private Vector2 _movementCounter = Vector2.Zero;
        private Vector2 _prevPosition;
        private Vector2 _playerPrevPosition;    // Player的PreviousPosition貌似在相对速度为0的情况下和当前Position相同
        private bool _lengthLocked = false;
        private float _elapsed = 0.0f;
        private float _lastEmitElapsed;
        private float _fixElapsed;
        private bool _hitUnhookable = false;
        private bool _hitInteractable = false;

        private HookSprite _sprite;
        private Sprite _elecShockSprite;

        private static readonly Vector2[] OFFSET_DIRECTIONS =
        {
            -Vector2.UnitY,
            Calc.SafeNormalize(Vector2.One),
            Calc.SafeNormalize(-Vector2.One),
            Vector2.UnitX,
            -Vector2.UnitX,
            Vector2.UnitY,
            Calc.SafeNormalize(new Vector2(1.0f, -1.0f)),
            Calc.SafeNormalize(new Vector2(-1.0f, 1.0f)),
        };
    }
}
