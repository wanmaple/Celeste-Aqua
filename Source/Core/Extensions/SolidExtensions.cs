namespace Celeste.Mod.Aqua.Core
{
    public static class SolidExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider;
        }

        public static void Uninitialize()
        {
            On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider;
        }

        private static bool Solid_HasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self)
        {
            if (!orig(self))
            {
                if (!self.IsHookAttached())
                    return false;
            }
            return true;
        }
    }
}
