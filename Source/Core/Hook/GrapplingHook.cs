using Celeste.Mod.Aqua.Miscellaneous;
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

        public float HookSize { get; private set; }   // 爪子的边长，碰撞箱是正方形

        public HookStates State { get; private set; } = HookStates.None;
        public bool Revoked { get; private set; } = false;

        public GrapplingHook(float size, float length)
            : base(Vector2.Zero)
        {
            HookSize = size;
            Active = false;

            Collider = new Hitbox(size, size, -size * 0.5f, -size * 0.5f);

            Add(new HookRope(length));
            AddTag(Tags.Global);
        }

        public void Emit(Vector2 direction, float speed)
        {
            HookRope rope = Get<HookRope>();
            rope.CurrentDirection = direction;
            rope.EmitSpeed = speed;
            Revoked = false;

            //_direction = direction;
            //_speed = speed;
            //_currentLength = 0.0f;
            //Revoked = false;
        }

        public void Revoke()
        {
            State = HookStates.Revoking;
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
            Vector2 prevPosition = Position;
            Vector2 nextPosition = Position;
            HookRope rope = Get<HookRope>();
            rope.ReleasePivotsIfPossible();
            Player player = Scene.Tracker.GetEntity<Player>();
            Segment ropeSeg = new Segment(player.PreviousPosition + player.Center - player.Position, rope.BottomPivot);
            Segment playerSeg = new Segment(player.PreviousPosition + player.Center - player.Position, player.Center);
            switch (State)
            {
                case HookStates.Emitting:
                    bool changeState;
                    nextPosition = rope.DetectHookNextPosition(Engine.DeltaTime, false, out changeState);
                    Vector2 movement = nextPosition - prevPosition;
                    bool collided = false;
                    if (!AquaMaths.IsApproximateZero(movement.X))
                    {
                        collided = collided || MoveH(movement.X);
                    }
                    if (!AquaMaths.IsApproximateZero(movement.Y))
                    {
                        collided = collided || MoveV(movement.Y);
                    }
                    if (collided)
                    {
                        State = HookStates.Fixed;
                    }
                    else if (changeState)
                    {
                        Revoke();
                    }
                    break;
                case HookStates.Revoking:
                    bool revokeHook;
                    nextPosition = rope.DetectHookNextPosition(Engine.DeltaTime, true, out revokeHook);
                    Revoked = revokeHook;
                    Position = nextPosition;
                    break;
                case HookStates.Fixed:
                    rope.CheckCollision(ropeSeg, playerSeg);
                    rope.UpdateCurrentDirection();
                    break;
                default:
                    break;
            }

            base.Update();
        }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.Red);
        }
    }
}
