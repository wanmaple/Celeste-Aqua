using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class SpringExtensions
    {
        public static Vector2 GetBounceDirection(this Spring self)
        {
            Vector2 direction = Vector2.Zero;
            switch (self.Orientation)
            {
                case Spring.Orientations.Floor:
                    direction = -Vector2.UnitY;
                    break;
                case Spring.Orientations.WallLeft:
                    direction = Vector2.UnitX;
                    break;
                case Spring.Orientations.WallRight:
                    direction = -Vector2.UnitX;
                    break;
                default:
                    break;
            }
            return direction;
        }
    }
}
