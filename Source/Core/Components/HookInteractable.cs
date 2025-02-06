using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class HookInteractable : Component
    {
        public delegate bool InteractHookHandler(GrapplingHook hook, Vector2 at);

        public InteractHookHandler Interaction { get; set; }
        public Collider Collider { get; set; }
        public bool CollideOutside { get; set; }

        public HookInteractable(InteractHookHandler interaction)
            : base(false, false)
        {
            Interaction = interaction;
        }

        public bool CollideEntity(Entity entity, Vector2 at)
        {
            if (!Entity.Collidable)
                return false;
            Collider collider = Collider == null ? Entity.Collider : Collider;
            if (collider != null)
            {
                Collider old = Entity.Collider;
                Entity.Collider = collider;
                bool collide = false;
                if (!CollideOutside)
                {
                    Vector2 oldPos = entity.Position;
                    entity.Position = at;
                    collide = Collide.Check(entity, Entity);
                    entity.Position = oldPos;
                }
                else
                {
                    collide = !Collide.Check(entity, Entity) && Collide.Check(entity, Entity, at);
                }
                Entity.Collider = old;
                return collide;
            }
            return false;
        }

        public bool OnInteract(GrapplingHook hook, Vector2 at)
        {
            if (Interaction != null)
            {
                return Interaction.Invoke(hook, at);
            }
            return false;
        }
    }
}
