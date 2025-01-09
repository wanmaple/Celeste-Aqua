using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Area")]
    [Tracked(false)]
    public class UnhookableArea : Entity
    {
        public UnhookableArea(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            this.SetHookable(true);
            Add(new HookInteractable(OnInteractHook));
            if (data.Bool("attachToSolid"))
            {
                var staticMover = new StaticMover();
                staticMover.SolidChecker = solid => CollideCheck(solid);
                staticMover.OnEnable = OnEnable;
                staticMover.OnDisable = OnDisable;
                staticMover.OnDestroy = OnDestroy;
                Add(staticMover);
            }
        }

        private bool OnInteractHook(GrapplingHook hook, Vector2 at)
        {
            Audio.Play("event:/char/madeline/unhookable", Position);
            hook.Revoke();
            return true;
        }

        private void OnEnable()
        {
            Collidable = true;
        }

        private void OnDisable()
        {
            Collidable = false;
        }

        private void OnDestroy()
        {
            Collidable = false;
        }
    }
}
