using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Slidable Ice Block")]
    public class SlidableIceBlock : SlidableSolid
    {
        public SlidableIceBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        { }

        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.White);
        }
    }
}
