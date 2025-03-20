using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlatformExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Platform.ctor += Platform_Construct;
            On.Celeste.Platform.Update += Platform_Update;
            On.Celeste.Platform.MoveH_float += Platform_MoveH_1;
            On.Celeste.Platform.MoveH_float_float += Platform_MoveH_2;
            On.Celeste.Platform.MoveHNaive += Platform_MoveHNaive;
            On.Celeste.Platform.MoveHCollideSolids += Platform_MoveHCollideSolids;
            On.Celeste.Platform.MoveHCollideSolidsAndBounds += Platform_MoveHCollideSolidsAndBounds;
            On.Celeste.Platform.MoveV_float += Platform_MoveV_1;
            On.Celeste.Platform.MoveV_float_float += Platform_MoveV_2;
            On.Celeste.Platform.MoveVNaive += Platform_MoveVNaive;
            On.Celeste.Platform.MoveVCollideSolids += Platform_MoveVCollideSolids;
            On.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3_bool += Platform_MoveVCollideSolidsAndBounds;
        }

        public static void Uninitialize()
        {
            On.Celeste.Platform.ctor -= Platform_Construct;
            On.Celeste.Platform.Update -= Platform_Update;
            On.Celeste.Platform.MoveH_float -= Platform_MoveH_1;
            On.Celeste.Platform.MoveH_float_float -= Platform_MoveH_2;
            On.Celeste.Platform.MoveHNaive -= Platform_MoveHNaive;
            On.Celeste.Platform.MoveHCollideSolids -= Platform_MoveHCollideSolids;
            On.Celeste.Platform.MoveHCollideSolidsAndBounds -= Platform_MoveHCollideSolidsAndBounds;
            On.Celeste.Platform.MoveV_float -= Platform_MoveV_1;
            On.Celeste.Platform.MoveV_float_float -= Platform_MoveV_2;
            On.Celeste.Platform.MoveVNaive -= Platform_MoveVNaive;
            On.Celeste.Platform.MoveVCollideSolids -= Platform_MoveVCollideSolids;
            On.Celeste.Platform.MoveVCollideSolidsAndBounds_Level_float_bool_Action3_bool -= Platform_MoveVCollideSolidsAndBounds;
        }

        private static void Platform_Construct(On.Celeste.Platform.orig_ctor orig, Platform self, Vector2 position, bool safe)
        {
            orig(self, position, safe);
            DynamicData.For(self).Set("hookable", true);
        }

        private static void Platform_Update(On.Celeste.Platform.orig_Update orig, Platform self)
        {
            DynamicData.For(self).Set("prev_position", self.Position);
            orig(self);
        }

        private static void Platform_MoveH_1(On.Celeste.Platform.orig_MoveH_float orig, Platform self, float moveH)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveH);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static void Platform_MoveH_2(On.Celeste.Platform.orig_MoveH_float_float orig, Platform self, float moveH, float liftSpeedH)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveH, liftSpeedH);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static void Platform_MoveHNaive(On.Celeste.Platform.orig_MoveHNaive orig, Platform self, float moveH)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveH);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static bool Platform_MoveHCollideSolids(On.Celeste.Platform.orig_MoveHCollideSolids orig, Platform self, float moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide)
        {
            Vector2 oldPosition = self.ExactPosition;
            bool ret = orig(self, moveH, thruDashBlocks, onCollide);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
            return ret;
        }

        private static bool Platform_MoveHCollideSolidsAndBounds(On.Celeste.Platform.orig_MoveHCollideSolidsAndBounds orig, Platform self, Level level, float moveH, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide)
        {
            Vector2 oldPosition = self.ExactPosition;
            bool ret = orig(self, level, moveH, thruDashBlocks, onCollide);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
            return ret;
        }

        private static void Platform_MoveV_1(On.Celeste.Platform.orig_MoveV_float orig, Platform self, float moveV)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveV);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static void Platform_MoveV_2(On.Celeste.Platform.orig_MoveV_float_float orig, Platform self, float moveV, float liftSpeedV)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveV, liftSpeedV);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static void Platform_MoveVNaive(On.Celeste.Platform.orig_MoveVNaive orig, Platform self, float moveV)
        {
            Vector2 oldPosition = self.ExactPosition;
            orig(self, moveV);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
        }

        private static bool Platform_MoveVCollideSolids(On.Celeste.Platform.orig_MoveVCollideSolids orig, Platform self, float moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide)
        {
            Vector2 oldPosition = self.ExactPosition;
            bool ret = orig(self, moveV, thruDashBlocks, onCollide);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
            return ret;
        }

        private static bool Platform_MoveVCollideSolidsAndBounds(On.Celeste.Platform.orig_MoveVCollideSolidsAndBounds_Level_float_bool_Action3_bool orig, Platform self, Level level, float moveV, bool thruDashBlocks, Action<Vector2, Vector2, Platform> onCollide, bool checkBottom)
        {
            Vector2 oldPosition = self.ExactPosition;
            bool ret = orig(self, level, moveV, thruDashBlocks, onCollide, checkBottom);
            Vector2 newPosition = self.ExactPosition;
            Vector2 movement = newPosition - oldPosition;
            self.MakeGrappleFollowMe(movement);
            return ret;
        }
    }
}
