using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Aqua.Rendering
{
    public interface ICustomRenderEntity
    {
        Effect GetEffect();
        void OnReload();
    }
}
