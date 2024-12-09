using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class SpringExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Spring.ctor_Vector2_Orientations_bool += Spring_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Spring.ctor_Vector2_Orientations_bool -= Spring_Construct;
        }

        private static void Spring_Construct(On.Celeste.Spring.orig_ctor_Vector2_Orientations_bool orig, Spring self, Vector2 position, Spring.Orientations orientation, bool playerCanUse)
        {
            orig(self, position, orientation, playerCanUse);

            self.Add(new HookInteractable(self.OnInteractHook));
        }

        private static bool OnInteractHook(this Spring self, GrapplingHook hook, Vector2 at)
        {
            if (hook.Bounce(self.GetBounceDirection(), AquaModule.Settings.HookSettings.BounceSpeedAddition))
            {
                self.BounceAnimate();
                return true;
            }
            return false;
        }

        private static Vector2 GetBounceDirection(this Spring self)
        {
            Vector2 direction = Vector2.Zero;
            switch (self.Orientation)
            {
                case Spring.Orientations.Floor:
                    direction = -Vector2.UnitY;
                    break;
                case Spring.Orientations.WallLeft:
                    direction = Vector2.UnitX;
                    break;
                case Spring.Orientations.WallRight:
                    direction = -Vector2.UnitX;
                    break;
                default:
                    break;
            }
            return direction;
        }
    }
}
