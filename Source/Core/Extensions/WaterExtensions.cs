using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class WaterExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Water.ctor_Vector2_bool_bool_float_float += Water_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Water.ctor_Vector2_bool_bool_float_float -= Water_Construct;
        }

        private static void Water_Construct(On.Celeste.Water.orig_ctor_Vector2_bool_bool_float_float orig, Water self, Vector2 position, bool topSurface, bool bottomSurface, float width, float height)
        {
            orig(self, position, topSurface, bottomSurface, width, height);
            self.SetHookable(true);
        }
    }
}
