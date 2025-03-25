using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class ColliderExtensions
    {
        public static bool CheckLineWithoutEdge(this Collider self, Vector2 pt1, Vector2 pt2)
        {
            if (self is Grid)
            {
                return (self as Grid).CheckLineNotOnEdge(pt1, pt2);
            }
            else if (self is Hitbox)
            {
                return (self as Hitbox).CheckLineNotOnEdge(pt1, pt2);
            }
            return false;
        }
    }
}
