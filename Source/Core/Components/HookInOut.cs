using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class HookInOut : Component
    {
        public HookInOut(Action<GrapplingHook> hookIn, Action<GrapplingHook> hookOut)
            : base(true, false)
        {
            _hookIn = hookIn;
            _hookOut = hookOut;
        }

        public override void Update()
        {
            GrapplingHook hook = Scene.Tracker.GetEntity<GrapplingHook>();
            bool currentIn = hook != null && Entity.CollideCheck(hook);
            if (_lastIn != currentIn)
            {
                if (currentIn)
                {
                    _hookIn?.Invoke(hook);
                }
                else
                {
                    _hookOut?.Invoke(hook);
                }
            }
            _lastIn = currentIn;
        }

        private bool _lastIn = false;
        private Action<GrapplingHook> _hookIn;
        private Action<GrapplingHook> _hookOut;
    }
}
