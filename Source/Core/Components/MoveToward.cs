using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class MoveToward : Component
    {
        public Entity Target { get; set; }

        public bool DeactiveOnCollidePlayer { get; set; }
        public bool SyncHoldableContainer { get; set; }

        public MoveToward(Entity target, bool deactiveOnCollidePlayer = false, bool syncHoldableContainer = false)
            : base(true, false)
        {
            Target = target;
            DeactiveOnCollidePlayer = deactiveOnCollidePlayer;
            SyncHoldableContainer = syncHoldableContainer;
        }

        public override void Update()
        {
            if (Target == null || Target is not GrapplingHook)
                return;
            if (!Entity.Collidable || !Entity.IsHookable())
                return;

            Entity eeveeContainer = Entity.GetHoldableContainer();
            Vector2 oldPos = Entity.Position;
            Entity.Position = Target.Center;
            Vector2 movement = Entity.Position - oldPos;
            if (SyncHoldableContainer && eeveeContainer != null)
            {
                eeveeContainer.Position += movement;
            }
            if (DeactiveOnCollidePlayer)
            {
                GrapplingHook grapple = Target as GrapplingHook;
                Player player = grapple.Owner;
                if (player != null && player.CollideCheck(Entity))
                {
                    oldPos = Entity.Position;
                    Entity.Position = player.Center;
                    movement = Entity.Position - oldPos;
                    if (SyncHoldableContainer && eeveeContainer != null)
                    {
                        eeveeContainer.Position += movement;
                    }
                    Active = false;
                }
            }
        }
    }
}
