using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlatformExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Platform.ctor += Platform_Construct;
            On.Celeste.Platform.Update += Platform_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Platform.ctor -= Platform_Construct;
            On.Celeste.Platform.Update -= Platform_Update;
        }

        private static void Platform_Construct(On.Celeste.Platform.orig_ctor orig, Platform self, Vector2 position, bool safe)
        {
            orig(self, position, safe);
            DynamicData.For(self).Set("hookable", true);
        }

        private static void Platform_Update(On.Celeste.Platform.orig_Update orig, Platform self)
        {
            DynamicData.For(self).Set("prev_position", self.Position);
            orig(self);
        }
    }
}
