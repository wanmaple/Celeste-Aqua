using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class InvisibleBarrierExtensions
    {
        public static void Initialize()
        {
            On.Celeste.InvisibleBarrier.ctor_Vector2_float_float += InvisibleBarrier_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.InvisibleBarrier.ctor_Vector2_float_float -= InvisibleBarrier_Construct;
        }

        private static void InvisibleBarrier_Construct(On.Celeste.InvisibleBarrier.orig_ctor_Vector2_float_float orig, InvisibleBarrier self, Vector2 position, float width, float height)
        {
            orig(self, position, width, height);
            self.SetHookable(false);
        }
    }
}
