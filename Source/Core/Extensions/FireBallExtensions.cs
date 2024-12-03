using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class FireBallExtensions
    {
        public static void Initialize()
        {
            On.Celeste.FireBall.ctor_Vector2Array_int_int_float_float_bool += FireBall_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.FireBall.ctor_Vector2Array_int_int_float_float_bool -= FireBall_Construct;
        }

        private static void FireBall_Construct(On.Celeste.FireBall.orig_ctor_Vector2Array_int_int_float_float_bool orig, FireBall self, Vector2[] nodes, int amount, int index, float offset, float speedMult, bool notCoreMode)
        {
            orig(self, nodes, amount, index, offset, speedMult, notCoreMode);
            self.Add(new HookInteractable(self.OnHookInteract));
        }

        private static bool OnHookInteract(this FireBall self, GrapplingHook hook, Vector2 at)
        {
            if (self.iceMode)
            {
                hook.Revoke();
                return true;
            }
            return false;
        }
    }
}
