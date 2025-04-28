using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Flag Assigner")]
    public class GrappleFlagAssigner : GrappleInteractionAssigner
    {
        public string Flag { get; private set; }
        public bool State { get; private set; }
        public bool Attach { get; private set; }

        public GrappleFlagAssigner(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            State = data.Bool("state", true);
            Attach = data.Bool("attach", true);
            this.MakeExtraCollideCondition();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            var platforms = scene.Tracker.GetEntities<Platform>();
            foreach (Platform platform in platforms)
            {
                if (CollideCheck(platform) && !IsInBlacklist(platform))
                {
                    SetupEntity(platform);
                }
            }
            var magnets = scene.Tracker.GetEntities<GrappleAttractor>();
            foreach (GrappleAttractor magnet in magnets)
            {
                if (CollideCheck(magnet) && !IsInBlacklist(magnet))
                {
                    SetupEntity(magnet);
                }
            }
            RemoveSelf();
        }

        private void SetupEntity(Entity entity)
        {
            Action<GrapplingHook> onAttach = entity.GetAttachCallback();
            Action<GrapplingHook> onDetach = entity.GetDetachCallback();
            if (Attach)
            {
                if (onAttach != null)
                {
                    onAttach += OnAttachedOrDetached;
                }
                else
                {
                    onAttach = OnAttachedOrDetached;
                }
            }
            else
            {
                if (onDetach != null)
                {
                    onDetach += OnAttachedOrDetached;
                }
                else
                {
                    onDetach = OnAttachedOrDetached;
                }
            }
            entity.SetAttachCallbacks(onAttach, onDetach);
        }

        private void OnAttachedOrDetached(GrapplingHook grapple)
        {
            if (!string.IsNullOrEmpty(Flag))
            {
                grapple.SceneAs<Level>().Session.SetFlag(Flag, State);
            }
        }

        private bool CanCollide(Entity other)
        {
            return other.IsHookable();
        }
    }
}
