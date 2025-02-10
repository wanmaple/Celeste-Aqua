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

        public MagnetToggle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Flag = data.Attr("flag");
            OneUse = data.Bool("one_use", false);
            Add(new PlayerCollider(OnPlayer));
            Add(_sprite = new Sprite());
            _tickerCoolDown = new TimeTicker(COOL_DOWN, true);
            _useable = true;
            Depth = 2000;
            _shatter = new ParticleType(Refill.P_Shatter)
            {
            };
        }

        public override void Update()
        {
            base.Update();
            _tickerCoolDown.Tick(Engine.DeltaTime);
        }

        private void OnPlayer(Player player)
        {
            if (_useable && _tickerCoolDown.Check())
            {
                SceneAs<Level>().Session.SetFlag(Flag, true);
                _tickerCoolDown.Reset();
                if (OneUse)
                {
                    Audio.Play("event:/game/09_core/switch_dies", Center);
                    _useable = false;
                }
                else
                {
                    Audio.Play("event:/game/09_core/switch_to_cold", Center);
                }
            }
        }

        private TimeTicker _tickerCoolDown;
        private Sprite _sprite;
        private bool _useable;
        private ParticleType _shatter;

        private const float COOL_DOWN = 1.0f;
    }
}
