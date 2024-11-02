using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class SolidMovements
    {
        public static void Initialize()
        {
            On.Celeste.Solid.Awake += Solid_Awake;
            On.Celeste.Solid.Update += Solid_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Solid.Awake -= Solid_Awake;
            On.Celeste.Solid.Update -= Solid_Update;
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
