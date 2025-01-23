using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class MoveToward : Component
    {
        public Entity Target
        {
            get => _target;
            set
            {
                _target = value;
                if (_target != null)
                {
                    _targetPrevPosition = _target.Center;
                }
            }
        }
        public float BaseSpeed { get; set; }
        public bool FasterAsTarget { get; set; }
        public bool DeactiveOnCollidePlayer { get; set; }

        public MoveToward(Entity target, float baseSpeed, bool fasterAsTarget = true, bool deactiveOnCollidePlayer = false)
            : base(true, false)
        {
            Target = target;
            BaseSpeed = baseSpeed;
            FasterAsTarget = fasterAsTarget;
            DeactiveOnCollidePlayer = deactiveOnCollidePlayer;
        }

        public override void Update()
        {
            if (_target == null)
                return;

            float dt = Engine.DeltaTime;
            float targetSpeed = (Target.Center - _targetPrevPosition).Length() / dt;
            float finalSpeed = BaseSpeed + targetSpeed;
            Vector2 entityToTarget = Target.Center - Entity.Center;
            Vector2 direction = Calc.SafeNormalize(entityToTarget);
            if (!AquaMaths.IsApproximateZero(direction))
            {
                Vector2 movement = direction * finalSpeed * dt;
                if (movement.LengthSquared() > entityToTarget.LengthSquared())
                {
                    Entity.Position = Target.Center;
                }
                else
                {
                    Entity.Position += direction * finalSpeed * dt;
                }
            }
            _targetPrevPosition = Target.Center;

            if (DeactiveOnCollidePlayer)
            {
                Player player = Entity.Scene.Tracker.GetEntity<Player>();
                if (player != null && player.CollideCheck(Entity))
                {
                    Active = false;
                }
            }
        }

        private Vector2 _targetPrevPosition;
        private Entity _target;
    }
}
