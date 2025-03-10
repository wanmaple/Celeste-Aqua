using System;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using static Celeste.MoveBlock;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Cil;

namespace Celeste.Mod.Aqua.Core
{
    public static class MoveBlockExtensions
    {
        public static void Initialize()
        {
            _ilEmptyController = new ILHook(_methodController, MoveBlock_ILController);
            On.Celeste.MoveBlock.ctor_Vector2_int_int_Directions_bool_bool += MoveBlock_Construct;
            On.Celeste.MoveBlock.UpdateColors += MoveBlock_UpdateColors;
            On.Celeste.MoveBlock.Update += MoveBlock_Update;
            On.Celeste.MoveBlock.Render += MoveBlock_Render;
        }

        public static void Uninitialize()
        {
            _ilEmptyController?.Dispose();
            _ilEmptyController = null;
            On.Celeste.MoveBlock.ctor_Vector2_int_int_Directions_bool_bool -= MoveBlock_Construct;
            On.Celeste.MoveBlock.UpdateColors -= MoveBlock_UpdateColors;
            On.Celeste.MoveBlock.Update -= MoveBlock_Update;
            On.Celeste.MoveBlock.Render -= MoveBlock_Render;
        }

        private static void MoveBlock_ILController(ILContext il)
        {
            // add an empty il hook to recompile it for fixing the issue.
            // https://github.com/CommunalHelper/CommunalHelper/issues/228
        }

        private static void MoveBlock_Construct(On.Celeste.MoveBlock.orig_ctor_Vector2_int_int_Directions_bool_bool orig, MoveBlock self, Vector2 position, int width, int height, Directions direction, bool canSteer, bool fast)
        {
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

        private static ILHook _ilEmptyController;
        private static MethodInfo _methodController = typeof(MoveBlock).GetMethod("Controller", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetStateMachineTarget();

        private static Color AccelerationColor = Calc.HexToColor("fbff00");
        private static Color DeaccelerationColor = Calc.HexToColor("5298e6");
    }
}
