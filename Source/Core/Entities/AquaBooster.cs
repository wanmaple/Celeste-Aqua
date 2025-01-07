using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Booster")]
    [Tracked(false)]
    public class AquaBooster : Booster
    {
        public static ParticleType P_BurstOrange;
        public static ParticleType P_BurstPurple;
        public static ParticleType P_AppearOrange;
        public static ParticleType P_AppearPurple;

        public bool Hookable { get; private set; }

        public AquaBooster(Vector2 position, bool red, bool hookable, string skin)
            : base(position, red)
        {
            Hookable = hookable;
            if (Hookable)
            {
                string spriteName = skin;
                if (red)
                {
                    if (!GFX.SpriteBank.Has(skin))
                    {
                        spriteName = "Aqua_BoosterPurple";
                    }
                    GFX.SpriteBank.CreateOn(sprite, spriteName);
                    particleType = P_BurstPurple;
                }
                else
                {
                    if (!GFX.SpriteBank.Has(skin))
                    {
                        spriteName = "Aqua_BoosterOrange";
                    }
                    GFX.SpriteBank.CreateOn(sprite, spriteName);
                    particleType = P_BurstOrange;
                }
                HookInteractable com = Get<HookInteractable>();
                com.Interaction = OnHookGrab;
                PlayerCollider com2 = Get<PlayerCollider>();
                com2.OnCollide = OnPlayerEx;
                Add(_moveToward = new MoveToward(null, 0.0f, true));
                _moveToward.Active = false;
            }
            else if (GFX.SpriteBank.Has(skin))
            {
                GFX.SpriteBank.CreateOn(sprite, skin);
            }
        }

        public AquaBooster(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Bool("red"), data.Bool("hookable"), data.Attr("sprite", string.Empty))
        {
        }

        public override void Update()
        {
            base.Update();
            Collidable = respawnTimer <= 0.0f;
        }

        private bool OnHookGrab(GrapplingHook hook, Vector2 at)
        {
            if (!BoostingPlayer)
            {
                Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
                hook.Revoke();
                _moveToward.Target = hook;
                _moveToward.BaseSpeed = 100000.0f;
                _moveToward.Active = true;
                _grabbing = true;
                return true;
            }
            return false;
        }

        private void OnPlayerEx(Player player)
        {
            bool condition = respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer;
            if (condition)
            {
                if (Hookable && _grabbing)
                {
                    Position = player.Center;
                }
            }
            base.OnPlayer(player);
            if (condition)
            {
                _moveToward.Active = false;
                _grabbing = false;
                BeginBoosting();
            }
        }

        protected virtual void BeginBoosting()
        {

        }

        private MoveToward _moveToward;
        private bool _grabbing;
    }
}
