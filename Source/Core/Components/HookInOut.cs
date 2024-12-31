using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class HookInOut : Component
    {
        public HookInOut(Action<GrapplingHook> hookIn, Action<GrapplingHook> hookOut, Action<GrapplingHook> keepIn = null)
            : base(true, false)
        {
            _hookIn = hookIn;
            _hookOut = hookOut;
            _keepIn = keepIn; 
        }

        public override void Update()
        {
            GrapplingHook hook = Scene.Tracker.GetEntity<GrapplingHook>();
            bool hookable = Entity.IsHookable();
            Entity.SetHookable(true);
            bool currentIn = hook != null && Entity.CollideCheck(hook);
            Entity.SetHookable(hookable);
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
            if (currentIn)
            {
                _keepIn?.Invoke(hook);
            }
            _lastIn = currentIn;
        }

        private bool _lastIn = false;
        private Action<GrapplingHook> _hookIn;
        private Action<GrapplingHook> _hookOut;
        private Action<GrapplingHook> _keepIn;
    }
}
