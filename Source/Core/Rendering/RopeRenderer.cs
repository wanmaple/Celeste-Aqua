using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class RopeRenderer
    {
        public RopeRenderer(MTexture texture)
        {
            _texture = texture;
        }

        public void Render(IReadOnlyList<Segment> segments)
        {
            if (_texture == null) return;

            int step = _texture.Width;
            Vector2 origin = new Vector2(0.0f, _texture.Height * 0.5f);
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
                    Rectangle rect = new Rectangle(start, 0, Math.Min((int)MathF.Ceiling(length), step), _texture.Height);
                    _texture.Draw(position, origin, Color.White, Vector2.One, angle, rect);
                    position += direction * step;
                    length -= step;
                }
            }
        }

        private MTexture _texture;
    }
}
