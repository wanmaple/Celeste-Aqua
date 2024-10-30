using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public enum Cornors
    {
        Free,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public struct RopePivot
    {
        public Vector2 point;
        public Cornors direction;
        public Entity entity;

        public RopePivot(Vector2 pt, Cornors cn, Entity e = null)
        {
            point = pt;
            direction = cn;
            entity = e;
        }

        public Vector2 OffsetPoint(float offset)
        {
            Vector2 off = Vector2.Zero;
            switch (direction)
            {
                case Cornors.TopLeft:
                    off = new Vector2(-1.0f, -1.0f);
                    break;
                case Cornors.TopRight:
                    off = new Vector2(1.0f, -1.0f);
                    break;
                case Cornors.BottomLeft:
                    off = new Vector2(-1.0f, 1.0f);
                    break;
                case Cornors.BottomRight:
                    off = new Vector2(1.0f, 1.0f);
                    break;
            }
            return point + off * offset;
        }
    }
}
