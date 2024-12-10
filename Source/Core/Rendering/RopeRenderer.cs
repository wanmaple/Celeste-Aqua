using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class RopeRenderer
    {
        public bool ElectricShocking { get; set; }

        public RopeRenderer(MTexture texture)
        {
            _texture = texture;
            _elecShockTextures = new MTexture[5];
            for (int i = 0; i <= 4; ++i)
            {
                _elecShockTextures[i] = GFX.Game["objects/hook/rope_charge" + i.ToString()];
            }
        }

        public void Update(float dt)
        {
            _elapsed += dt;
            _elecShockIndex = (int)MathF.Floor(_elapsed / 0.1f) % 5;
        }

        public void Render(IReadOnlyList<Segment> segments)
        {
            if (_texture != null)
            {
                RenderSteps(segments, _texture);
            }
            if (ElectricShocking)
            {
                MTexture texElecShock = _elecShockTextures[_elecShockIndex];
                if (texElecShock != null)
                {
                    RenderSteps(segments, texElecShock);
                }
            }
        }

        private void RenderSteps(IReadOnlyList<Segment> segments, MTexture texture)
        {
            int step = texture.Width;
            Vector2 origin = new Vector2(0.0f, texture.Height * 0.5f);
            int start = 0;
            for (int i = 0; i < segments.Count; i++)
            {
                Segment seg = segments[i];
                float length = seg.Length;
                Vector2 position = seg.Point1;
                Vector2 direction = seg.Direction;
                float angle = direction.Angle();
                while (length > 0.0f)
                {
                    Rectangle rect = new Rectangle(start, 0, Math.Min((int)MathF.Ceiling(length), step), texture.Height);
                    texture.Draw(position, origin, Color.White, Vector2.One, angle, rect);
                    position += direction * step;
                    length -= step;
                }
            }
        }

        private float _elapsed;
        private MTexture _texture;
        private int _elecShockIndex;
        private MTexture[] _elecShockTextures;
    }
}
