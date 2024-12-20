using Microsoft.Xna.Framework;
using Mono.Cecil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

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
                cursor.EmitDelegate(CheckClimbSlidable);
                cursor.EmitBrtrue(jumpLabel);
            }
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
                if (slidable.HasPlayerOnTop())
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
            if (self.moveX != 0)
            {
                if (MathF.Abs(self.Speed.X) < maxSpeed)
                {
                    if (MathF.Sign(self.Speed.X) == self.moveX)
                    {
                        self.SetSlideState(SlideStates.LowSpeed);
                        self.Speed.X = Calc.Approach(self.Speed.X, maxSpeed * self.moveX, 1000.0f * speedRate * Engine.DeltaTime);
                    }
                    else
                    {
                        self.SetSlideState(SlideStates.Turning);
                        self.Speed.X = Calc.Approach(self.Speed.X, maxSpeed * self.moveX, 60.0f * speedRate * Engine.DeltaTime);
                    }
                }
                else
                {
                    if (MathF.Sign(self.Speed.X) != self.moveX)
                    {
                        self.SetSlideState(SlideStates.Turning);
                        self.Speed.X = Calc.Approach(self.Speed.X, maxSpeed * self.moveX, 60.0f * speedRate * Engine.DeltaTime);
                    }
                    else
                    {
                        self.SetSlideState(SlideStates.HighSpeed);
                    }
                }
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
            if (Input.Jump.Pressed)
            {
                if (MathF.Abs(self.Speed.X) < SLIDE_JUMP_THRESHOLD)
                    self.Jump(false, true);
                else
                    self.SuperJump();
            }
            return (int)AquaStates.StNormal;
        }
    }
}
