using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System.ComponentModel;

namespace Celeste.Mod.Aqua.Core
{
    public abstract class CustomRefill : Refill
    {
        public bool Hookable { get; protected set; }
        public int RefillCount { get; protected set; }
        public float RespawnTime { get; protected set; }
        public bool FillStamina { get; private set; }
        public string RefillSprite { get; protected set; }
        public string FlashSprite { get; protected set; }
        public string OutlineTexture { get; protected set; }
        public Color ParticleColor1 { get; protected set; }
        public Color ParticleColor2 { get; protected set; }
        public bool UseDefaultSprite { get; protected set; }
        public bool SyncHoldableContainer { get; protected set; }

        public Vector2 HoldableContainerOffset => _containerOffset;

        protected CustomRefill(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Hookable = data.Bool("hookable", true);
            RespawnTime = data.Float("respawn_time", 2.5f);
            FillStamina = data.Bool("fill_stamina", true);
            RefillSprite = data.Attr("refill_sprite");
            FlashSprite = data.Attr("flash_sprite");
            OutlineTexture = data.Attr("outline_texture");
            ParticleColor1 = data.HexColor("particle_color1", P_Shatter.Color);
            ParticleColor2 = data.HexColor("particle_color2", P_Shatter.Color2);
            UseDefaultSprite = data.Bool("use_default_sprite", true);
            SyncHoldableContainer = data.Bool("sync_holdable_container", false);
            this.SetHookable(Hookable);
            PlayerCollider com = Get<PlayerCollider>();
            com.OnCollide = OnPlayerCustom;
            Add(new HookInteractable(OnHookInteract));
            Add(_moveToward = new MoveToward(null, true, SyncHoldableContainer));
            _moveToward.Active = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetupSprite();
            SetupParticles();
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

        protected abstract void SetupSprite();
        protected abstract bool UseRefill(Player player);
        protected abstract bool RefillCondition(Player player);

        protected virtual void SetupParticles()
        {
            p_shatter = new ParticleType(P_Shatter)
            {
                Color = ParticleColor1,
                Color2 = ParticleColor2,
            };
            p_glow = new ParticleType(P_Glow)
            {
                Color = ParticleColor1,
                Color2 = ParticleColor2,
            };
            p_regen = new ParticleType(P_Regen)
            {
                Color = ParticleColor1,
                Color2 = ParticleColor2,
            };
        }

        protected virtual void OnPlayerCustom(Player player)
        {
            if (OnUseRefill(player))
            {
                Audio.Play(twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                Collidable = false;
                Add(new Coroutine(RefillRoutine(player)));
                respawnTimer = RespawnTime;
            }
        }

        private bool OnUseRefill(Player player)
        {
            float stamina = player.Stamina;
            if (UseRefill(player))
            {
                if (!FillStamina)
                    player.Stamina = stamina;
                if (SyncHoldableContainer)
                {
                    if (_container != null)
                    {
                        _container.Active = false;
                    }
                }
                return true;
            }
            return false;
        }

        protected virtual bool OnHookInteract(GrapplingHook hook, Vector2 at)
        {
            if (!Hookable)
                return false;

            Player player = hook.Owner;
            if (player != null && RefillCondition(player))
            {
                hook.Revoke();
                _moveToward.Target = hook;
                _moveToward.Active = true;
                return true;
            }
            return false;
        }

        private MoveToward _moveToward;
        private Vector2 _containerOffset;
        private Entity _container;
    }
}
