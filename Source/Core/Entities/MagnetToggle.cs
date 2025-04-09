using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Magnet Toggle")]
    public class MagnetToggle : Entity
    {
        public string Flag { get; private set; }
        public bool OneUse { get; private set; }
        public bool HoldableCanActivate { get; private set; }

        public MagnetToggle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(16f, 24f, -8f, -8f);
            Flag = data.Attr("flag");
            OneUse = data.Bool("one_use", false);
            HoldableCanActivate = data.Bool("holdable_can_activate", false);
            Add(new PlayerCollider(OnPlayer));
            if (HoldableCanActivate)
                Add(new HoldableCollider(OnHoldable));
            Add(_sprite = new Sprite());
            GFX.SpriteBank.CreateOn(_sprite, "Aqua_MagnetToggle");
            _sprite.Play("idle");
            _tickerCoolDown = new TimeTicker(COOL_DOWN, true);
            _useable = true;
            Add(_sound = new SoundSource());
            Depth = 2000;
        }

        public override void Update()
        {
            base.Update();
            _tickerCoolDown.Tick(Engine.DeltaTime);
        }

        private void OnPlayer(Player player)
        {
            OnToggle();
        }

        private void OnHoldable(Holdable holdable)
        {
            OnToggle();
        }

        private void OnToggle()
        {
            if (_useable && _tickerCoolDown.Check())
            {
                SceneAs<Level>().Session.SetFlag(Flag, true);
                _tickerCoolDown.Reset();
                if (!_flag)
                    _sound.Play("event:/game/09_core/switch_to_hot");
                else
                    _sound.Play("event:/game/09_core/switch_to_cold");
                _flag = !_flag;
                if (OneUse)
                {
                    Audio.Play("event:/game/09_core/switch_dies", Center);
                    _sprite.Play("corrupt");
                    _useable = false;
                }
                else
                {
                    _sprite.Play("toggle");
                }
            }
        }

        private TimeTicker _tickerCoolDown;
        private Sprite _sprite;
        private bool _useable;
        private SoundSource _sound;
        private bool _flag;

        private const float COOL_DOWN = 1.0f;
    }
}
