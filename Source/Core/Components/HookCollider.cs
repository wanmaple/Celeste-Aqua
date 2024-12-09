using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class HookCollider : Component
    {
        public HookCollider(Action<GrapplingHook> callback)
            : base(false, false)
        {
            _callback = callback;
        }

        public bool Check(GrapplingHook hook)
        {
            Collider collider = Entity.Collider;
            if (hook.CollideCheck(Entity))
            {
                _callback(hook);
                return true;
            }

            return false;
        }

        private Action<GrapplingHook> _callback;
    }
}
