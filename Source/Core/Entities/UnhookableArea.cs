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
        }

        private bool OnInteractHook(GrapplingHook hook, Vector2 at)
        {
            Audio.Play("event:/char/madeline/unhookable", Position);
            hook.Revoke();
            return true;
        }
    }
}
