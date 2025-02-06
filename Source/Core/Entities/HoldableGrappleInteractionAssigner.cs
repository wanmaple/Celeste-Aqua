using Monocle;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Celeste.Mod.Entities;

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
            foreach (Entity entity in Scene.Entities)
            {
                if (CollideCheck(entity) && !IsInBlacklist(entity))
                {
                    AssignInteractionToEntity(entity);
                }
            }
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

        private void AssignInteractionToEntity(Entity entity)
        {
            entity.SetHookable(true);
            if (entity is Actor actor)
            {
                actor.SetMass(Mass);
                actor.SetStaminaCost(StaminaCost);
            }
            else
            {
                ActorExtraFields extras = new ActorExtraFields();
                extras.Mass = Mass;
                extras.StaminaCost = StaminaCost;
                entity.Add(extras);
            }
            if (entity is not TheoCrystal && entity is not Glider && entity is Actor actor2)
            {
                Holdable holdable = entity.Get<Holdable>();
                HookInteractable interactable = new HookInteractable(actor2.GeneralHoldableInteraction);
                interactable.Collider = holdable.PickupCollider;
                interactable.CollideOutside = true;
                entity.Add(interactable);
            }
            else if (entity is not Actor)
            {
                // might be eevee helper case.
            }
        }

        private bool CanCollide(Entity other)
        {
            if (other is Platform)
                return false;
            Holdable holdable = other.Get<Holdable>();
            return holdable != null;
        }
    }
}
