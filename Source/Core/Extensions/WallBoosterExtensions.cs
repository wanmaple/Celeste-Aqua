using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class WallBoosterExtensions
    {
        public static void Initialize()
        {
            On.Celeste.WallBooster.ctor_Vector2_float_bool_bool += WallBooster_Construct;
            On.Celeste.WallBooster.Update += WallBooster_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.WallBooster.ctor_Vector2_float_bool_bool -= WallBooster_Construct;
            On.Celeste.WallBooster.Update -= WallBooster_Update;
        }

        private static void WallBooster_Construct(On.Celeste.WallBooster.orig_ctor_Vector2_float_bool_bool orig, WallBooster self, Vector2 position, float height, bool left, bool notCoreMode)
        {
            orig(self, position, height, left, notCoreMode);
            self.SetHookable(true);
            DynamicData.For(self).Set("post_move_patch", (Action<Vector2>)self.PostMove);
        }

        private static void WallBooster_Update(On.Celeste.WallBooster.orig_Update orig, WallBooster self)
        {
            orig(self);
            if (!self.IceMode && self.IsHookAttached())
            {
                var grapples = self.Scene.Tracker.GetEntities<GrapplingHook>();
                foreach (GrapplingHook grapple in grapples)
                {
                    if (grapple.AttachedEntity == self)
                    {
                        if (grapple.Top > self.Top)
                        {
                            float movement = MathF.Min(160.0f * Engine.DeltaTime, grapple.Top - self.Top);
                            grapple.AddMovement(Vector2.UnitY * -movement);
                        }
                    }
                }
            }
        }

        private static void PostMove(this WallBooster self, Vector2 movement)
        {
            if (self.IsHookAttached())
                self.MakeGrappleFollowMe(movement, movement);
        }
    }
}
