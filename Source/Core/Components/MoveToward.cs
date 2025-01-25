using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class MoveToward : Component
    {
        public Entity Target { get; set; }

        public bool DeactiveOnCollidePlayer { get; set; }

        public MoveToward(Entity target, bool deactiveOnCollidePlayer = false)
            : base(true, false)
        {
            Target = target;
            DeactiveOnCollidePlayer = deactiveOnCollidePlayer;
        }

        public override void Update()
        {
            if (Target == null)
                return;
            if (!Entity.Collidable || !Entity.IsHookable())
                return;

            Entity.Position = Target.Center;
            if (DeactiveOnCollidePlayer)
            {
                Player player = Entity.Scene.Tracker.GetEntity<Player>();
                if (player != null && player.CollideCheck(Entity))
                {
                    Entity.Position = player.Center;
                    Active = false;
                }
            }
        }
    }
}
