using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Filter")]
    public class GrappleFilter : Filter
    {
        public GrappleFilter(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }

        protected override bool CanCollide(Entity other)
        {
            if (other is Player player)
            {
                return true;
            }
            else if (other is Platform)
            {
                return CollideSolids;
            }
            return true;
        }
    }
}
