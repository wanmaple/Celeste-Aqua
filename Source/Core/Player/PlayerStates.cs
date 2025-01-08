using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using MonoMod.Utils;
using Mono.Cecil;
using Celeste.Mod.Aqua.Debug;
using System.Linq;
using System.IO.Pipes;

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
            On.Celeste.Player.BoostBegin += Player_BoostBegin;
            On.Celeste.Player.RedDashBegin += Player_RedDashBegin;
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
            On.Celeste.Player.SummitLaunchBegin += Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin += Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin += Player_FlingBirdBegin;
            On.Celeste.Player.CassetteFlyBegin += Player_CassetteFlyBegin;
            On.Celeste.Player.PickupCoroutine += Player_PickupCoroutine;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.WindMove += Player_WindMove;
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
            On.Celeste.Player.BoostBegin -= Player_BoostBegin;
            On.Celeste.Player.RedDashBegin -= Player_RedDashBegin;
            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
            On.Celeste.Player.SummitLaunchBegin -= Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin -= Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin -= Player_FlingBirdBegin;
            On.Celeste.Player.CassetteFlyBegin -= Player_CassetteFlyBegin;
            On.Celeste.Player.PickupCoroutine -= Player_PickupCoroutine;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.WindMove -= Player_WindMove;
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

            self.StateMachine.SetCallbacks((int)AquaStates.StHanging, self.Player_HangingUpdate, null, self.Player_HangingBegin, self.Player_HangingEnd);
            self.StateMachine.SetStateName((int)AquaStates.StHanging, "Hanging");
            self.StateMachine.SetCallbacks((int)AquaStates.StElectricShocking, self.Player_ElectricShockingUpdate, null, self.Player_ElectricShockingBegin);
            self.StateMachine.SetStateName((int)AquaStates.StElectricShocking, "ElectricShocking");
            DynamicData.For(self).Set("start_emitting", false);
            self.SetTimeTicker("bullet_time_ticker", 0.0f);
            self.SetTimeTicker("dash_hanging_ticker", 0.05f);
            self.SetTimeTicker("emit_ticker", 0.05f);
            self.SetTimeTicker("elec_shock_ticker", 1.0f);
            DynamicData.For(self).Set("rope_is_loosen", true);
            DynamicData.For(self).Set("is_booster_dash", false);
            self.SetSlideState(SlideStates.None);
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

        private static void Player_DreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_RedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_BoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
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
        }

        private static int Player_HangingUpdate(this Player self)
        {
            float dt = Engine.DeltaTime;
            var hook = self.GetGrappleHook();
            var shotCheck = self.GetShootHookCheck();
            Vector2 ropeDirection = hook.RopeDirection;
            Vector2 swingDirection = hook.SwingDirection;
            bool swingUp = IsRopeSwingingUp(ropeDirection);
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
                FlyTowardHook(self);
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
            else if (Input.GrabCheck && self.ClimbCheck((int)self.Facing))
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
                if (!PERMIT_SHOOT_STATES.Contains(self.StateMachine.State))
                {
                    DynamicData.For(self).Set("start_emitting", false);
                }
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
            var hook = self.GetGrappleHook();
            hook.Revoke();
            var levelState = self.SceneAs<Level>().GetState();
            const float X_LIFT = 0.0f;
            const float Y_LIFT = 1.1f;
            self.LiftSpeed = self.Speed * new Vector2(X_LIFT, Y_LIFT * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f));
            self.Jump(false);
            float staminaCost = levelState.HookSettings.SwingJumpStaminaCost;
            self.Stamina = MathF.Max(0.0f, self.Stamina - staminaCost);
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
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;
            var hook = self.GetGrappleHook();
            Vector2 ropeDirection = hook.RopeDirection;
            var levelState = self.level.GetState();
            float origAlongSpeed = MathF.Max(Vector2.Dot(self.Speed, -ropeDirection), 0.0f);
            self.Speed = -ropeDirection * MathF.Min(self.SceneAs<Level>().GetState().HookSettings.FlyTowardSpeed + origAlongSpeed, levelState.HookSettings.MaxLineSpeed);
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;
        }

        private static int PreHookUpdate(Player self)
        {
            if (!self.level.GetState().FeatureEnabled)
                return -1;

            var hook = self.GetGrappleHook();
            var shotCheck = self.GetShootHookCheck();
            float dt = Engine.DeltaTime;
            if (DynamicData.For(self).Get<bool>("start_emitting"))
            {
                TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
                emittingTicker.Tick(Engine.RawDeltaTime);
                if (emittingTicker.CheckRate(0.5f))
                {
                    if (!hook.Active)
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
                        if (DynamicData.For(self).Get<bool>("backward_down_shoot"))
                        {
                            direction = new Vector2(-(int)self.Facing, 1.0f);
                            direction.Normalize();
                        }
                        else if (Input.Aim.Value != Vector2.Zero)
                        {
                            direction = Input.GetAimVector(self.Facing);
                        }
                        direction.Y *= ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
                        float emitSpeed = self.SceneAs<Level>().GetState().HookSettings.EmitSpeed;
                        float emitSpeedCoeff = self.CalculateEmitParameters(emitSpeed * direction, ref direction);
                        if (self.CanEmitHook(direction) && hook.CanEmit(self.level))
                        {
                            hook.Emit(self.level, direction, emitSpeed, emitSpeedCoeff - 1.0f);
                            self.Scene.Add(hook);
                        }
                    }
                    if (emittingTicker.CheckRate(1.0f))
                    {
                        DynamicData.For(self).Set("start_emitting", false);
                        Engine.TimeRateB = 1.0f;
                    }
                }
                return -1;
            }
            if (!hook.Active && (shotCheck.CanThrow || AquaModule.Settings.BackwardDownShoot.Pressed) && !self.IsExhausted() && self.Holding == null)
            {
                Engine.TimeRateB = 0.1f;
                DynamicData.For(self).Set("start_emitting", true);
                DynamicData.For(self).Set("backward_down_shoot", AquaModule.Settings.BackwardDownShoot.Pressed);
                TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
                emittingTicker.Reset();
                return -1;
            }

            if (hook.Active)
            {
                if (hook.State == GrapplingHook.HookStates.Fixed && self.IsExhausted())
                {
                    hook.Revoke();
                }
                else if (hook.JustFixed && hook.CanFlyToward())
                {
                    FlyTowardHook(self);
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
                            FlyTowardHook(self);
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
                if ((!self.onGround || Input.MoveY.Value < 0) && (self.StateMachine.State != (int)AquaStates.StDash || dashHangingTicker.Check()) && (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible))
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
                    if (speedTangent >= SPEED_CHECK_GRAPPLING_SWING_DOWN)
                    {
                        return (int)AquaStates.StHanging;
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

        private static bool CanEmitHook(this Player self, Vector2 direction)
        {
            //if (self.Ducking)
            //    return false;
            //if (self.onGround && direction.Y > 0)
            //    return false;
            return true;
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
            direction = Vector2.Normalize(emitSpeed);
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
            return AquaMaths.Cross(Vector2.UnitX, direction) * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f) >= 0.0f;
        }

        private static bool IsExhausted(this Player self)
        {
            return self.CheckStamina <= 0.0f;
        }

        private static readonly int[] PERMIT_SHOOT_STATES =
        {
            (int)AquaStates.StNormal,
            (int)AquaStates.StDash,
            (int)AquaStates.StLaunch,
        };
    }
}
