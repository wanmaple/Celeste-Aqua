using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Rendering
{
    public static class RenderHook
    {
        public static void Initialize()
        {
            On.Monocle.Engine.Draw += Engine_Draw;
        }

        public static void Uninitialize()
        {
            On.Monocle.Engine.Draw -= Engine_Draw;
        }

        private static void Engine_Draw(On.Monocle.Engine.orig_Draw orig, Engine self, GameTime gameTime)
        {
            orig(self, gameTime);
        }
    }
}
