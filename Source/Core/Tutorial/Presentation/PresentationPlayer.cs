using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class PresentationPlayer : Entity
    {
        public PresentationPlayer(float lifetime)
        {
            _lifetime = lifetime;
            _sprite = new PlayerSprite(PlayerSpriteMode.Playback);
            if (_lifetime <= 0.0f)
            {
                Add(_hair = new PlayerHair(_sprite));
                _hair.Color = Player.NormalHairColor;
            }
            Add(_sprite);
            if (_lifetime > 0.0f)
            {
                Depth = 1;
            }
        }

        public override void Update()
        {
            base.Update();
            if (_lifetime > 0.0f)
            {
                _elapsed += Engine.DeltaTime;
                float t = Calc.Clamp(_elapsed / _lifetime, 0.0f, 1.0f);
                Color spriteColor = _sprite.Color;
                spriteColor.A = (byte)((1.0f - t) * 255);
                _sprite.Color = spriteColor;
                if (t >= 1.0f)
                {
                    RemoveSelf();
                }
            }
        }

        public void Apply(PlayerFrameData frame)
        {
            if (_sprite.Has(frame.AnimationID))
            {
                _sprite.Play(frame.AnimationID);
                _sprite.CurrentAnimationFrame = frame.AnimationFrame;
                _sprite.Stop();
            }
            else
            {
                AquaDebugger.LogInfo("ANIMATION ID: {0} NOT EXIST.", frame.AnimationID);
            }
            Position = frame.Position;
            _sprite.Scale = frame.Scale;
            _sprite.Scale.X *= frame.Facing;
            _sprite.Scale.Y *= frame.Gravity;
            _sprite.HairCount = frame.HairCount;
            _sprite.RenderPosition = frame.RenderPosition;
            if (_hair != null)
                _hair.Facing = (Facings)frame.HairFacing;
        }

        public void SetColor(Color color)
        {
            _sprite.Color = color;
            if (_hair != null)
                _hair.Color = color;
        }

        private PlayerSprite _sprite;
        private PlayerHair _hair;
        private float _lifetime;
        private float _elapsed = 0.0f;
    }
}
