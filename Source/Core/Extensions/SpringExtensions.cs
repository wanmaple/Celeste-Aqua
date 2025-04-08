using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;

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
            self.SetHookable(true);
            self.Add(new HookInteractable(self.OnInteractHook));
        }

        private static bool OnInteractHook(this Spring self, GrapplingHook hook, Vector2 at)
        {
            if (hook.Bounce(self.GetBounceDirection(), GrapplingHook.BOUNCE_SPEED_ADDITION))
            {
                // GravitySpring doesn't use the default bounce animation.
                if (ModInterop.SpringTypes.Count > 0 && ModInterop.SpringTypes.Any(type => self.GetType().IsAssignableTo(type)))
                {
                    if (!self.GetType().CallMethodIfExist(self, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, "bounceAnimate", out object nouse))
                    {
                        self.BounceAnimate();
                    }
                }
                else
                {
                    self.BounceAnimate();
                }
                return true;
            }
            return false;
        }

        private static Vector2 GetBounceDirection(this Spring self)
        {
            Vector2 direction = Vector2.Zero;
            if (ModInterop.SpringTypes.Count > 0 && ModInterop.SpringTypes.Any(type => self.GetType().IsAssignableTo(type)))
            {
                FieldInfo fieldOrientation = self.GetType().FindField(BindingFlags.Instance | BindingFlags.NonPublic, "_ourOrientation");   // This is Momentum Spring's orientation name.
                object value = null;
                if (fieldOrientation != null)
                {
                    value = fieldOrientation.GetValue(self);
                }
                if (value == null)
                {
                    value = self.GetType().GetFieldOrPropertyValue(self, BindingFlags.Public | BindingFlags.Instance, "Orientation");
                }
                int orientation = (int)value;
                switch (orientation)
                {
                    case 0:
                        direction = -Vector2.UnitY;
                        break;
                    case 1:
                        direction = Vector2.UnitX;
                        break;
                    case 2:
                        direction = -Vector2.UnitX;
                        break;
                    case 3:
                        direction = Vector2.UnitY;
                        break;
                }
            }
            else
            {
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
            }
            return direction;
        }
    }
}
