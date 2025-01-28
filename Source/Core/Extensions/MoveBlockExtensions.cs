using System;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using static Celeste.MoveBlock;

namespace Celeste.Mod.Aqua.Core
{
    public static class MoveBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.MoveBlock.ctor_Vector2_int_int_Directions_bool_bool += MoveBlock_Construct;
            On.Celeste.MoveBlock.UpdateColors += MoveBlock_UpdateColors;
            On.Celeste.MoveBlock.Update += MoveBlock_Update;
            On.Celeste.MoveBlock.Render += MoveBlock_Render;
        }

        public static void Uninitialize()
        {
            On.Celeste.MoveBlock.ctor_Vector2_int_int_Directions_bool_bool -= MoveBlock_Construct;
            On.Celeste.MoveBlock.UpdateColors -= MoveBlock_UpdateColors;
            On.Celeste.MoveBlock.Update -= MoveBlock_Update;
            On.Celeste.MoveBlock.Render -= MoveBlock_Render;
        }

        private static void MoveBlock_Construct(On.Celeste.MoveBlock.orig_ctor_Vector2_int_int_Directions_bool_bool orig, MoveBlock self, Vector2 position, int width, int height, Directions direction, bool canSteer, bool fast)
        {
            self.SetReversed(false);
            self.SetAccelerateState(AccelerationArea.AccelerateState.None);
            orig(self, position, width, height, direction, canSteer, fast);
            self.Add(new AccelerationAreaInOut(self.OnKeepInAccelerationArea, null, self.OnExitAccelerationArea));
            self.Add(new PureColorTrails(e => (e as MoveBlock).GetAccelerateState() == AccelerationArea.AccelerateState.Accelerate && self.Scene.OnInterval(0.05f), e => AccelerationColor, Vector2.Zero));
        }

        private static void MoveBlock_UpdateColors(On.Celeste.MoveBlock.orig_UpdateColors orig, MoveBlock self)
        {
            if (self.state == MovementState.Breaking)
                orig(self);
            else if (self.GetAccelerateState() != AccelerationArea.AccelerateState.None)
                self.UpdateColorsAcceleration();
            else
                orig(self);
        }

        private static void MoveBlock_Update(On.Celeste.MoveBlock.orig_Update orig, MoveBlock self)
        {
            orig(self);
            if (self.state == MovementState.Breaking)
                self.SetReversed(false);
        }

        private static void MoveBlock_Render(On.Celeste.MoveBlock.orig_Render orig, MoveBlock self)
        {
            PureColorTrails trails = self.Get<PureColorTrails>();
            trails.Render();
            if (self is RodMoveBlock rod)
            {
                MoveBlock.idleBgFill = rod.IdleFillColor;
                MoveBlock.pressedBgFill = rod.MovingFillColor;
                MoveBlock.breakingBgFill = rod.BreakingFillColor;
            }
            else
            {
                MoveBlock.idleBgFill = Calc.HexToColor("474070");
                MoveBlock.pressedBgFill = Calc.HexToColor("30b335");
                MoveBlock.breakingBgFill = Calc.HexToColor("cc2541");
            }
            orig(self);
        }

        private static void UpdateColorsAcceleration(this MoveBlock self)
        {
            Color value = idleBgFill;
            if (self.GetAccelerateState() == AccelerationArea.AccelerateState.Accelerate)
            {
                value = AccelerationColor;
            }
            else if (self.GetAccelerateState() == AccelerationArea.AccelerateState.Deaccelerate)
            {
                value = DeaccelerationColor;
            }

            self.fillColor = Color.Lerp(self.fillColor, value, 10.0f * Engine.DeltaTime);
            foreach (Image item in self.topButton)
            {
                item.Color = self.fillColor;
            }

            foreach (Image item2 in self.leftButton)
            {
                item2.Color = self.fillColor;
            }

            foreach (Image item3 in self.rightButton)
            {
                item3.Color = self.fillColor;
            }
        }

        private static void OnExitAccelerationArea(this MoveBlock self, AccelerationArea area)
        {
            self.SetAccelerateState(AccelerationArea.AccelerateState.None);
        }

        private static void OnKeepInAccelerationArea(this MoveBlock self, AccelerationArea area)
        {
            int oldSign = MathF.Sign(self.targetSpeed);
            self.SetAccelerateState(area.TryAccelerate(self));
            int newSign = MathF.Sign(self.targetSpeed);
            if (oldSign != newSign)
            {
                self.SetReversed(!self.IsReversed());
            }
        }

        public static bool IsReversed(this MoveBlock self)
        {
            return DynamicData.For(self).Get<bool>("reversed");
        }

        public static void SetReversed(this MoveBlock self, bool reversed)
        {
            DynamicData.For(self).Set("reversed", reversed);
        }

        private static Color AccelerationColor = Calc.HexToColor("fbff00");
        private static Color DeaccelerationColor = Calc.HexToColor("5298e6");
    }
}
