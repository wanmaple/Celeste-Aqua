using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class PresentationPlayer : Entity
    {
        public PresentationPlayer CreateTrail(float lifetime)
        {
            var clone = new PresentationPlayer(lifetime);
            clone._hair.Border = Color.Transparent;
            clone._hair.DrawPlayerSpriteOutline = false;
            clone._hair.SimulateMotion = false;
            clone._sprite.Color = clone._hair.Color = _sprite.Color;
            if (_sprite.Has(_sprite.CurrentAnimationID))
            {
                _sprite.Play(_sprite.CurrentAnimationID);
                clone._sprite.CurrentAnimationFrame = _sprite.CurrentAnimationFrame;
                _sprite.Stop();
            }
            clone.Position = Position;
            clone._sprite.Scale = _sprite.Scale;
            clone._sprite.HairCount = _sprite.HairCount;
            clone._sprite.RenderPosition = _sprite.RenderPosition;
            clone._hair.Facing = _hair.Facing;
            for (int i = 0; i < _hair.Nodes.Count; i++)
            {
                clone._hair.Nodes[i] = _hair.Nodes[i];
            }
            return clone;
        }

        public PresentationPlayer(float lifetime)
        {
            _lifetime = lifetime;
            _sprite = new PlayerSprite(PlayerSpriteMode.Playback);
            Add(_hair = new PlayerHair(_sprite));
            _hair.Color = Player.NormalHairColor;
            Add(_sprite);
            if (_lifetime > 0.0f)
            {
                Depth = 1;
            }
        }

        private PresentationPlayer()
        { }

        public override void Update()
        {
            base.Update();
            if (_lifetime > 0.0f)
            {
                _elapsed += Engine.DeltaTime;
                float t = Calc.Clamp(_elapsed / _lifetime, 0.0f, 1.0f);
                Color spriteColor = _sprite.Color;
                spriteColor.A = (byte)((1.0f - t) * 255);
                _hair.Alpha = spriteColor.A / 255.0f;
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
            _hair.Facing = (Facings)frame.HairFacing;
        }

        public void SetColor(Color color)
        {
            _sprite.Color = color;
            _hair.Color = color;
        }

        private PlayerSprite _sprite;
        private PlayerHair _hair;
        private float _lifetime;
        private float _elapsed = 0.0f;
    }
}
