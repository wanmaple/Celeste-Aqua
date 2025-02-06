using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class SeekerBarrierExtensions
    {
        public static void Initialize()
        {
            On.Celeste.SeekerBarrier.ctor_Vector2_float_float += SeekerBarrier_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.SeekerBarrier.ctor_Vector2_float_float -= SeekerBarrier_Construct;
        }

        private static void SeekerBarrier_Construct(On.Celeste.SeekerBarrier.orig_ctor_Vector2_float_float orig, SeekerBarrier self, Vector2 position, float width, float height)
        {
            orig(self, position, width, height);
            self.SetHookable(false);
        }
    }
}
