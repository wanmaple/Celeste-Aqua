using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Seeker Explode Barrier")]
    [Tracked(false)]
    public class SeekerExplodeBarrier : SeekerBarrier
    {
        public SeekerExplodeBarrier(EntityData data, Vector2 offset)
            : base(data, offset)
        { }

        public override void Render()
        {
            if (!CullHelper.IsRectangleVisible(Position.X, Position.Y, Width, Height))
            {
                return;
            }

            Color color = Color.Red * 0.5f;
            foreach (Vector2 particle in particles)
            {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
            }

            if (Flashing)
            {
                Draw.Rect(Collider, Color.White * Flash * 0.5f);
            }
        }
    }
}
