using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Refill")]
    [Tracked(true)]
    public class AquaRefill : Refill
    {
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
                    dir = "objects/refill_two/";
                    animID = "RefillTwo";
                }
                else
                {
                    dir = "objects/refill_one/";
                    animID = "RefillOne";
                }
                outline.Texture = GFX.Game[dir + "outline"];
                GFX.SpriteBank.CreateOn(sprite, animID);
                GFX.SpriteBank.CreateOn(flash, animID + "Flash");
            }
            PlayerCollider old = Get<PlayerCollider>();
            old.OnCollide = OnPlayerCollide;
            Add(new HookInteractable(OnHookInteract));
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
                MoveToward com = Get<MoveToward>();
                if (com != null)
                    Remove(com);
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
                Add(new MoveToward(hook, 100000.0f, true));
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
    }
}
