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
        public bool OneUse { get; protected set; }
        public Color ParticleColor { get; protected set; }
        public ParticleType AppearParticle { get; protected set; }
        public bool UseDefaultSprite { get; protected set; }
        public bool SyncHoldableContainer { get; protected set; }

        public Vector2 HoldableContainerOffset => _containerOffset;

        public AquaBooster(Vector2 position, bool red, bool hookable, string skin)
            : base(position, red)
        {
        }

        public AquaBooster(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Bool("red"))
        {
            Hookable = data.Bool("hookable", true);
            OneUse = data.Bool("one_use");
            string skin = data.Attr("sprite");
            ParticleColor = data.HexColor("particle_color");
            UseDefaultSprite = data.Bool("use_default_sprite", true);
            SyncHoldableContainer = data.Bool("sync_holdable_container", false);
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
                Add(_moveToward = new MoveToward(null, true, SyncHoldableContainer));
                _moveToward.Active = false;
            }
        }

        public override void Removed(Scene scene)
        {
            if (SyncHoldableContainer)
            {
                if (respawnTimer > 0.0f && _container != null)
                {
                    _container.Active = false;
                }
            }
            base.Removed(scene);
            outline.RemoveSelf();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (SyncHoldableContainer)
            {
                _container = this.GetHoldableContainer();
                _containerOffset = _container != null ? _container.Position - Position : Vector2.Zero;
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
            if (SyncHoldableContainer)
            {
                if (_container != null)
                {
                    _container.Active = false;
                }
            }
        }

        private MoveToward _moveToward;
        private bool _grabbing;
        private Vector2 _containerOffset;
        private Entity _container;
    }
}
