using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
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
            if (SceneAs<Level>().Session.GetFlag(Flag))
            {
                SetActivated(!_activated);
                SceneAs<Level>().Session.SetFlag(Flag, false);
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
                    GrapplingHook grapple = Scene.Tracker.GetEntity<GrapplingHook>();
                    if (grapple != null && grapple.State == GrapplingHook.HookStates.Attracted && grapple.CurrentAttractor == this)
                    {
                        grapple.CancelAttraction();
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
                grapple.AttractTo(this);
                return true;
            }
            return false;
        }

        protected bool _activated = true;
    }
}
