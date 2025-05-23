﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public abstract class GrappleAttractor : Entity
    {
        public abstract Vector2 AttractionTarget { get; }
        public string Flag { get; private set; }
        public abstract float MinRange { get; }
        public abstract float MaxRange { get; }

        protected GrappleAttractor(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.SetHookable(true);
            Add(new HookInteractable(OnGrappleInteract));
            Flag = data.Attr("flag");
            Depth = Depths.SolidsBelow;
        }

        public override void Update()
        {
            base.Update();
            if (!string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag))
            {
                SetActivated(!_activated);
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => SceneAs<Level>().Session.SetFlag(Flag, false), 0.0f, true));
            }
        }

        public void SetActivated(bool activated)
        {
            if (_activated != activated)
            {
                _activated = activated;
                this.SetHookable(activated);
                if (!activated)
                {
                    var grapples = Scene.Tracker.GetEntities<GrapplingHook>();
                    foreach (GrapplingHook grapple in grapples)
                    {
                        if (grapple.State == GrapplingHook.HookStates.Attracted && grapple.CurrentAttractor == this)
                        {
                            grapple.CancelAttraction();
                        }
                    }
                    OnDeactivated();
                }
                else
                {
                    OnActivated();
                }
            }
        }

        protected virtual void OnActivated()
        {
        }

        protected virtual void OnDeactivated()
        {
        }

        private bool OnGrappleInteract(GrapplingHook grapple, Vector2 at)
        {
            if (grapple.State == GrapplingHook.HookStates.Emitting || grapple.State == GrapplingHook.HookStates.Bouncing)
            {
                List<Entity> attractors = Scene.Tracker.GetEntities<GrappleAttractor>();
                float minDistance = float.MaxValue;
                GrappleAttractor preferAttractor = this;
                foreach (GrappleAttractor attractor in attractors)
                {
                    float disSq = (attractor.AttractionTarget - grapple.Position).LengthSquared();
                    if (disSq < minDistance)
                    {
                        minDistance = disSq;
                        preferAttractor = attractor;
                    }
                }
                grapple.AttractTo(preferAttractor);
                return true;
            }
            return false;
        }

        protected bool _activated = true;
    }
}
