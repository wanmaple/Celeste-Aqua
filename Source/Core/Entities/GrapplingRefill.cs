using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grappling Refill")]
    [Tracked(true)]
    public class GrapplingRefill : Entity
    {
        public static ParticleType P_Shatter;
        public static ParticleType P_Glow;
        public static ParticleType P_Regen;
        public static ParticleType P_Shatter2;
        public static ParticleType P_Glow2;
        public static ParticleType P_Regen2;

        public bool OneUse { get; private set; }
        public bool ChargeTwo { get; private set; }

        public GrapplingRefill(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(16.0f, 16.0f, -8.0f, -8.0f);
            _respawnPosition = Position;
            _respawnTicker = new TimeTicker(2.5f);
            OneUse = data.Bool("oneUse");
            ChargeTwo = data.Bool("chargeTwo");
            string outline = "objects/refills/refill_Hook/outline";
            string spriteName = "Aqua_HookRefill";
            string spriteFlashName = "Aqua_HookRefillFlash";
            if (ChargeTwo)
            {
                outline= "objects/refills/refillTwo_Hook/outline";
                spriteName = "Aqua_HookRefillTwo";
                spriteFlashName = "Aqua_HookRefillTwoFlash";
            }
            Add(_outline = new Image(GFX.Game[outline]));
            _outline.CenterOrigin();
            _outline.Visible = false;
            Add(_sprite = new Sprite());
            GFX.SpriteBank.CreateOn(_sprite, spriteName);
            Add(_flash = new Sprite());
            GFX.SpriteBank.CreateOn(_flash, spriteFlashName);
            _flash.OnFinish = delegate
            {
                _flash.Visible = false;
            };
            Add(_wiggler = Wiggler.Create(1.0f, 4.0f, delegate (float v)
            {
                _sprite.Scale = _flash.Scale = Vector2.One * (1.0f + v * 0.2f);
            }));
            Add(new MirrorReflection());
            Add(_bloom = new BloomPoint(0.8f, 16.0f));
            Add(_light = new VertexLight(Color.White, 1.0f, 16, 48));
            Add(_sine = new SineWave(0.6f, 0.0f));
            _sine.Randomize();
            UpdateY();
            Add(new PlayerCollider(OnPlayer));
            Add(new HookInteractable(OnHookInteract));
            Add(_moveToward = new MoveToward(null, 0.0f, true));
            _moveToward.Active = false;
            this.SetHookable(true);
            Depth = -100;
        }

        public override void Update()
        {
            base.Update();
            float dt = Engine.DeltaTime;
            Level level = SceneAs<Level>();
            if (!Collidable)
            {
                _respawnTicker.Tick(dt);
                if (_respawnTicker.Check())
                {
                    Respawn();
                }
            }
            else if (Scene.OnInterval(0.1f))
            {
                level.ParticlesFG.Emit(ChargeTwo ? P_Glow2 : P_Glow, 1, Position, Vector2.One * 5.0f);
            }

            UpdateY();
            _light.Alpha = Calc.Approach(_light.Alpha, _sprite.Visible ? 1.0f : 0.0f, 4.0f * Engine.DeltaTime);
            _bloom.Alpha = _light.Alpha * 0.8f;
            if (Scene.OnInterval(2.0f) && _sprite.Visible)
            {
                _flash.Play("flash", restart: true);
                _flash.Visible = true;
            }
        }

        public override void Render()
        {
            if (_sprite.Visible)
            {
                _sprite.DrawOutline();
            }
            base.Render();
        }

        private void Respawn()
        {
            if (!Collidable)
            {
                Collidable = true;
                _sprite.Visible = true;
                _outline.Visible = false;
                Depth = -100;
                _wiggler.Start();
                Audio.Play("event:/game/general/diamond_return", Position);
                SceneAs<Level>().ParticlesFG.Emit(ChargeTwo ? P_Regen2 : P_Regen, 16, Position, Vector2.One * 2.0f);
            }
        }

        private void UpdateY()
        {
            _flash.Y = _sprite.Y = _bloom.Y = _sine.Value * 2.0f;
        }

        private void OnPlayer(Player player)
        {
            var hook = player.GetGrappleHook();
            if (hook.Mode == GrapplingHook.GameplayMode.ShootCounter)
            {
                SceneAs<Level>().GetState().RestShootCount += (ChargeTwo ? 2 : 1);
            }
            Audio.Play("event:/game/general/diamond_touch", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            Collidable = false;
            _outline.Position = _respawnPosition - Position;
            Add(new Coroutine(RefillAndResetRoutine(player)));
            _respawnTicker.Reset();
            _hookCollided = false;
            _moveToward.Active = false;
        }

        private bool OnHookInteract(GrapplingHook hook, Vector2 at)
        {
            if (_hookCollided || hook.Mode != GrapplingHook.GameplayMode.ShootCounter)
                return false;

            hook.Revoke();
            _moveToward.Target = hook;
            _moveToward.BaseSpeed = 100000.0f;
            _moveToward.Active = true;
            _hookCollided = true;
            return true;
        }

        private IEnumerator RefillAndResetRoutine(Player player)
        {
            Celeste.Freeze(0.05f);
            yield return null;
            Level level = SceneAs<Level>();
            level.Shake();
            _sprite.Visible = _flash.Visible = false;
            if (!OneUse)
            {
                _outline.Visible = true;
            }

            Depth = 8999;
            yield return 0.05f;
            float num = player.Speed.Angle();
            level.ParticlesFG.Emit(ChargeTwo ? P_Shatter2 : P_Shatter, 5, Position, Vector2.One * 4f, num - MathF.PI / 2f);
            level.ParticlesFG.Emit(ChargeTwo ? P_Shatter2 : P_Shatter, 5, Position, Vector2.One * 4f, num + MathF.PI / 2f);
            SlashFx.Burst(Position, num);
            if (OneUse)
            {
                RemoveSelf();
            }
            _outline.Position = Vector2.Zero;
            Position = _respawnPosition;
        }

        private Sprite _sprite;
        private Sprite _flash;
        private Image _outline;
        private Wiggler _wiggler;
        private BloomPoint _bloom;
        private VertexLight _light;
        private SineWave _sine;
        private MoveToward _moveToward;
        private Vector2 _respawnPosition;
        private bool _hookCollided;
        private TimeTicker _respawnTicker;
    }
}
