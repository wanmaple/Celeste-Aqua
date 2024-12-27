using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class HookSprite : Sprite
    {
        public const string Emit = "emit";
        public const string Revoke = "revoke";
        public const string Hit = "hit";

        public HookSprite()
            : base(null, null)
        {
            GFX.SpriteBank.CreateOn(this, "Aqua_Hook");
        }
    }
}
