﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Refill")]
    [Tracked(false)]
    public class AquaRefill : Refill
    {
        public static ParticleType P_ShatterHookable1;
        public static ParticleType P_GlowHookable1;
        public static ParticleType P_RegenHookable1;
        public static ParticleType P_ShatterHookable2;
        public static ParticleType P_GlowHookable2;
        public static ParticleType P_RegenHookable2;

        public bool HookTouchable { get; private set; }

        public AquaRefill(Vector2 position, bool twoDashes, bool hookable, bool oneUse)
            : base(position, twoDashes, oneUse)
        {
            HookTouchable = hookable;
            _respawnPosition = Position;
            if (HookTouchable)
            {
                string dir = string.Empty;
                string animID = string.Empty;
                if (twoDashes)
                {
                    dir = "objects/refills/refillTwo_Hookable/";
                    animID = "Aqua_RefillTwo";
                    p_shatter = P_ShatterHookable2;
                    p_glow = P_GlowHookable2;
                    p_regen = P_RegenHookable2;
                }
                else
                {
                    dir = "objects/refills/refillOne_Hookable/";
                    animID = "Aqua_RefillOne";
                    p_shatter = P_ShatterHookable1;
                    p_glow = P_GlowHookable1;
                    p_regen = P_RegenHookable1;
                }
                outline.Texture = GFX.Game[dir + "outline"];
                GFX.SpriteBank.CreateOn(sprite, animID);
                GFX.SpriteBank.CreateOn(flash, animID + "Flash");
                this.SetHookable(true);
            }
            PlayerCollider old = Get<PlayerCollider>();
            old.OnCollide = OnPlayerCollide;
            Add(new HookInteractable(OnHookInteract));
            Add(_moveToward = new MoveToward(null, 0.0f, true));
            _moveToward.Active = false;
        }

        public AquaRefill(EntityData data, Vector2 offset) 
            : this(data.Position + offset, data.Bool("twoDash"), data.Bool("hookable"), data.Bool("oneUse"))
        {
        }

        private void OnPlayerCollide(Player player)
        {
            if (player.UseRefill(twoDashes))
            {
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                outline.Position = _respawnPosition - Position;
                Add(new Coroutine(RefillAndResetRoutine(player)));
                respawnTimer = 2.5f;
                _hookCollided = false;
                _moveToward.Active = false;
            }
        }

        private bool OnHookInteract(GrapplingHook hook, Vector2 at)
        {
            if (!HookTouchable || _hookCollided)
                return false;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player.Dashes < (twoDashes ? 2 : 1))
            {
                hook.Revoke();
                _moveToward.Target = hook;
                _moveToward.BaseSpeed = 100000.0f;
                _moveToward.Active = true;
                _hookCollided = true;
                return true;
            }
            return false;
        }

        private IEnumerator RefillAndResetRoutine(Player player)
        {
            yield return RefillRoutine(player);
            outline.Position = Vector2.Zero;
            Position = _respawnPosition;
        }

        private bool _hookCollided = false;
        private Vector2 _respawnPosition;
        private MoveToward _moveToward;
    }
}
