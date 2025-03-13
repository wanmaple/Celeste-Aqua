using Monocle;
using System;
using System.Collections.Generic;

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
            var grapples = Scene.Tracker.GetEntities<GrapplingHook>();
            foreach (GrapplingHook grapple in grapples)
            {
                bool hookable = Entity.IsHookable();
                Entity.SetHookable(true);
                bool currentIn = Entity.CollideCheck(grapple);
                Entity.SetHookable(hookable);
                bool lastIn = _inGrapples.Contains(grapple);
                if (lastIn != currentIn)
                {
                    if (currentIn)
                    {
                        _inGrapples.Add(grapple);
                        _hookIn?.Invoke(grapple);
                    }
                    else
                    {
                        _inGrapples.Remove(grapple);
                        _hookOut?.Invoke(grapple);
                    }
                }
                if (currentIn)
                {
                    _keepIn?.Invoke(grapple);
                }
            }
        }

        private HashSet<GrapplingHook> _inGrapples = new HashSet<GrapplingHook>(4);
        private Action<GrapplingHook> _hookIn;
        private Action<GrapplingHook> _hookOut;
        private Action<GrapplingHook> _keepIn;
    }
}
