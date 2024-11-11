namespace Celeste.Mod.Aqua.Core.Extensions
{
    public static class CoreModeToggleExtensions
    {
        public static void Initialize()
        {
            On.Celeste.CoreModeToggle.ctor_Vector2_bool_bool_bool += CoreModeToggle_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.CoreModeToggle.ctor_Vector2_bool_bool_bool -= CoreModeToggle_Construct;
        }

        private static void CoreModeToggle_Construct(On.Celeste.CoreModeToggle.orig_ctor_Vector2_bool_bool_bool orig, CoreModeToggle self, Microsoft.Xna.Framework.Vector2 position, bool onlyFire, bool onlyIce, bool persistent)
        {
            orig(self, position, onlyFire, onlyIce, persistent);

            self.Add(new HookCollider(self.OnGrapplingHook));
        }

        private static void OnGrapplingHook(this CoreModeToggle self, GrapplingHook hook)
        {
            self.OnPlayer(null);
        }
    }
}
