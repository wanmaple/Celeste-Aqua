using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlatformExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Platform.ctor += Platform_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Platform.ctor -= Platform_Construct;
        }

        private static void Platform_Construct(On.Celeste.Platform.orig_ctor orig, Platform self, Microsoft.Xna.Framework.Vector2 position, bool safe)
        {
            orig(self, position, safe);
            DynamicData.For(self).Set("hookable", true);
        }
    }
}
