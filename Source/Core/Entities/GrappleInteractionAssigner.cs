using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Interaction Assigner")]
    public class GrappleInteractionAssigner : Entity
    {
        public enum GrappleInteractions
        {
            None,
            GrabToPlayer,
            PullPlayer,
        }

        public GrappleInteractions InteractionType { get; private set; }
        public IReadOnlyList<string> Blacklist { get; private set; }

        public GrappleInteractionAssigner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            switch (data.Attr("interaction_type"))
            {
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
            switch (InteractionType)
            {
                case GrappleInteractions.GrabToPlayer:
                    MakeEnableToGrab(entity);
                    break;
                case GrappleInteractions.PullPlayer:
                    MakeEnableToPullPlayer(entity);
                    break;
            }
        }

        private void MakeEnableToGrab(Entity entity)
        {
            entity.MakeSelfEnableGrabToPlayer();
        }

        private void MakeEnableToPullPlayer(Entity entity)
        {

        }

        private bool CanCollide(Entity other)
        {
            if (other is Decal || other is Platform || other is Actor)
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
