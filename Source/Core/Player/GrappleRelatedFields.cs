using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class GrappleRelatedFields : Component
    {
        public bool HasFixingSpeed { get; set; }
        public Vector2 FixingSpeed { get; set; }

        public GrappleRelatedFields()
            : base(false, false)
        { }
    }
}
