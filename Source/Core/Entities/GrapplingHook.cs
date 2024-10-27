using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    //[CustomEntity("Aqua/Grappling Hook")]
    [Tracked(true)]
    public class GrapplingHook : Actor
    {
        public enum HookStates : byte
        {
            None = 0,
            Emitting,
            Revoking,
            Fixed,
        }

        public float HookRadius { get; private set; }   // 爪子的半径，碰撞箱是圆形
        public float RopeLength { get; private set; }   // 钩绳的最大长度

        public HookStates State { get; private set; } = HookStates.None;
        public bool Revoked { get; private set; } = false;

        public GrapplingHook(float radius, float length)
            : base(Vector2.Zero)
        {
            HookRadius = radius;
            RopeLength = length;
            Active = false;

            Add(new HookRope());
            AddTag(Tags.Global);
        }

        public void Emit(Vector2 direction, float speed)
        {
            _direction = direction;
            _speed = speed;
            _movement = 0.0f;
            Revoked = false;
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
            HookRope rope = Get<HookRope>();
            rope.Active = rope.Visible = false;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            Vector2 oldPosition = Position;
            Player player = Scene.Tracker.GetEntity<Player>();
            switch (State)
            {
                case HookStates.Emitting:
                    _movement += _speed * Engine.DeltaTime;
                    bool reachLength = _movement > RopeLength;
                    if (reachLength)
                    {
                        _movement = RopeLength;
                    }
                    Vector2 newPosition = player.Center + _direction * _movement;
                    Vector2 movement = newPosition - oldPosition;
                    bool collided = false;
                    if (!Maths.ApproximateEqual(movement.X, 0.0f))
                    {
                        collided = collided || MoveH(movement.X);
                    }
                    if (!Maths.ApproximateEqual(movement.Y, 0.0f))
                    {
                        collided = collided || MoveV(movement.Y);
                    }
                    if (collided)
                    {
                        State = HookStates.Fixed;
                    }
                    else if (reachLength)
                    {
                        State = HookStates.Revoking;
                    }
                    //Position = newPosition;
                    break;
                case HookStates.Revoking:
                    if (!Revoked)
                    {
                        _movement -= _speed * Engine.DeltaTime;
                        if (_movement <= 0.0f)
                        {
                            Revoked = true;
                            _movement = 0.0f;
                        }
                        Position = player.Center + _direction * _movement;
                    }
                    break;
                case HookStates.Fixed:
                    break;
                default:
                    break;
            }

            base.Update();
        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Position, HookRadius, Color.Red, 16);
        }

        private Vector2 _direction;
        private float _speed;
        private float _movement = 0.0f;
    }
}
