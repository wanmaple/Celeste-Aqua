using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class SolidExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Solid.ctor += Solid_Construct;
            On.Celeste.Solid.Awake += Solid_Awake;
            On.Celeste.Solid.Update += Solid_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Solid.ctor -= Solid_Construct;
            On.Celeste.Solid.Awake -= Solid_Awake;
            On.Celeste.Solid.Update -= Solid_Update;
        }

        private static void Solid_Construct(On.Celeste.Solid.orig_ctor orig, Solid self, Vector2 position, float width, float height, bool safe)
        {
            orig(self, position, width, height, safe);
            DynamicData.For(self).Set("hookable", true);
        }

        private static void Solid_Awake(On.Celeste.Solid.orig_Awake orig, Solid self, Monocle.Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("prev_position", self.Position);
        }

        private static void Solid_Update(On.Celeste.Solid.orig_Update orig, Solid self)
        {
            DynamicData.For(self).Set("prev_position", self.Position);
            orig(self);
        }

        public static Vector2 GetPreviousPosition(this Solid self)
        {
            return DynamicData.For(self).Get<Vector2>("prev_position");
        }
    }
}
