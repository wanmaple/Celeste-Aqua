using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class HookCollider : Component
    {
        public Action<GrapplingHook> Callback { get; set; }

        public HookCollider(Action<GrapplingHook> callback)
            : base(false, false)
        {
            Callback = callback;
        }

        public bool Check(GrapplingHook hook)
        {
            Collider collider = Entity.Collider;
            if (hook.CollideCheck(Entity))
            {
                Callback(hook);
                return true;
            }

            return false;
        }
    }
}
