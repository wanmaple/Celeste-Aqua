using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public interface IBarrierRenderable
    {
        Vector2 Position { get; }
        Collider Collider { get; }
        Color Color { get; }
        float Flash { get; }
        float Solidify { get; }
        float X { get; }
        float Y { get; }
        float Right { get; }
        float Bottom { get; }
        float Width { get; }
        float Height { get; }
        bool Visible { get; }
    }
}
