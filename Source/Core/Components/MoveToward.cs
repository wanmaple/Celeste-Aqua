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
            if (Target == null || Target is not GrapplingHook)
                return;
            if (!Entity.Collidable || !Entity.IsHookable())
                return;

            Entity.Position = Target.Center;
            if (DeactiveOnCollidePlayer)
            {
                GrapplingHook grapple = Target as GrapplingHook;
                Player player = grapple.Owner;
                if (player != null && player.CollideCheck(Entity))
                {
                    Entity.Position = player.Center;
                    Active = false;
                }
            }
        }
    }
}
