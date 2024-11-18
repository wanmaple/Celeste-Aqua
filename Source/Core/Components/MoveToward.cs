using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class MoveToward : Component
    {
        public Entity Target { get; private set; }
        public float BaseSpeed { get; private set; }
        public bool FasterAsTarget { get; private set; }

        public MoveToward(Entity target, float baseSpeed, bool fasterAsTarget = true) 
            : base(true, false)
        {
            Target = target;
            BaseSpeed = baseSpeed;
            FasterAsTarget = fasterAsTarget;
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            _targetPrevPosition = Target.Center;
        }

        public override void Update()
        {
            float dt = Engine.DeltaTime;
            float targetSpeed = (Target.Center - _targetPrevPosition).Length() / dt;
            float finalSpeed = BaseSpeed + targetSpeed;
            Vector2 entityToTarget = Target.Center - Entity.Center;
            Vector2 direction = Vector2.Normalize(entityToTarget);
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
        }

        private Vector2 _targetPrevPosition;
    }
}
