using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class ExtraSpeedComponent : Component
    {
        public float ExtraSpeed { get; set; }

        public ExtraSpeedComponent()
            : base(false, false)
        {
        }
    }
}
