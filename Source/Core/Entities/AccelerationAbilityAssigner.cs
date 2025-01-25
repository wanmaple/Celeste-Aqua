using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Acceleration Ability Assigner")]
    public class AccelerationAbilityAssigner : Entity
    {
        public float Acceleration { get; private set; }

        public AccelerationAbilityAssigner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Acceleration = MathF.Max(data.Float("acceleration"), 0.0f);
            this.MakeExtraCollideCondition();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Solid solid in scene.Tracker.GetEntities<Solid>())
            {
                if (CollideCheck(solid))
                {
                    solid.Add(new AccelerationAreaInOut(area =>
                    {
                        area.CommonAccelerate(solid, Acceleration);
                    }, null, null));
                }
            }
        }

        private bool CanCollide(Entity other)
        {
            if (other is Player)
                return false;
            return other is Actor || other is Solid;
        }
    }
}
