using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class GrapplingHookAttachBehavior : Component
    {
        public Action<Entity, GrapplingHook> UpdateAction { get; private set; }

        public GrapplingHookAttachBehavior(Action<Entity, GrapplingHook> action)
            : base(true, false)
        {
            UpdateAction = action;
        }

        public override void Update()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;
            GrapplingHook hook = player.GetGrappleHook();
            if (hook == null)
                return;

            UpdateAction?.Invoke(Entity, hook);
        }
    }
}
