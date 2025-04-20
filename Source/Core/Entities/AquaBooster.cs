using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Booster")]
    [Tracked(false)]
    public class AquaBooster : Booster
    {
        public bool Hookable { get; protected set; }
        public Color ParticleColor { get; protected set; }
        public ParticleType AppearParticle { get; protected set; }
        public bool UseDefaultSprite { get; protected set; }

        public AquaBooster(Vector2 position, bool red, bool hookable, string skin)
            : base(position, red)
        {
        }

        public AquaBooster(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Bool("red"))
        {
            Hookable = data.Bool("hookable");
            string skin = data.Attr("sprite");
            ParticleColor = data.HexColor("particle_color");
            UseDefaultSprite = data.Bool("use_default_sprite", true);
            string spriteName = skin;
            if (UseDefaultSprite || !GFX.SpriteBank.Has(skin))
            {
                if (Hookable)
                {
                    spriteName = red ? "Aqua_BoosterPurple" : "Aqua_BoosterOrange";
                    GFX.SpriteBank.CreateOn(sprite, spriteName);
                    Color color = red ? Calc.HexToColor("760ebc") : Calc.HexToColor("bc630e");
                    particleType = !red ? new ParticleType(P_Burst) { Color = color, } : new ParticleType(P_BurstRed) { Color = color, };
                    AppearParticle = !red ? new ParticleType(P_Appear) { Color = color, } : new ParticleType(P_RedAppear) { Color = color, };
                }
                else
                {
                    AppearParticle = !red ? P_Appear : P_RedAppear;
                }
            }
            else
            {
                GFX.SpriteBank.CreateOn(sprite, skin);
                particleType = !red ? new ParticleType(P_Burst) { Color = ParticleColor, } : new ParticleType(P_BurstRed) { Color = ParticleColor, };
                AppearParticle = !red ? new ParticleType(P_Appear) { Color = ParticleColor, } : new ParticleType(P_RedAppear) { Color = ParticleColor, };
            }
            if (Hookable)
            {
                HookInteractable com = Get<HookInteractable>();
                com.Interaction = OnHookGrab;
                PlayerCollider com2 = Get<PlayerCollider>();
                com2.OnCollide = OnPlayerEx;
                Add(_moveToward = new MoveToward(null, true));
                _moveToward.Active = false;
            }
        }

        public override void Update()
        {
            base.Update();
            Collidable = respawnTimer <= 0.0f;
        }

        public virtual void OnTransport()
        {
            _grabbing = false;
        }

        private bool OnHookGrab(GrapplingHook hook, Vector2 at)
        {
            if (!BoostingPlayer)
            {
                Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
                hook.Revoke();
                _moveToward.Target = hook;
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
