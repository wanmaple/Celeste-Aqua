using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityExtensions
    {
        public static void Initialize()
        {
            On.Monocle.Entity.ctor += Entity_Construct;
        }

        public static void Uninitialize()
        {
            On.Monocle.Entity.ctor -= Entity_Construct;
        }

        private static void Entity_Construct(On.Monocle.Entity.orig_ctor orig, Monocle.Entity self)
        {
            orig(self);
            DynamicData.For(self).Set("hookable", false);
        }

        public static bool IsHookable(this Entity self)
        {
            return DynamicData.For(self).Get<bool>("hookable");
        }
    }
}
