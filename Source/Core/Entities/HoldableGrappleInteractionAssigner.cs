using Monocle;
using Microsoft.Xna.Framework;
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core.Entities
{
    [CustomEntity("Aqua/Holdable Grapple Interaction Assigner")]
    public class HoldableGrappleInteractionAssigner : GrappleInteractionAssigner
    {
        public float Mass { get; private set; }
        public float StaminaCost { get; private set; }
        public float AgainstBoostCoefficient { get; private set; }

        public HoldableGrappleInteractionAssigner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Mass = data.Float("mass", PlayerStates.MADELINE_MASS);
            if (Mass < 0.0f)
                Mass = 0.0f;
            else if (Mass >= PlayerStates.MAX_INFINITE_MASS)
                Mass = float.PositiveInfinity;
            StaminaCost = MathF.Max(data.Float("stamina_cost", 20.0f), 0.0f);
            AgainstBoostCoefficient = MathHelper.Clamp(data.Float("against_boost_coefficient", 1.0f), 0.0f, 1.0f);
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

        private void AssignInteractionToEntity(Actor actor)
        {
            actor.SetHookable(true);
            actor.SetMass(Mass);
            actor.SetStaminaCost(StaminaCost);
            actor.SetAgainstBoostCoefficient(AgainstBoostCoefficient);
            if (actor is not TheoCrystal && actor is not Glider)
            {
                Holdable holdable = actor.Get<Holdable>();
                HookInteractable interactable = actor.Get<HookInteractable>();
                if (interactable == null)
                {
                    interactable = new HookInteractable(null);
                    actor.Add(interactable);
                }
                interactable.Interaction = actor.GeneralHoldableInteraction;
                interactable.Collider = holdable.PickupCollider;
                interactable.CollideOutside = true;
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
