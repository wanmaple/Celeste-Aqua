using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Debug;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class GrapplingHook : Entity
    {
        public enum HookStates : byte
        {
            None = 0,
            Emitting,
            Revoking,
            Bouncing,
            Fixed,
        }

        public enum RopeMaterial
        {
            Default,
            Metal,
        }

        public const float BOUNCE_SPEED_ADDITION = 350.0f;

        public float HookSize { get; private set; }   // 爪子的边长，碰撞箱是正方形
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

        public HookStates State { get; private set; } = HookStates.None;
        public Vector2 Velocity { get; private set; }
        public Vector2 Acceleration { get; private set; }
        public bool Revoked { get; private set; } = false;
        public bool JustFixed { get; private set; } = false;
        public float AlongRopeSpeed { get; set; } = 0.0f;
        public Vector2 BouncingVelocity { get; set; }
        public float EmitSpeedCoefficient { get; private set; }

        public float MaxLength => Get<HookRope>().MaxLength;
        public float LockedRadius => Get<HookRope>().LockedLength;
        public float SwingRadius => Get<HookRope>().SwingRadius;
        public Vector2 HookDirection => Get<HookRope>().HookDirection;
        public Vector2 RopeDirection => Get<HookRope>().RopeDirection;
        public Vector2 SwingDirection => Get<HookRope>().SwingDirection;
        public Vector2 PlayerPreviousPosition => _playerPrevPosition;

        public GrapplingHook(float size, float length, RopeMaterial material = RopeMaterial.Default)
            : base(Vector2.Zero)
        {
            HookSize = size;
            _material = material;
            Active = false;

            Collider = new Hitbox(size, size, -size * 0.5f, -size * 0.5f);

            Add(new HookRope(length, material));
            Add(_sprite = new HookSprite());
            Add(_elecShockSprite = new Sprite());
            GFX.SpriteBank.CreateOn(_elecShockSprite, "HookElectricShock");
            AddTag(Tags.Global);

            this.MakeExtraCollideCondition();
        }

        public bool CanCollide(Entity other)
        {
            return other.IsHookable();
        }

        public void SetPositionRounded(Vector2 position)
        {
            Position = AquaMaths.Round(position);
            HookRope rope = Get<HookRope>();
            if (rope != null)
            {
                rope.UpdateTopPivot(Position);
            }
        }

        public void Emit(Vector2 direction, float speed, float speedCoeff)
        {
            AlongRopeSpeed = 0.0f;
            HookRope rope = Get<HookRope>();
            rope.CurrentDirection = direction;
            rope.EmitSpeed = speed;
            EmitSpeedCoefficient = speedCoeff;
            Revoked = false;

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
            _fixTimer.Reset();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        public bool Bounce(Vector2 axis, float speed)
        {
            axis = Vector2.Normalize(axis);
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
            BouncingVelocity = Vector2.Normalize(BouncingVelocity) * rope.EmitSpeed * (1.0f + EmitSpeedCoefficient);
            return true;
        }

        public void SetRopeLengthLocked(bool locked, Vector2 playerPosition)
        {
            if (locked && _lengthLocked == locked) return;
            _lengthLocked = locked;
            HookRope rope = Get<HookRope>();
            rope.SetLengthLocked(locked, playerPosition);
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

        public bool CanFlyToward()
        {
            float range = (Scene as Level).GetState().HookSettings.FlyTowardDuration;
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
            Position = _prevPosition = AquaMaths.Round(madeline.Center);
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
            Velocity = Acceleration = Vector2.Zero;
            Active = false;
            JustFixed = false;
            HookRope rope = Get<HookRope>();
            rope.Active = rope.Visible = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            Player player = Scene.Tracker.GetEntity<Player>();
            _playerPrevPosition = player.ExactCenter();
            _elapsed = 0.0f;
            _lastEmitElapsed = float.MinValue;
            _fixElapsed = float.MinValue;
        }

        public override void Update()
        {
            HookRope rope = Get<HookRope>();
            Player player = Scene.Tracker.GetEntity<Player>();
            float dt = Engine.DeltaTime;
            _elapsed += dt;
            JustFixed = false;
            _elecShockSprite.Visible = ElectricShocking;
            if (player.StateMachine.State == (int)AquaStates.StElectricShocking)
            {
                _elecShockSprite.Visible = true;
                rope.ElectricShocking = true;
                base.Update();
                return;
            }

            UpdateColliderOffset();
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
                        rope.CheckCollision(playerSeg);
                        nextPosition = rope.DetectHookNextPosition(dt, false, CalculateSpeedCoefficient(false), out changeState);
                        movement = nextPosition - prevPosition;
                        collided = BresenhamMove(movement, OnCollideEntity);
                        //if (!AquaMaths.IsApproximateZero(movement.X))
                        //{
                        //    collided = collided || MoveH(movement.X, OnCollideEntity);
                        //}
                        //if (!AquaMaths.IsApproximateZero(movement.Y))
                        //{
                        //    collided = collided || MoveV(movement.Y, OnCollideEntity);
                        //}
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
                        rope.CheckCollision(playerSeg);
                        movement = BouncingVelocity * dt;
                        float currentLength = rope._prevLength + movement.Length();
                        if (currentLength > rope.MaxLength)
                        {
                            movement = Vector2.Normalize(movement) * (movement.Length() - (currentLength - rope.MaxLength));
                            changeState = true;
                        }
                        collided = BresenhamMove(movement, OnCollideEntity);
                        //if (!AquaMaths.IsApproximateZero(movement.X))
                        //{
                        //    collided = collided || MoveH(movement.X, OnCollideEntity);
                        //}
                        //if (!AquaMaths.IsApproximateZero(movement.Y))
                        //{
                        //    collided = collided || MoveV(movement.Y, OnCollideEntity);
                        //}
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
                        _fixTimer.Tick(dt);
                        if (_fixTimer.Check())
                        {
                            Audio.Play("event:/char/madeline/hook_fixing", Position);
                            _fixTimer.Expire();
                        }
                        _playerPrevPosition = player.ExactCenter();
                        break;
                    default:
                        break;
                }
            }

            _prevPosition = Position;
            Velocity /= dt;
            Acceleration = (Velocity - lastVelocity) / dt;
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

        private void UpdateColliderOffset()
        {
            if (State != HookStates.Fixed)
            {
                Hitbox box = Collider as Hitbox;
                Vector2 orig = new Vector2(-box.Width * 0.5f, -box.Height * 0.5f);
                float cosVal = MathF.Cos(MathF.PI * 0.125f);
                Vector2 direction = HookDirection;
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

        private void OnCollideEntity(CollisionData collisionData)
        {
            HookRope rope = Get<HookRope>();
            Entity hitEntity = collisionData.Hit;
            if (hitEntity.IsHookable())
            {
                rope.HookAttachEntity(hitEntity);
                hitEntity.SetHookAttached(true);
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
            Water water = CollideFirst<Water>();
            if (water != null)
            {
                coeff *= 0.5f;
            }
            return coeff;
        }

        private bool BresenhamMove(Vector2 movement, Collision onCollide = null)
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
                        if (StepMoveV(step, onCollide))
                            return true;
                        dy -= step;
                        Y += step;
                    }
                }
                else if (dy == 0)
                {
                    int step = MathF.Sign(dx);
                    while (dx != 0)
                    {
                        if (StepMoveH(step, onCollide))
                            return true;
                        dx -= step;
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
                        // swap dx, dy
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
                        if (error >= 2 * absX)
                        {
                            error -= 2 * absX;
                            if ((!swapped && StepMoveV(stepY, onCollide)) || (swapped && StepMoveH(stepX, onCollide)))
                            {
                                if (swapped)
                                    _movementCounter.X = 0.0f;
                                else
                                    _movementCounter.Y = 0.0f;
                                return true;
                            }
                            else
                            {
                                if (!swapped)
                                    Y += stepY;
                                else
                                    X += stepX;
                            }
                        }
                        if ((!swapped && StepMoveH(stepX, onCollide)) || (swapped && StepMoveV(stepY, onCollide)))
                        {
                            if (swapped)
                                _movementCounter.Y = 0.0f;
                            else
                                _movementCounter.X = 0.0f;
                            return true;
                        }
                        else
                        {
                            if (!swapped)
                                X += stepX;
                            else
                                Y += stepY;
                        }
                        absX--;
                    } while (absX != 0);
                }
            }
            return false;
        }

        private bool StepMoveH(int step, Collision onCollide)
        {
            if (CheckInteractables(Position + Vector2.UnitX * step))
            {
                return true;
            }

            Solid solid = CollideFirst<Solid>(Position + Vector2.UnitX * step);
            if (solid != null)
            {
                _movementCounter.X = 0f;
                onCollide?.Invoke(new CollisionData
                {
                    Direction = Vector2.UnitX * step,
                    Hit = solid,
                    Pusher = null,
                });

                return true;
            }
            _movementCounter.X -= step;
            return false;
        }

        private bool StepMoveV(int step, Collision onCollide)
        {
            if (CheckInteractables(Position + Vector2.UnitY * step))
            {
                return true;
            }

            Platform platform = CollideFirst<Solid>(Position + Vector2.UnitY * step);
            CollisionData data;
            if (platform != null)
            {
                _movementCounter.Y = 0f;
                if (onCollide != null)
                {
                    data = new CollisionData
                    {
                        Direction = Vector2.UnitY * step,
                        Hit = platform,
                        Pusher = null,
                    };
                    onCollide(data);
                }

                return true;
            }

            if (step > 0)
            {
                platform = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * step);
                if (platform != null)
                {
                    _movementCounter.Y = 0f;
                    if (onCollide != null)
                    {
                        data = new CollisionData
                        {
                            Direction = Vector2.UnitY * step,
                            Hit = platform,
                            Pusher = null,
                        };
                        onCollide(data);
                    }

                    return true;
                }
            }
            _movementCounter.Y -= step;
            return false;
        }

        private bool CheckInteractables(Vector2 at)
        {
            _hitInteractable = false;
            Vector2 position = Position;
            Position = at;
            List<Component> interactables = Scene.Tracker.GetComponents<HookInteractable>();
            for (int i = 0; i < interactables.Count; i++)
            {
                HookInteractable interactable = interactables[i] as HookInteractable;
                Entity entity = interactable.Entity;
                if (entity != null && entity.Collidable && entity.Collider != null && Collide.Check(this, entity))
                {
                    if (interactable.OnInteract(this, at))
                    {
                        _hitInteractable = true;
                        break;
                    }
                }
            }
            Position = position;
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
        private TimeTicker _fixTimer = new TimeTicker(0.08f);

        private HookSprite _sprite;
        private Sprite _elecShockSprite;

        private static readonly Vector2[] OFFSET_DIRECTIONS =
        {
            -Vector2.UnitY,
            Vector2.Normalize(Vector2.One),
            Vector2.Normalize(-Vector2.One),
            Vector2.UnitX,
            -Vector2.UnitX,
            Vector2.UnitY,
            Vector2.Normalize(new Vector2(1.0f, -1.0f)),
            Vector2.Normalize(new Vector2(-1.0f, 1.0f)),
        };
    }
}
