using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class HookSprite : Sprite
    {
        public const string Emit = "emit";
        public const string Revoke = "revoke";
        public const string Hit = "hit";

        public enum HookSpriteMode
        {
            Default,
            Playback,
        }

        public HookSprite(HookSpriteMode mode)
            : base(null, null)
        {
            string spriteName = "Aqua_Hook";
            switch (mode)
            {
                case HookSpriteMode.Playback:
                    spriteName = "Aqua_HookPlayback";
                    break;
            }
            GFX.SpriteBank.CreateOn(this, spriteName);
        }
    }
}
