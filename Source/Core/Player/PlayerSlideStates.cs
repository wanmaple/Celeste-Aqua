using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public partial class PlayerStates
    {
        public const float SLIDE_JUMP_THRESHOLD = 200.0f;

        public enum SlideStates
        {
            None,
            LowSpeed,
            HighSpeed,
            Turning,
        }

        private static void Player_ILClimbCheck(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel jumpLabel = null;
            if (cursor.TryGotoNext(ins => ins.MatchBrtrue(out jumpLabel)))
            {
                cursor.Index++;
                cursor.EmitLdarg0();
                cursor.EmitLdarg1();
                cursor.EmitLdarg2();
                cursor.EmitDelegate<Func<Player, int, int, bool>>(CheckClimbSlidable);
                cursor.EmitBrtrue(jumpLabel);
            }
        }

        private static void Player_ILClimbUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchConvR4()))
            {
                cursor.Index += 4;
                cursor.EmitLdarg0();
                cursor.EmitDelegate<Func<Player, bool>>(CheckClimbSlidable);
                cursor.EmitNot();
                cursor.EmitAnd();
            }
        }

        private static bool CheckClimbSlidable(this Player self)
        {
            return self.CheckClimbSlidable((int)self.Facing, 0);
        }

        private static bool CheckClimbSlidable(this Player self, int dir, int yAdd)
        {
            return self.CollideCheck<SlidableSolid>(self.Position + new Vector2(dir * 2, yAdd));
        }

        public static SlideStates GetSlideState(this Player self)
        {
            return DynamicData.For(self).Get<SlideStates>("slide_state");
        }

        private static void SetSlideState(this Player self, SlideStates state)
        {
            DynamicData.For(self).Set("slide_state", state);
        }

        private static bool CheckOnSlidable(this Player self)
        {
            List<Entity> slidables = self.Scene.Tracker.GetEntities<SlidableSolid>();
            foreach (SlidableSolid slidable in slidables)
            {
                if (slidable.GetPlayerRider() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static int SlideUpdate(this Player self)
        {
            if (self.LiftBoost.Y < 0f && self.wasOnGround && !self.onGround && self.Speed.Y >= 0f)
            {
                self.Speed.Y = self.LiftBoost.Y;
            }
            if (self.Speed.Y > 0.0f)
            {
                self.Speed.Y = 0.0f;
            }
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State == GrapplingHook.HookStates.Fixed && hook.ReachLockedLength(self.Center) && Vector2.Dot(hook.RopeDirection, new Vector2(MathF.Sign(self.Speed.X), 0.0f)) > 0.0f)
            {
                self.Speed.X = Calc.Approach(self.Speed.X, 0.0f, 60.0f * Engine.DeltaTime);
            }
            self.Ducking = false;
            if (self.Holding != null)
            {
                if (!Input.GrabCheck && self.minHoldTimer <= 0f)
                {
                    self.Throw();
                }
            }
            else
            {
                if (Input.GrabCheck && !self.IsTired)
                {
                    if (MathF.Abs(self.Speed.X) == 0.0f)
                    {
                        foreach (Holdable component in self.Scene.Tracker.GetComponents<Holdable>())
                        {
                            if (component.Check(self) && self.Pickup(component))
                            {
                                return (int)AquaStates.StPickup;
                            }
                        }
                    }
                    if (MathF.Sign(self.Speed.X) != 0 - self.Facing)
                    {
                        if (self.ClimbCheck((int)self.Facing))
                        {
                            if (!SaveData.Instance.Assists.NoGrabbing)
                            {
                                return (int)AquaStates.StClimb;
                            }

                            self.ClimbTrigger((int)self.Facing);
                        }
                    }
                }
                if (self.CanDash)
                {
                    self.Speed += self.LiftBoost;
                    return self.StartDash();
                }
            }
            float speedRate = 1.0f;
            if (self.level.CoreMode == Session.CoreModes.Cold)
            {
                speedRate *= 0.3f;
            }
            float maxSpeed;
            if (self.Holding != null && self.Holding.SlowRun)
            {
                maxSpeed = 70f;
            }
            else
            {
                maxSpeed = 90f;
            }
            if (self.level.InSpace)
            {
                maxSpeed *= 0.6f;
            }
            Vector2 windSpeed = self.GetWindSpeed();
            if (!AquaMaths.IsApproximateZero(windSpeed.X))
            {
                float windAcc = MathF.Abs(windSpeed.X) / 800.0f * 640.0f;
                float targetSpeed = windSpeed.X;
                float defaultAcc = 0.0f;
                if (MathF.Sign(self.Speed.X) == self.moveX)
                {
                    defaultAcc = 360.0f;
                }
                else if (MathF.Sign(self.Speed.X) == -self.moveX)
                {
                    defaultAcc = 60.0f;
                }
                self.Speed.X = Calc.Approach(self.Speed.X, targetSpeed, (defaultAcc + windAcc) * speedRate * Engine.DeltaTime);
                if (MathF.Sign(self.Speed.X) == -self.moveX)
                    self.SetSlideState(SlideStates.Turning);
                else if (MathF.Abs(self.Speed.X) < maxSpeed)
                    self.SetSlideState(SlideStates.LowSpeed);
                else
                    self.SetSlideState(SlideStates.HighSpeed);
            }
            else
            {
                if (self.moveX != 0)
                {
                    if (MathF.Sign(self.Speed.X) == self.moveX)
                    {
                        if (MathF.Abs(self.Speed.X) < maxSpeed)
                        {
                            self.Speed.X = Calc.Approach(self.Speed.X, maxSpeed * self.moveX, 360.0f * speedRate * Engine.DeltaTime);
                        }
                    }
                    else
                    {
                        self.Speed.X = Calc.Approach(self.Speed.X, maxSpeed * self.moveX, 60.0f * speedRate * Engine.DeltaTime);
                    }
                    if (MathF.Sign(self.Speed.X) == -self.moveX)
                        self.SetSlideState(SlideStates.Turning);
                    else if (MathF.Abs(self.Speed.X) < maxSpeed)
                        self.SetSlideState(SlideStates.LowSpeed);
                    else
                        self.SetSlideState(SlideStates.HighSpeed);
                }
                else
                {
                    if (MathF.Abs(self.Speed.X) == 0.0f)
                        self.SetSlideState(SlideStates.None);
                    else if (MathF.Abs(self.Speed.X) < maxSpeed)
                        self.SetSlideState(SlideStates.LowSpeed);
                    else
                        self.SetSlideState(SlideStates.HighSpeed);
                }
            }
            if (Input.Jump.Pressed)
            {
                if (MathF.Abs(self.Speed.X) < SLIDE_JUMP_THRESHOLD)
                    self.Jump(false, true);
                else if ((int)self.Facing == MathF.Sign(self.Speed.X))
                    self.SuperJump();
                else
                    self.Jump(false, true);
            }
            return (int)AquaStates.StNormal;
        }
    }
}
