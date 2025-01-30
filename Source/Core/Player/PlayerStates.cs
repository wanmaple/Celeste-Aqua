using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using MonoMod.Utils;
using Mono.Cecil;
using Celeste.Mod.Aqua.Debug;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public enum AquaStates
    {
        StNormal = 0, // Normal
        StClimb = 1, // Climbing
        StDash = 2, // Dashing
        StBoost = 4, // In the green booster
        StRedDash = 5, // In the red booster
        StHitSquash = 6, // While red booster hits wall or something.
        StLaunch = 7, // Bounced by the bumper/puffle etc.
        StPickup = 8, // Picking holdables.
        StDreamDash = 9, // Dream dashing.
        StSummitLaunch = 10, // Last up throwing by Badeline.
        StDummy = 11, // For some cutscenes I think.
        StIntroWalk = 12,
        StIntroJump = 13,
        StIntroRespawn = 14,
        StIntroWakeUp = 15,
        StBirdDashTutorial = 16,
        StFrozen = 17,
        StReflectionFall = 18, // Falling sceneary in 6a-2.
        StStarFly = 19, // Feather flying.
        StTempleFall = 20, // Falling sceneary in 5a.
        StCassetteFly = 21, // Wrapped by bubbles when obtaining the casette.
        StAttract = 22, // Attraction in 6a badeline boss
        StIntroMoonJump = 23, // 9a begining.
        StFlingBird = 24, // Throwed by bird in 9a.
        StIntroThinkForABit = 25,

        // Extended States
        StHanging = 26,
        StElectricShocking,

        MaxStates,
    }

    public static partial class PlayerStates
    {
        public const float SPEED_CHECK_GRAPPLING_SWING_DOWN = 250.0f;
        public const float SPEED_CHECK_2_GRAPPLING_SWING_DOWN = 160.0f;
        public const float SPEED_CHECK_GRAPPLING_SWING_UP = 90.0f;

        public static void Initialize()
        {
            IL.Celeste.Player.ctor += Player_ILConstruct;
            IL.Celeste.Player.NormalUpdate += Player_ILNormalUpdate;
            IL.Celeste.Player.OnCollideV += Player_ILOnCollideV;
            IL.Celeste.Player.ClimbCheck += Player_ILClimbCheck;
            IL.Celeste.Player.ClimbUpdate += Player_ILClimbUpdate;
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.NormalUpdate += Player_NormalUpdate;
            On.Celeste.Player.DashBegin += Player_DashBegin;
            On.Celeste.Player.DashEnd += Player_DashEnd;
            On.Celeste.Player.DashUpdate += Player_DashUpdate;
            On.Celeste.Player.LaunchUpdate += Player_LaunchUpdate;
            On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.Player.RedDashBegin += Player_RedDashBegin;
            On.Celeste.Player.RedDashUpdate += Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine;
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
            On.Celeste.Player.DreamDashUpdate += Player_DreamDashUpdate;
            On.Celeste.Player.SummitLaunchBegin += Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin += Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin += Player_FlingBirdBegin;
            On.Celeste.Player.CassetteFlyBegin += Player_CassetteFlyBegin;
            On.Celeste.Player.PickupCoroutine += Player_PickupCoroutine;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.WindMove += Player_WindMove;
            On.Celeste.Player.Jump += Player_Jump;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
            On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor -= Player_ILConstruct;
            IL.Celeste.Player.NormalUpdate -= Player_ILNormalUpdate;
            IL.Celeste.Player.OnCollideV -= Player_ILOnCollideV;
            IL.Celeste.Player.ClimbCheck -= Player_ILClimbCheck;
            IL.Celeste.Player.ClimbUpdate -= Player_ILClimbUpdate;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashBegin -= Player_DashBegin;
            On.Celeste.Player.DashEnd -= Player_DashEnd;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.LaunchUpdate -= Player_LaunchUpdate;
            On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
            On.Celeste.Player.RedDashBegin -= Player_RedDashBegin;
            On.Celeste.Player.RedDashUpdate -= Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
            On.Celeste.Player.DreamDashUpdate -= Player_DreamDashUpdate;
            On.Celeste.Player.SummitLaunchBegin -= Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin -= Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin -= Player_FlingBirdBegin;
            On.Celeste.Player.CassetteFlyBegin -= Player_CassetteFlyBegin;
            On.Celeste.Player.PickupCoroutine -= Player_PickupCoroutine;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.WindMove -= Player_WindMove;
            On.Celeste.Player.Jump -= Player_Jump;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
            On.Celeste.Player.UpdateSprite -= Player_UpdateSprite;
        }

        private static void Player_ILConstruct(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdcI4(26)))
            {
                cursor.Index++;
                cursor.EmitLdcI4((int)AquaStates.MaxStates - (int)AquaStates.StHanging);
                cursor.EmitAdd();
            }
        }

        private static void Player_ILNormalUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdcR4(90.0f), ins => ins.MatchStloc(6)))
            {
                cursor.Index += 2;
                cursor.EmitLdloc(6);
                cursor.EmitLdarg0();
                cursor.EmitDelegate(CalculateNormalMoveCoefficient);
                cursor.EmitMul();
                cursor.EmitStloc(6);

                ILLabel label = null;
                if (cursor.TryGotoNext(ins => ins.MatchLdloc(7), ins => ins.MatchBltUn(out label)))
                {
                    cursor.Index += 2;
                    cursor.EmitLdarg0();
                    cursor.EmitDelegate(IsEmittingHook);
                    cursor.EmitBrtrue(label);
                }
            }
        }

        private static bool IsEmittingHook(this Player self)
        {
            GrapplingHook hook = self.GetGrappleHook();
            if (hook != null && hook.Active && (hook.State == GrapplingHook.HookStates.Emitting || hook.State == GrapplingHook.HookStates.Bouncing))
            {
                return true;
            }
            return false;
        }

        private static float CalculateNormalMoveCoefficient(this Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && (hook.State == GrapplingHook.HookStates.Emitting || hook.State == GrapplingHook.HookStates.Bouncing))
            {
                return 0.5f;
            }
            return 1.0f;
        }

        private static void Player_ILOnCollideV(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            FieldReference field = null;
            MethodReference method = null;
            ILLabel label = null;
            if (cursor.TryGotoNext(ins => ins.MatchLdfld(out field), ins => ins.MatchCallvirt(out method), ins => ins.MatchLdcI4(1), ins => ins.MatchBeq(out label)))
            {
                cursor.Index += 4;
                cursor.EmitLdarg0();
                cursor.EmitLdfld(field);
                cursor.EmitCallvirt(method);
                cursor.EmitLdcI4((int)AquaStates.StHanging);
                cursor.EmitBeq(label);
            }
        }

        private static void Player_Construct(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            self.StateMachine.SetCallbacks((int)AquaStates.StLaunch, self.LaunchUpdate, null, self.LaunchBegin, self.Player_LaunchEnd);
            self.StateMachine.SetCallbacks((int)AquaStates.StHanging, self.Player_HangingUpdate, null, self.Player_HangingBegin, self.Player_HangingEnd);
            self.StateMachine.SetStateName((int)AquaStates.StHanging, "Hanging");
            self.StateMachine.SetCallbacks((int)AquaStates.StElectricShocking, self.Player_ElectricShockingUpdate, null, self.Player_ElectricShockingBegin);
            self.StateMachine.SetStateName((int)AquaStates.StElectricShocking, "ElectricShocking");
            //DynamicData.For(self).Set("start_emitting", false);
            //self.SetTimeTicker("emit_ticker", 0.05f);
            self.SetTimeTicker("dash_hanging_ticker", 0.05f);
            self.SetTimeTicker("boost_speed_save_ticker", 0.5f);
            self.SetTimeTicker("swing_jump_keeping_ticker", 0.1f);
            self.SetTimeTicker("elec_shock_ticker", 1.0f);
            DynamicData.For(self).Set("lift_speed_y", 0.0f);
            DynamicData.For(self).Set("rope_is_loosen", true);
            DynamicData.For(self).Set("is_booster_dash", false);
            self.SetSlideState(SlideStates.None);
            self.SetSavedSwingSpeed(Vector2.Zero);
            self.SetSpecialSwingDirection(0.0f);
            self.SetSpecialSwingSpeed(Player.DashSpeed);
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("previous_facing", (int)self.Facing);
        }

        private static void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active)
            {
                scene.Remove(hook);
            }
            var state = (scene as Level).GetState();
            if (state != null)
            {
                state.RestShootCount = state.InitialShootCount;
                hook.ChangeGameplayMode(state.GameplayMode, scene as Level, state.InitialShootCount);
            }

            orig(self, scene);
        }

        private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
        {
            int nextState = orig(self);
            self.RestoreSavedSwingSpeedIfPossible(nextState);
            return nextState;
        }

        private static void Player_RedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self)
        {
            orig(self);
            Vector2 speed = self.GetSavedSwingSpeed();
            if (AquaMaths.IsApproximateZero(speed))
            {
                speed = self.Speed;
            }
            if (AquaMaths.IsApproximateZero(speed))
            {
                speed = (int)self.Facing * Player.DashSpeed * Vector2.UnitX;
            }
            SetupSpecialSwingArguments(self, speed);
        }

        private static int Player_RedDashUpdate(On.Celeste.Player.orig_RedDashUpdate orig, Player self)
        {
            int nextState = orig(self);
            if (nextState == (int)AquaStates.StRedDash && !AquaMaths.IsApproximateZero(self.Speed))
            {
                self.UpdateSpecialSwing();
            }
            return nextState;
        }

        private static System.Collections.IEnumerator Player_RedDashCoroutine(On.Celeste.Player.orig_RedDashCoroutine orig, Player self)
        {
            yield return new SwapImmediately(orig(self));
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    float sign = MathF.Sign(Vector2.Dot(self.Speed, hook.SwingDirection));
                    if (sign != self.GetSpecialSwingDirection())
                    {
                        self.SetSpecialSwingDirection(sign);
                        self.SetSpecialSwingSpeed(Player.DashSpeed);
                    }
                }
            }
        }

        private static void Player_DreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self)
        {
            orig(self);
            SetupSpecialSwingArguments(self, self.Speed);
        }

        private static int Player_DreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            int nextState = orig(self);
            if (nextState == (int)AquaStates.StDreamDash)
            {
                self.UpdateSpecialSwing();
            }
            else
            {
                // Goto normal state.
            }
            return nextState;
        }

        private static void Player_FlingBirdBegin(On.Celeste.Player.orig_FlingBirdBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_CassetteFlyBegin(On.Celeste.Player.orig_CassetteFlyBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static System.Collections.IEnumerator Player_PickupCoroutine(On.Celeste.Player.orig_PickupCoroutine orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            return orig(self);
        }

        private static void Player_StarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_SummitLaunchBegin(On.Celeste.Player.orig_SummitLaunchBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static int Player_NormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
        {
            int nextState = PreHookUpdate(self);
            if (nextState < 0)
            {
                if (self.CheckOnSlidable())
                    nextState = self.SlideUpdate();
                else
                    nextState = orig(self);
            }
            if (nextState == (int)AquaStates.StNormal)
            {
                int postState = PostHookUpdate(self);
                if (postState >= 0)
                {
                    nextState = postState;
                }
            }
            return nextState;
        }

        private static void Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            orig(self);
            TimeTicker dashHangingTicker = self.GetTimeTicker("dash_hanging_ticker");
            dashHangingTicker.Reset();
            if (self.CurrentBooster != null)
            {
                DynamicData.For(self).Set("is_booster_dash", true);
            }
        }

        private static void Player_DashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
        {
            orig(self);
            DynamicData.For(self).Set("is_booster_dash", false);
        }

        private static int Player_DashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
        {
            int nextState = PreHookUpdate(self);
            if (nextState < 0)
            {
                nextState = orig(self);
            }
            if (nextState == (int)AquaStates.StDash)
            {
                int postState = PostHookUpdate(self);
                if (postState >= 0)
                {
                    nextState = postState;
                }
            }
            return nextState;
        }

        private static int Player_LaunchUpdate(On.Celeste.Player.orig_LaunchUpdate orig, Player self)
        {
            int nextState = PreHookUpdate(self);
            if (nextState < 0)
            {
                nextState = orig(self);
            }
            if (nextState == (int)AquaStates.StLaunch)
            {
                int postState = PostHookUpdate(self);
                if (postState >= 0)
                {
                    nextState = postState;
                }
            }
            return nextState;
        }

        private static void Player_LaunchEnd(this Player self)
        {
            if (self.StateMachine.State == (int)AquaStates.StHanging)
            {
                TimeTicker keepTicker = self.GetTimeTicker("swing_jump_keeping_ticker");
                keepTicker.Reset();
                DynamicData.For(self).Set("lift_speed_y", MathF.Min(self.Speed.Y, 0.0f));
            }
        }

        private static void Player_HangingBegin(this Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.UserLockedLength > 0.0f)
            {
                hook.SetLockedLengthDirectly(hook.UserLockedLength);
            }
            else
            {
                hook.SetRopeLengthLocked(true, self.Center);
                hook.UserLockedLength = hook.LockedRadius;
            }
            DynamicData.For(self).Set("climb_rope_direction", 0);
        }

        private static void Player_HangingEnd(this Player self)
        {
            var hook = self.GetGrappleHook();
            hook.SetRopeLengthLocked(false, self.Center);
            DynamicData.For(self).Set("climb_rope_direction", 0);
            DynamicData.For(self).Set("lift_speed_y", 0.0f);
            if (self.StateMachine.State == (int)AquaStates.StBoost || self.StateMachine.State == (int)AquaStates.StRedDash)
            {
                self.SetSavedSwingSpeed(self.Speed);
                TimeTicker saveTicker = self.GetTimeTicker("boost_speed_save_ticker");
                saveTicker.Reset();
            }
        }

        private static int Player_HangingUpdate(this Player self)
        {
            float dt = Engine.DeltaTime;
            var hook = self.GetGrappleHook();
            var shotCheck = self.GetShootHookCheck();
            Vector2 ropeDirection = hook.RopeDirection;
            Vector2 swingDirection = hook.SwingDirection;
            bool swingUp = IsRopeSwingingUp(ropeDirection);
            TimeTicker keepTicker = self.GetTimeTicker("swing_jump_keeping_ticker");
            keepTicker.Tick(dt);
            if (keepTicker.Check())
            {
                DynamicData.For(self).Set("lift_speed_y", 0.0f);
            }
            if (hook.State != GrapplingHook.HookStates.Fixed)
            {
                return (int)AquaStates.StNormal;
            }
            else if (self.CanDash)
            {
                return self.StartDash();
            }
            else if (self.IsExhausted())
            {
                hook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (shotCheck.CanFlyTowards && hook.CanFlyToward())
            {
                self.FlyTowardHook();
            }
            else if (shotCheck.CanRevoke)
            {
                hook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (!Input.GrabCheck && !AquaModule.Settings.AutoGrabRopeIfPossible)
            {
                return (int)AquaStates.StNormal;
            }
            else if (Input.GrabCheck && self.ClimbCheck((int)self.Facing) && !self.DashAttacking)
            {
                return (int)AquaStates.StClimb;
            }
            else if (swingUp && Input.Jump.Pressed)
            {
                self.SwingJump(dt);
                return (int)AquaStates.StNormal;
            }
            else if (!swingUp)
            {
                if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                    self.Speed.Y = -self.Speed.Y;
                float speedTangent = MathF.Abs(Vector2.Dot(self.Speed, swingDirection));
                if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                    self.Speed.Y = -self.Speed.Y;
                if (speedTangent >= SPEED_CHECK_2_GRAPPLING_SWING_DOWN)
                {
                    if (Input.Jump.Pressed)
                    {
                        self.SwingJump(dt);
                        return (int)AquaStates.StNormal;
                    }
                }
                else
                {
                    return (int)AquaStates.StNormal;
                }
            }

            DynamicData.For(self).Set("climb_rope_direction", 0);
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;
            self.HandleHangingSpeed(dt);
            float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
            hook.AlongRopeSpeed = speedAlongRope;
            bool ableToClimbUpDown = false;
            if (speedAlongRope >= 0.0f)
            {
                DynamicData.For(self).Set("rope_is_loosen", false);
                float approxLen = hook.CalculateRopeLength(self.Center);
                if (approxLen >= hook.LockedRadius - 1.5f)
                {
                    ableToClimbUpDown = true;
                    self.Speed = self.TurnToTangentSpeed(self.Speed, swingDirection);
                }
                else
                {
                    Vector2 tangentSpd = self.TurnToTangentSpeed(self.Speed, swingDirection);
                    if (tangentSpd.LengthSquared() >= SPEED_CHECK_GRAPPLING_SWING_UP * SPEED_CHECK_GRAPPLING_SWING_UP)
                    {
                        self.Speed = tangentSpd;
                    }
                }
            }
            int inputY = Input.MoveY.Value;
            if (swingUp)
            {
                DynamicData.For(self).Set("climb_rope_direction", MathF.Sign(inputY));
            }
            var levelState = (self.Scene as Level).GetState();
            if (inputY != 0 && swingUp && ableToClimbUpDown)
            {
                float rollingSpeed = inputY > 0 ? 80.0f : -45.0f;
                hook.AddLockedRopeLength(rollingSpeed * dt);
                // Since the enforcing will handle the up climbing more smoothly as well as i'm not glad to see the climbing speed affects on the swing jump, we only handle down climbing here.
                if (inputY > 0)
                {
                    self.Speed += ropeDirection * rollingSpeed;
                    if (self.onGround)
                    {
                        return (int)AquaStates.StNormal;
                    }
                }
                else
                {
                    // For some reason, I'd like to avoid up spikes collision while climbing up.
                    // I need a more elegant way to handle this, TODO.
                    self.Speed -= ropeDirection;
                }
            }
            else if (self.onGround)
            {
                return (int)AquaStates.StNormal;
            }
            if (Input.MoveX.Value != 0)
            {
                float sign = swingUp ? 1.0f : -1.0f;
                Vector2 strengthSpeed = levelState.HookSettings.SwingStrength * swingDirection * Input.MoveX.Value * dt * sign;
                strengthSpeed *= Calc.Clamp(hook.SwingRadius / hook.MaxLength, 0.1f, 1.0f);
                self.Speed += strengthSpeed;
            }
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;

            return (int)AquaStates.StHanging;
        }

        private static void Player_ElectricShockingBegin(this Player self)
        {
            self.Speed = Vector2.Zero;
            TimeTicker ticker = self.GetTimeTicker("elec_shock_ticker");
            ticker.Reset();
            self.Sprite.Play("aqua_electricshock");
            Audio.Play("event:/char/madeline/electric_shock", self.Position);
        }

        private static int Player_ElectricShockingUpdate(this Player self)
        {
            TimeTicker ticker = self.GetTimeTicker("elec_shock_ticker");
            ticker.Tick(Engine.DeltaTime);
            if (ticker.Check())
            {
                self.Die(Vector2.Zero);
                return (int)AquaStates.StNormal;
            }
            return (int)AquaStates.StElectricShocking;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (self.StateMachine.State == (int)AquaStates.StElectricShocking)
            {
                foreach (var com in self.Components)
                {
                    com.Update();
                }
            }
            else
            {
                float dt = Engine.DeltaTime;
                DynamicData.For(self).Set("rope_is_loosen", true);
                DynamicData.For(self).Set("previous_facing", (int)self.Facing);
                //if (!PERMIT_SHOOT_STATES.Contains(self.StateMachine.State))
                //{
                //    DynamicData.For(self).Set("start_emitting", false);
                //}
                if (!self.CheckOnSlidable())
                    self.SetSlideState(SlideStates.None);

                orig(self);

                var hook = self.GetGrappleHook();
                var shotCheck = self.GetShootHookCheck();
                if (hook.Active && hook.Revoked)
                {
                    self.Scene.Remove(hook);
                    shotCheck.EndPeriod();
                }
                else if (hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
                {
                    if (hook.EnforcePlayer(self, new Segment(hook.PlayerPreviousPosition, self.Center), Engine.DeltaTime))
                    {
                        hook.Revoke();
                        if (self.StateMachine.State == (int)AquaStates.StHanging)
                        {
                            self.StateMachine.ForceState((int)AquaStates.StNormal);
                        }
                    }
                    if (self.StateMachine.State == (int)AquaStates.StHanging)
                    {
                        int climbRopeDirection = DynamicData.For(self).Get<int>("climb_rope_direction");
                        float staminaCost = 0.0f;
                        if (climbRopeDirection < 0)
                        {
                            staminaCost = self.SceneAs<Level>().GetState().HookSettings.ClimbUpStaminaCost * dt;
                        }
                        else if (climbRopeDirection == 0)
                        {
                            staminaCost = self.SceneAs<Level>().GetState().HookSettings.GrabingStaminaCost * dt;
                        }
                        self.Stamina = MathF.Max(0.0f, self.Stamina - staminaCost);
                    }
                }
            }
            self.GetShootHookCheck().Update();
            {
                GrapplingHook hook = self.GetGrappleHook();
                if (hook != null)
                {
                    if ((!Input.GrabCheck && !AquaModule.Settings.AutoGrabRopeIfPossible) || self.onGround || !hook.Active || hook.State != GrapplingHook.HookStates.Fixed || (self.StateMachine.State != (int)AquaStates.StNormal && self.StateMachine.State != (int)AquaStates.StHanging))
                    {
                        hook.UserLockedLength = 0.0f;
                    }
                }
            }
        }

        private static void Player_WindMove(On.Celeste.Player.orig_WindMove orig, Player self, Vector2 move)
        {
            if (!DynamicData.For(self).Get<bool>("rope_is_loosen"))
                return;

            orig(self, move);
        }

        private static void Player_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
        {
            bool possible = false;
            if (self.dreamJump)
            {
                GrapplingHook hook = self.GetGrappleHook();
                if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
                {
                    // force do a swing jump?
                    //if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                    {
                        // replace dream jump to swing jump.
                        possible = true;
                        if (possible)
                        {
                            self.SwingJump(Engine.DeltaTime);
                        }
                    }
                }
            }

            if (!possible)
            {
                orig(self, particles, playSfx);
            }
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            // Since the climb jump is conflict with grabbing rope, simply revoke it.
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self)
        {
            if (self.StateMachine.State != (int)AquaStates.StHanging)
            {
                if (self.GetSlideState() != SlideStates.None)
                {
                    self.Sprite.Scale = Vector2.One;
                    switch (self.GetSlideState())
                    {
                        case SlideStates.LowSpeed:
                            if (self.Sprite.CurrentAnimationID != "fliponice")
                            {
                                if (MathF.Sign(self.Speed.X) != (int)self.Facing)
                                {
                                    self.Sprite.Play("runStumble");
                                }
                                else
                                {
                                    self.Sprite.Play("aqua_icestumble");
                                }
                            }
                            break;
                        case SlideStates.HighSpeed:
                            if (self.Sprite.CurrentAnimationID != "fliponice")
                            {
                                self.Sprite.Play("aqua_iceslide");
                            }
                            break;
                        case SlideStates.Turning:
                            if (DynamicData.For(self).Get<int>("previous_facing") == -(int)self.Facing)
                            {
                                self.Sprite.PlayFlipOnIce();
                            }
                            else if (self.Sprite.CurrentAnimationID != "fliponice")
                            {
                                self.Sprite.Play("skid");
                            }
                            break;
                    }
                }
                else
                {
                    orig(self);
                }
            }
            else
            {
                self.Sprite.Scale = Vector2.One;
                int climbRopeDirection = DynamicData.For(self).Get<int>("climb_rope_direction");
                if (DynamicData.For(self).Get<int>("previous_facing") == -(int)self.Facing)
                {
                    self.Sprite.Play("aqua_hookflip", true);
                }
                else if (self.Sprite.CurrentAnimationID != "aqua_hookflip")
                {
                    if (climbRopeDirection < 0)
                    {
                        self.Sprite.Play("aqua_hookup");
                    }
                    else if (climbRopeDirection > 0)
                    {
                        self.Sprite.Play("aqua_hookdown");
                    }
                    else
                    {
                        self.Sprite.Play("aqua_hookidle");
                    }
                }
            }
        }

        private static void SwingJump(this Player self, float dt)
        {
            Input.Jump.ConsumeBuffer();
            var hook = self.GetGrappleHook();
            hook.Revoke();
            var levelState = self.SceneAs<Level>().GetState();
            const float X_LIFT = 0.0f;
            const float Y_LIFT = 1.0f;
            float keepedLiftY = DynamicData.For(self).Get<float>("lift_speed_y");
            Vector2 speed = new Vector2(self.Speed.X, MathF.Min(self.Speed.Y, keepedLiftY));
            speed = TurnToMoreAccurateSpeed(speed, SWING_JUMP_ACCURACY_RANGE_LIST);
            self.LiftSpeed = speed * new Vector2(X_LIFT, Y_LIFT * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f));
            self.dreamJump = false;
            self.Jump(false);
            if (!self.launched)
            {
                self.launched = MathF.Abs(self.Speed.X) >= 300.0f;
                if (self.launched)
                {
                    self.Play("event:/char/madeline/jump_assisted");
                }
            }
            float staminaCost = levelState.HookSettings.SwingJumpStaminaCost;
            self.Stamina = MathF.Max(0.0f, self.Stamina - staminaCost);
        }

        private static Vector2 TurnToMoreAccurateSpeed(Vector2 speed, KeyValuePair<float, Vector2>[] accuracyConfig)
        {
            if (AquaMaths.IsApproximateZero(speed))
                return Vector2.Zero;
            speed = Calc.SafeNormalize(speed) * MathF.Round(speed.Length() / 10.0f) * 10.0f;
            float xSign = MathF.Sign(speed.X);
            float ySign = MathF.Sign(speed.Y);
            speed = AquaMaths.Abs(speed);
            float len = speed.Length();
            float start = 0.0f;
            for (int i = 0; i < accuracyConfig.Length; i++)
            {
                var pair = accuracyConfig[i];
                float end = pair.Key;
                Vector2 vStart = Calc.AngleToVector(start, 1.0f);
                Vector2 vEnd = Calc.AngleToVector(end, 1.0f);
                if (AquaMaths.IsVectorInsideTwoVectors(speed, vStart, vEnd))
                {
                    speed = pair.Value * len;
                    break;
                }
                start = end;
            }
            speed *= new Vector2(xSign, ySign);
            return speed;
        }

        private static Vector2 TurnToTangentSpeed(this Player self, Vector2 speed, Vector2 swingDirection)
        {
            float lineSpeed = Vector2.Dot(speed, swingDirection);
            var levelState = self.SceneAs<Level>().GetState();
            lineSpeed = Calc.Clamp(lineSpeed, -levelState.HookSettings.MaxLineSpeed, levelState.HookSettings.MaxLineSpeed);
            return lineSpeed * swingDirection;
        }

        private static void FlyTowardHook(this Player self)
        {
            Celeste.Freeze(0.05f);
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;
            var hook = self.GetGrappleHook();
            hook.ResetFlyTowardThings();
            Vector2 ropeDirection = hook.RopeDirection;
            var levelState = self.level.GetState();
            float origAlongSpeed = MathF.Max(Vector2.Dot(self.Speed, -ropeDirection), 0.0f);
            self.Speed = -ropeDirection * MathF.Min(self.SceneAs<Level>().GetState().HookSettings.FlyTowardSpeed + origAlongSpeed, levelState.HookSettings.MaxLineSpeed);
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        private static int PreHookUpdate(Player self)
        {
            if (self.level.GetState() == null)
                return -1;
            if (!self.level.GetState().FeatureEnabled)
                return -1;

            var hook = self.GetGrappleHook();
            var shotCheck = self.GetShootHookCheck();
            float dt = Engine.DeltaTime;
            //if (DynamicData.For(self).Get<bool>("start_emitting"))
            //{
            //    TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
            //    emittingTicker.Tick(Engine.RawDeltaTime);
            //    if (emittingTicker.CheckRate(0.5f))
            //    {
            //        if (!hook.Active)
            //        {
            //            Vector2 direction;
            //            switch (AquaModule.Settings.DefaultShotDirection)
            //            {
            //                case DefaultShotDirections.Forward:
            //                    direction = Vector2.UnitX * (int)self.Facing;
            //                    break;
            //                case DefaultShotDirections.ForwardUp:
            //                    direction = new Vector2((int)self.Facing, -1.0f);
            //                    direction.Normalize();
            //                    break;
            //                case DefaultShotDirections.Up:
            //                default:
            //                    direction = -Vector2.UnitY;
            //                    break;
            //            }
            //            if (DynamicData.For(self).Get<bool>("backward_down_shoot"))
            //            {
            //                direction = new Vector2(-(int)self.Facing, 1.0f);
            //                direction.Normalize();
            //            }
            //            else if (DynamicData.For(self).Get<bool>("down_shoot"))
            //            {
            //                direction = Vector2.UnitY;
            //            }
            //            else if (Input.Aim.Value != Vector2.Zero)
            //            {
            //                direction = Input.GetAimVector(self.Facing);
            //            }
            //            direction.Y *= ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
            //            float emitSpeed = self.SceneAs<Level>().GetState().HookSettings.EmitSpeed;
            //            float emitSpeedCoeff = self.CalculateEmitParameters(emitSpeed * direction, ref direction);
            //            if (self.CanEmitHook(direction) && hook.CanEmit(self.level))
            //            {
            //                hook.Emit(self.level, direction, emitSpeed, emitSpeedCoeff - 1.0f);
            //                self.Scene.Add(hook);
            //            }
            //        }
            //        if (emittingTicker.CheckRate(1.0f))
            //        {
            //            DynamicData.For(self).Set("start_emitting", false);
            //            Engine.TimeRateB = 1.0f;
            //        }
            //    }
            //    return -1;
            //}
            //if (!hook.Active && (shotCheck.CanThrow || AquaModule.Settings.DownShoot.Pressed || AquaModule.Settings.BackwardDownShoot.Pressed) && !self.IsExhausted() && self.Holding == null)
            //{
            //    Engine.TimeRateB = 0.1f;
            //    DynamicData.For(self).Set("start_emitting", true);
            //    DynamicData.For(self).Set("down_shoot", AquaModule.Settings.DownShoot.Pressed);
            //    DynamicData.For(self).Set("backward_down_shoot", AquaModule.Settings.BackwardDownShoot.Pressed);
            //    TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
            //    emittingTicker.Reset();
            //    return -1;
            //} 
            bool downGrapplePressed = AquaModule.Settings.DownShoot.Pressed;
            bool backwardDownGrapplePressed = AquaModule.Settings.BackwardDownShoot.Pressed;
            if (!hook.Active && (shotCheck.CanThrow || downGrapplePressed || backwardDownGrapplePressed) && !self.IsExhausted() && self.Holding == null && hook.CanEmit(self.level))
            {
                Vector2 direction;
                switch (AquaModule.Settings.DefaultShotDirection)
                {
                    case DefaultShotDirections.Forward:
                        direction = Vector2.UnitX * (int)self.Facing;
                        break;
                    case DefaultShotDirections.ForwardUp:
                        direction = new Vector2((int)self.Facing, -1.0f);
                        direction.Normalize();
                        break;
                    case DefaultShotDirections.Up:
                    default:
                        direction = -Vector2.UnitY;
                        break;
                }
                if (backwardDownGrapplePressed)
                {
                    direction = new Vector2(-(int)self.Facing, 1.0f);
                    direction.Normalize();
                }
                else if (downGrapplePressed)
                {
                    direction = Vector2.UnitY;
                }
                else if (Input.Aim.Value != Vector2.Zero)
                {
                    direction = Input.GetAimVector(self.Facing);
                }
                if (direction.Y > 0.0f)
                {
                    Celeste.Freeze(0.05f);
                }
                else
                {
                    Celeste.Freeze(0.03f);
                }
                direction.Y *= ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
                float emitSpeed = self.SceneAs<Level>().GetState().HookSettings.EmitSpeed;
                float emitSpeedCoeff = self.CalculateEmitParameters(emitSpeed * direction, ref direction);
                hook.Emit(self.level, direction, emitSpeed, emitSpeedCoeff - 1.0f);
                self.Scene.Add(hook);
            }

            if (hook.Active)
            {
                if (hook.State == GrapplingHook.HookStates.Fixed && self.IsExhausted())
                {
                    hook.Revoke();
                }
                else if (hook.JustFixed && (hook.CanFlyToward() || shotCheck.CanFlyTowards))
                {
                    self.FlyTowardHook();
                }
                else if (shotCheck.CanRevoke || shotCheck.CanFlyTowards)
                {
                    if (hook.State == GrapplingHook.HookStates.Emitting)
                    {
                        hook.RecordEmitElapsed();
                    }
                    else if (hook.State == GrapplingHook.HookStates.Fixed)
                    {
                        if (shotCheck.CanFlyTowards && hook.CanFlyToward())
                        {
                            self.FlyTowardHook();
                        }
                        else if (shotCheck.CanRevoke)
                        {
                            hook.Revoke();
                        }
                    }
                }
                else if (hook.State == GrapplingHook.HookStates.Fixed)
                {
                    if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                        self.Speed.Y = -self.Speed.Y;
                    if (!self.onGround && hook.ReachLockedLength(self.Center))
                    {
                        Vector2 ropeDirection = hook.RopeDirection;
                        Vector2 swingDirection = hook.SwingDirection;
                        self.HandleHangingSpeed(dt);
                        float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                        hook.AlongRopeSpeed = speedAlongRope;
                        if (speedAlongRope >= 0.0f)
                        {
                            DynamicData.For(self).Set("rope_is_loosen", false);
                            self.Speed = self.TurnToTangentSpeed(self.Speed, swingDirection);
                        }
                    }
                    else if (hook.ReachLockedLength(self.Center))
                    {
                        Vector2 ropeDirection = hook.RopeDirection;
                        float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                        hook.AlongRopeSpeed = speedAlongRope;
                    }
                    else
                    {
                        hook.AlongRopeSpeed = 0.0f;
                    }
                    if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                        self.Speed.Y = -self.Speed.Y;
                    DynamicData.For(self).Set("fixing_speed", self.Speed);
                }
            }
            return -1;
        }

        private static int PostHookUpdate(Player self)
        {
            if (self.level.GetState() == null)
                return -1;
            if (!self.level.GetState().FeatureEnabled)
                return -1;

            float dt = Engine.DeltaTime;
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                TimeTicker dashHangingTicker = self.GetTimeTicker("dash_hanging_ticker");
                if (self.StateMachine.State == (int)AquaStates.StDash)
                {
                    dashHangingTicker.Tick(dt);
                }
                else if (self.StateMachine.State == (int)AquaStates.StNormal)
                {
                    float length = hook.CalculateRopeLength(self.Center);
                    if (hook.UserLockedLength > 0.0f && length > hook.UserLockedLength)
                    {
                        hook.UserLockedLength = length;
                    }
                }
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    if ((!self.onGround || Input.MoveY.Value < 0) && (self.StateMachine.State != (int)AquaStates.StDash || dashHangingTicker.Check()))
                    {
                        Vector2 ropeDirection = hook.RopeDirection;
                        bool swingUp = IsRopeSwingingUp(ropeDirection);
                        if (swingUp)
                        {
                            return (int)AquaStates.StHanging;
                        }
                        Vector2 swingDirection = hook.SwingDirection;
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        float speedTangent = MathF.Abs(Vector2.Dot(self.Speed, swingDirection));
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        float threshold = SPEED_CHECK_GRAPPLING_SWING_DOWN;
                        if (speedTangent >= threshold)
                        {
                            return (int)AquaStates.StHanging;
                        }
                    }
                }
                if (!self.onGround)
                {
                    // NormalUpdate限制太多，这种情况下强制改为之前计算的速度
                    if (hook.ReachLockedLength(self.Center))
                    {
                        self.Speed = DynamicData.For(self).Get<Vector2>("fixing_speed");
                    }
                }
            }
            return -1;
        }

        private static void HandleHangingSpeed(this Player self, float dt)
        {
            GrapplingHook hook = self.GetGrappleHook();
            float gravity = Player.Gravity * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f);
            self.Speed.Y += gravity * dt;
            self.Speed -= hook.Acceleration * dt * 0.8f;
            self.Speed += self.GetWindSpeed() * dt * self.SceneAs<Level>().GetState().HookSettings.WindCoefficient;
        }

        private static Vector2 GetWindSpeed(this Player self)
        {
            WindController windCtrl = self.Scene.Entities.FindFirst<WindController>();
            if (windCtrl == null)
                return Vector2.Zero;

            return windCtrl.targetSpeed;
        }

        private static float CalculateEmitParameters(this Player self, Vector2 emitSpeed, ref Vector2 direction)
        {
            WindController windCtrl = self.Scene.Entities.FindFirst<WindController>();
            if (windCtrl == null)
                return 1.0f;

            float oldSpeed = emitSpeed.Length();
            Vector2 windSpeed = windCtrl.targetSpeed;
            const float MAX_AFFECT_SPEED = 120.0f;
            emitSpeed.X += windSpeed.X / 800.0f * MAX_AFFECT_SPEED;
            emitSpeed.Y += windSpeed.Y / 800.0f * MAX_AFFECT_SPEED;
            const float MAX_COEFFICIENT = 0.25f;
            float speedCoeff = Calc.Clamp(emitSpeed.Length() / oldSpeed, 1.0f - MAX_COEFFICIENT, 1.0f + MAX_COEFFICIENT);
            direction = Calc.SafeNormalize(emitSpeed);
            return speedCoeff;
            //float maxSpeedUp = 0.25f;
            //direction.X *= (1.0f + windSpeed.X / 800.0f * maxSpeedUp * MathF.Sign(direction.X));
            //direction.Y *= (1.0f + windSpeed.Y / 800.0f * maxSpeedUp * MathF.Sign(direction.Y));
            //float speedCoeff = direction.Length();
            //direction.Normalize();
            //return speedCoeff;
        }

        private static bool IsRopeSwingingUp(Vector2 direction)
        {
            return AquaMaths.Cross(Vector2.UnitX, direction) * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f) > 0.0f;
        }

        private static bool IsExhausted(this Player self)
        {
            return self.CheckStamina <= 0.0f;
        }

        private static void SetupSpecialSwingArguments(this Player self, Vector2 speed)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    Vector2 swingDir = hook.SwingDirection;
                    float sign = MathF.Sign(Vector2.Dot(speed, swingDir));
                    if (AquaMaths.IsApproximateZero(sign))
                    {
                        sign = 0.0f;
                    }
                    self.SetSpecialSwingDirection(sign);
                    self.SetSpecialSwingSpeed(MathF.Max(Player.DashSpeed, speed.Length()));
                }
            }
        }

        private static void UpdateSpecialSwing(this Player self)
        {
            GrapplingHook hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    hook.Revoke();
                    self.Speed = TurnToMoreAccurateSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                    self.SetSpecialSwingDirection(0.0f);
                }
                else
                {
                    if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                    {
                        if (self.GetSpecialSwingDirection() != 0.0f)
                        {
                            Vector2 swingDir = hook.SwingDirection;
                            self.Speed = swingDir * self.GetSpecialSwingDirection() * self.GetSpecialSwingSpeed();
                            if (self.StateMachine.State == (int)AquaStates.StRedDash && ModInterop.GravityHelper.IsPlayerGravityInverted())
                            {
                                // seems red bubble speed won't be handled in GravityHelper
                                self.Speed.Y *= -1.0f;
                            }
                        }
                    }
                    else
                    {
                        self.Speed = TurnToMoreAccurateSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                        self.SetSpecialSwingDirection(0.0f);
                    }
                }
            }
            else
            {
                self.Speed = TurnToMoreAccurateSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                self.SetSpecialSwingDirection(0.0f);
            }
            if (!AquaMaths.IsApproximateZero(self.Speed))
            {
                self.DashDir = Vector2.Normalize(self.Speed);
            }
        }

        private static void RestoreSavedSwingSpeedIfPossible(this Player self, int nextState)
        {
            TimeTicker saveTicker = self.GetTimeTicker("boost_speed_save_ticker");
            GrapplingHook hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    if (nextState == (int)AquaStates.StDash && !saveTicker.Check())
                    {
                        self.Speed = self.GetSavedSwingSpeed();
                        self.SetSavedSwingSpeed(Vector2.Zero);
                    }
                }
            }
            else
            {
                self.SetSavedSwingSpeed(Vector2.Zero);
            }
            saveTicker.Tick(Engine.DeltaTime);
            if (nextState != (int)AquaStates.StBoost && nextState != (int)AquaStates.StRedDash)
            {
                self.SetSavedSwingSpeed(Vector2.Zero);
            }
        }

        private static Vector2 GetSavedSwingSpeed(this Player self)
        {
            return DynamicData.For(self).Get<Vector2>("saved_swing_speed");
        }

        private static void SetSavedSwingSpeed(this Player self, Vector2 speed)
        {
            DynamicData.For(self).Set("saved_swing_speed", speed);
        }

        private static float GetSpecialSwingDirection(this Player self)
        {
            return DynamicData.For(self).Get<float>("red_dash_swing_dir");
        }

        private static void SetSpecialSwingDirection(this Player self, float direction)
        {
            DynamicData.For(self).Set("red_dash_swing_dir", direction);
        }

        private static float GetSpecialSwingSpeed(this Player self)
        {
            return DynamicData.For(self).Get<float>("red_dash_swing_speed");
        }

        private static void SetSpecialSwingSpeed(this Player self, float speed)
        {
            DynamicData.For(self).Set("red_dash_swing_speed", speed);
        }

        private static readonly int[] PERMIT_SHOOT_STATES =
        {
            (int)AquaStates.StNormal,
            (int)AquaStates.StDash,
            (int)AquaStates.StLaunch,
        };

        private static KeyValuePair<float, Vector2>[] SWING_JUMP_ACCURACY_RANGE_LIST =
        {
            new KeyValuePair<float, Vector2>(10.0f * Calc.DegToRad, Vector2.UnitX),
            new KeyValuePair<float, Vector2>(30.0f * Calc.DegToRad, new Vector2(MathF.Cos(22.5f * Calc.DegToRad), MathF.Sin(22.5f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(55.0f * Calc.DegToRad, new Vector2(MathF.Cos(45.0f * Calc.DegToRad), MathF.Sin(45.0f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(75.0f * Calc.DegToRad, new Vector2(MathF.Cos(67.5f * Calc.DegToRad), MathF.Sin(67.5f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(90.0f * Calc.DegToRad, Vector2.UnitY),
        };

        private static KeyValuePair<float, Vector2>[] UNIFORM_ACCURACY_RANGE_LIST =
        {
            new KeyValuePair<float, Vector2>(11.25f * Calc.DegToRad, Vector2.UnitX),
            new KeyValuePair<float, Vector2>(33.75f * Calc.DegToRad, new Vector2(MathF.Cos(22.5f * Calc.DegToRad), MathF.Sin(22.5f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(56.25f * Calc.DegToRad, new Vector2(MathF.Cos(45.0f * Calc.DegToRad), MathF.Sin(45.0f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(78.75f * Calc.DegToRad, new Vector2(MathF.Cos(67.5f * Calc.DegToRad), MathF.Sin(67.5f * Calc.DegToRad))),
            new KeyValuePair<float, Vector2>(90.0f * Calc.DegToRad, Vector2.UnitY),
        };
    }
}
