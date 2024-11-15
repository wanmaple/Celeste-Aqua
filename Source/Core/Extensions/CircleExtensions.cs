using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public static class CircleExtensions
    {
        private const float SEGMENT_SIZE = 4.0f;
        private const int SIMULATE_SEGMENTS = 16;

        public static List<RopePivot> SimulatePivots(this Circle self)
        {
            List<RopePivot> results;
            float radius = self.Radius;
            if (_cache.TryGetValue(radius, out results))
            {
                return results;
            }

            results = new List<RopePivot>(32);
            float segmentAngle = MathF.PI * 2.0f / SIMULATE_SEGMENTS;
            for (int i = 0; i < SIMULATE_SEGMENTS; ++i)
            {
                float angle = segmentAngle * (0.5f + i);
                float x = MathF.Sin(angle) * radius;
                float y = MathF.Cos(angle) * radius;
                Cornors cornor = Cornors.BottomRight;
                if (angle > MathF.PI * 1.5f)
                {
                    cornor = Cornors.BottomLeft;
                }
                else if (angle > MathF.PI)
                {
                    cornor = Cornors.TopLeft;
                }
                else if (angle > MathF.PI * 0.5f)
                {
                    cornor = Cornors.TopRight;
                }
                Vector2 pt = self.Position + new Vector2(x, y);
                results.Add(new RopePivot(pt, cornor));
            }
            //int segmentsHalfSide = (int)MathF.Ceiling(radius / SEGMENT_SIZE);
            //float segmentSize = radius / segmentsHalfSide;
            //for (int i = 0; i < segmentsHalfSide; ++i)
            //{
            //    float x = 0.5f * segmentSize + i * segmentSize;
            //    float y = MathF.Sqrt(radius * radius - x * x);
            //    Vector2 tr = self.Position + new Vector2(x, -y);
            //    Vector2 br = self.Position + new Vector2(x, y);
            //    Vector2 tl = self.Position + new Vector2(-x, -y);
            //    Vector2 bl = self.Position + new Vector2(-x, y);
            //    results.Add(new RopePivot(tr, Cornors.TopRight));
            //    results.Add(new RopePivot(br, Cornors.BottomRight));
            //    results.Add(new RopePivot(tl, Cornors.TopLeft));
            //    results.Add(new RopePivot(bl, Cornors.BottomLeft));
            //}
            _cache.Add(radius, results);
            return results;
        }

        private static Dictionary<float, List<RopePivot>> _cache = new Dictionary<float, List<RopePivot>>(8);
    }
}
