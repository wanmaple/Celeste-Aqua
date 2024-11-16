﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Area")]
    [Tracked(true)]
    public class UnhookableArea : Entity
    {
        public UnhookableArea(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);

            Add(new HookInteractable(OnInteractHook));
        }

        private bool OnInteractHook(GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            return true;
        }
    }
}
