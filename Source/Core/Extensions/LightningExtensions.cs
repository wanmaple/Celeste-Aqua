using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class LightningExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Lightning.ctor_Vector2_int_int_Nullable1_float += Lightning_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Lightning.ctor_Vector2_int_int_Nullable1_float -= Lightning_Construct;
        }

        private static void Lightning_Construct(On.Celeste.Lightning.orig_ctor_Vector2_int_int_Nullable1_float orig, Lightning self, Vector2 position, int width, int height, Vector2? node, float moveTime)
        {
            orig(self, position, width, height, node, moveTime);
            self.SetHookable(true);
            self.Add(new HookInOut(self.OnHookEnterElectricity, self.OnHookOutElectricity));
            self.Add(new ElectricShockInLightning());
        }
    }
}
