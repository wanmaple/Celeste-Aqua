using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Interaction Assigner")]
    public class SimpleGrappleInteractionAssigner : GrappleInteractionAssigner
    {
        public enum GrappleInteractions
        {
            None,
            CancelInteraction,
            GrabToPlayer,
            PullPlayer,
        }

        public GrappleInteractions InteractionType { get; private set; }
        public bool SyncHoldableContainer { get; private set; }

        public SimpleGrappleInteractionAssigner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            SyncHoldableContainer = data.Bool("sync_holdable_container", false);
            switch (data.Attr("interaction_type"))
            {
                case "CancelInteraction":
                    InteractionType = GrappleInteractions.CancelInteraction;
                    break;
                case "GrabToPlayer":
                    InteractionType = GrappleInteractions.GrabToPlayer;
                    break;
                case "PullPlayer":
                    InteractionType = GrappleInteractions.PullPlayer;
                    break;
                default:
                    InteractionType = GrappleInteractions.None;
                    break;
            }
            this.MakeExtraCollideCondition();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (InteractionType != GrappleInteractions.None)
            {
                foreach (Entity entity in Scene.Entities)
                {
                    if (CollideCheck(entity) && !IsInBlacklist(entity))
                    {
                        AssignInteractionToEntity(entity);
                    }
                }
            }
            RemoveSelf();
        }

        private void AssignInteractionToEntity(Entity entity)
        {
            entity.SetHookable(true);
            switch (InteractionType)
            {
                case GrappleInteractions.CancelInteraction:
                    CancelGrappleInteraction(entity);
                    break;
                case GrappleInteractions.GrabToPlayer:
                    MakeEnableToGrab(entity);
                    break;
                case GrappleInteractions.PullPlayer:
                    MakeEnableToPullPlayer(entity);
                    break;
            }
        }

        private void CancelGrappleInteraction(Entity entity)
        {
            var interactable = entity.Get<HookInteractable>();
            if (interactable != null)
            {
                entity.Remove(interactable);
            }
        }

        private void MakeEnableToGrab(Entity entity)
        {
            entity.MakeSelfEnableGrabToPlayer(SyncHoldableContainer);
        }

        private void MakeEnableToPullPlayer(Entity entity)
        {

        }

        private bool CanCollide(Entity other)
        {
            if (other is Decal || other is Platform || other is Actor || other is Trigger)
                return false;
            if (other is UnhookableArea || other is UnhookableCrystalSpinner)
                return false;
            if (other is GrappleInteractionAssigner || other is DecalAssigner)
                return false;
            var sidewaysJumpthruTypes = ModInterop.SidewaysJumpthruTypes;
            foreach (Type type in sidewaysJumpthruTypes)
            {
                if (other.GetType().IsAssignableTo(type))
                    return false;
            }
            return true;
        }
    }
}
