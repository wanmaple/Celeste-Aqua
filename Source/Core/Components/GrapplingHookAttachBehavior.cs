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
            var players = Scene.Tracker.GetEntities<Player>();
            if (players == null)
                return;
            foreach (Player player in players)
            {
                GrapplingHook hook = player.GetGrappleHook();
                if (hook == null) continue;
                UpdateAction?.Invoke(Entity, hook);
            }
        }
    }
}
