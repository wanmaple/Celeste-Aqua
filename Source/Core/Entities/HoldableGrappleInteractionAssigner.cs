using Monocle;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core.Entities
{
    [CustomEntity("Aqua/Holdable Grapple Interaction Assigner")]
    public class HoldableGrappleInteractionAssigner : Entity
    {
        public float Mass { get; private set; }
        public float StaminaCost { get; private set; }
        public IReadOnlyList<string> Blacklist { get; private set; }

        public HoldableGrappleInteractionAssigner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Mass = data.Float("mass", PlayerStates.MADELINE_MASS);
            if (Mass < 0.0f)
                Mass = 0.0f;
            else if (Mass > 10.0f)
                Mass = float.PositiveInfinity;
            StaminaCost = MathF.Max(data.Float("stamina_cost", 20.0f), 0.0f);
            string blacklist = data.Attr("blacklist");
            if (!string.IsNullOrWhiteSpace(blacklist))
            {
                string[] array = blacklist.Split(',').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToArray();
                Blacklist = array;
            }
            else
            {
                Blacklist = Array.Empty<string>();
            }

            this.MakeExtraCollideCondition();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            var actors = scene.Tracker.GetEntities<Actor>();
            foreach (Actor actor in actors)
            {
                if (CheckCollision(actor) && !IsInBlacklist(actor))
                {
                    AssignInteractionToEntity(actor);
                }
            }
        }

        private bool CheckCollision(Actor actor)
        {
            if (ModInterop.HoldableType != null && actor.GetType() ==  ModInterop.HoldableType)
            {
                Hitbox collider = actor.Collider as Hitbox;
                return Collide.CheckRect(this, new Rectangle((int)collider.AbsoluteLeft, (int)collider.AbsoluteTop, (int)collider.Width, (int)collider.Height));
            }
            return CollideCheck(actor);
        }

        private bool IsInBlacklist(Entity entity)
        {
            string fullname = entity.GetType().FullName;
            foreach (string black in Blacklist)
            {
                if (fullname.Contains(black, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void AssignInteractionToEntity(Actor actor)
        {
            actor.SetHookable(true);
            actor.SetMass(Mass);
            actor.SetStaminaCost(StaminaCost);
            if (actor is not TheoCrystal && actor is not Glider)
            {
                Holdable holdable = actor.Get<Holdable>();
                HookInteractable interactable = new HookInteractable(actor.GeneralHoldableInteraction);
                interactable.Collider = holdable.PickupCollider;
                interactable.CollideOutside = true;
                actor.Add(interactable);
            }
        }

        private bool CanCollide(Entity other)
        {
            if (other is Platform || other is Player)
                return false;
            Holdable holdable = other.Get<Holdable>();
            return holdable != null;
        }
    }
}
