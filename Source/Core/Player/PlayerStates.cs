using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using MonoMod.Utils;
using Mono.Cecil;
using System.Collections.Generic;
using Celeste.Mod.Aqua.Debug;

namespace Celeste.Mod.Aqua.Core
{
    public enum AquaStates
    {
        StNormal = 0, // Normal
        StClimb = 1, // Climbing
        StDash = 2, // Dashing
        StSwim = 3, // Swimming
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
        StSwing = 26,
        StElectricShock,

        MaxStates,
    }

    public static partial class PlayerStates
    {
        public const float SPEED_CHECK_GRAPPLING_SWING_DOWN = 250.0f;
        public const float SPEED_CHECK_2_GRAPPLING_SWING_DOWN = 160.0f;
        public const float SPEED_CHECK_GRAPPLING_SWING_UP = 90.0f;
        public const float SAVED_SWING_SPEED = 325.0f;

        public static void Initialize()
        {
            IL.Celeste.Player.ctor += Player_ILConstruct;
            IL.Celeste.Player.NormalUpdate += Player_ILNormalUpdate;
            IL.Celeste.Player.OnCollideV += Player_ILOnCollideV;
            IL.Celeste.Player.ClimbUpdate += Player_ILClimbUpdate;
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.NormalUpdate += Player_NormalUpdate;
            On.Celeste.Player.DashBegin += Player_DashBegin;
            On.Celeste.Player.DashEnd += Player_DashEnd;
            On.Celeste.Player.DashUpdate += Player_DashUpdate;
            On.Celeste.Player.SwimUpdate += Player_SwimUpdate;
            On.Celeste.Player.LaunchUpdate += Player_LaunchUpdate;
            On.Celeste.Player.BoostUpdate += Player_BoostUpdate;
            On.Celeste.Player.RedDashBegin += Player_RedDashBegin;
            On.Celeste.Player.RedDashEnd += Player_RedDashEnd;
            On.Celeste.Player.RedDashUpdate += Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine += Player_RedDashCoroutine;
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
            On.Celeste.Player.DreamDashEnd += Player_DreamDashEnd;
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
            IL.Celeste.Player.ClimbUpdate -= Player_ILClimbUpdate;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashBegin -= Player_DashBegin;
            On.Celeste.Player.DashEnd -= Player_DashEnd;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.SwimUpdate -= Player_SwimUpdate;
            On.Celeste.Player.LaunchUpdate -= Player_LaunchUpdate;
            On.Celeste.Player.BoostUpdate -= Player_BoostUpdate;
            On.Celeste.Player.RedDashBegin -= Player_RedDashBegin;
            On.Celeste.Player.RedDashEnd -= Player_RedDashEnd;
            On.Celeste.Player.RedDashUpdate -= Player_RedDashUpdate;
            On.Celeste.Player.RedDashCoroutine -= Player_RedDashCoroutine;
            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
            On.Celeste.Player.DreamDashEnd -= Player_DreamDashEnd;
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
                cursor.EmitLdcI4((int)AquaStates.MaxStates - (int)AquaStates.StSwing);
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
            if (hook != null && hook.Active && hook.IsShooting)
            {
                return true;
            }
            return false;
        }

        private static float CalculateNormalMoveCoefficient(this Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.IsShooting)
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
                cursor.EmitLdcI4((int)AquaStates.StSwing);
                cursor.EmitBeq(label);
            }
        }

        private static void Player_Construct(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            self.UninitializeGrapplingHook();
            self.StateMachine.SetCallbacks((int)AquaStates.StLaunch, self.LaunchUpdate, null, self.LaunchBegin, self.Player_LaunchEnd);
            self.StateMachine.SetCallbacks((int)AquaStates.StSwing, self.Player_SwingUpdate, null, self.Player_SwingBegin, self.Player_SwingEnd);
            self.StateMachine.SetStateName((int)AquaStates.StSwing, "Swing");
            self.StateMachine.SetCallbacks((int)AquaStates.StElectricShock, self.Player_ElectricShockingUpdate, null, self.Player_ElectricShockingBegin);
            self.StateMachine.SetStateName((int)AquaStates.StElectricShock, "ElectricShocking");
            self.SetTimeTicker("dash_hanging_ticker", 0.05f);
            self.SetTimeTicker("boost_speed_save_ticker", 0.5f);
            self.SetTimeTicker("swing_jump_keeping_ticker", 0.1f);
            self.SetTimeTicker("elec_shock_ticker", 0.5f);
            DynamicData.For(self).Set("lift_speed_y", 0.0f);
            DynamicData.For(self).Set("rope_is_loosen", true);
            DynamicData.For(self).Set("is_booster_dash", false);
            self.SetSlideState(SlideStates.None);
            self.SetSavedSwingSpeed(Vector2.Zero);
            self.SetSpecialSwingDirection(0.0f);
            self.SetSpecialSwingSpeed(Player.DashSpeed);
            self.SetHookable(false);
            self.Add(new GrappleRelatedFields());
            var shootCheck = new ShotHookCheck(AquaModule.Settings.ThrowHookMode);
            DynamicData.For(self).Set("shoot_check", shootCheck);
            Component gravityListener = ModInterop.GravityHelper.CreatePlayerGravityListener(OnGravityChanged);
            if (gravityListener != null)
                self.Add(gravityListener);
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            if (scene is Level level)
            {
                var state = level.GetState();
                Player player = scene.Tracker.GetEntity<Player>();
                if (state != null && player != null && self != player)
                {
                    // possibly a cloned player.
                    AquaDebugger.LogInfo("INIT GRAPPLE CLONED?");
                    self.InitializeGrapplingHook(GrapplingHook.HOOK_SIZE, state.HookSettings.RopeLength, state.RopeMaterial, state.GameplayMode, state.InitialShootCount);
                }
            }
            DynamicData.For(self).Set("previous_facing", (int)self.Facing);
        }

        private static void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active)
            {
                scene.Remove(hook);
            }
            var state = (scene as Level).GetState();
            if (state != null && hook != null)
            {
                state.RestShootCount = state.InitialShootCount;
                hook.ChangeGameplayMode(state.GameplayMode, scene as Level, state.InitialShootCount);
            }

            orig(self, scene);
        }

        private static int Player_BoostUpdate(On.Celeste.Player.orig_BoostUpdate orig, Player self)
        {
            int nextState = orig(self);
            GrapplingHook grapple = self.GetGrappleHook();
            ShotHookCheck shotCheck = self.GetShootHookCheck();
            if (shotCheck != null && grapple != null && grapple.Active && grapple.State == GrapplingHook.HookStates.Fixed && shotCheck.CanRevoke)
            {
                grapple.Revoke();
            }
            self.RestoreSavedSwingSpeedIfPossible(nextState);
            return nextState;
        }

        private static void Player_RedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self)
        {
            Vector2 enterSpeed = self.Speed;
            orig(self);
            if (self.level.GetState() == null)
                return;
            if (!self.level.GetState().FeatureEnabled)
                return;
            bool isSpecialBooster = false;
            if (self.CurrentBooster != null && ModInterop.CurvedBoosterTypes != null)
            {
                foreach (Type type in ModInterop.CurvedBoosterTypes)
                {
                    if (self.CurrentBooster.GetType().IsAssignableTo(type))
                    {
                        isSpecialBooster = true;
                        break;
                    }
                }
            }
            if (isSpecialBooster)
            {
                var hook = self.GetGrappleHook();
                if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
                {
                    hook.Revoke();
                }
                self.SetSpecialSwingDirection(0.0f);
            }
            else
            {
                Vector2 speed = enterSpeed;
                if (AquaMaths.IsApproximateZero(speed))
                {
                    speed = self.Speed;
                }
                if (AquaMaths.IsApproximateZero(speed))
                {
                    speed = (int)self.Facing * Player.DashSpeed * Vector2.UnitX;
                }
                SetupSpecialSwingArguments(self, speed);
                var hook = self.GetGrappleHook();
                if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
                {
                    if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                    {
                        if (self.GetSpecialSwingDirection() != 0.0f)
                            hook.SetRopeLengthLocked(true, self.ExactCenter());
                    }
                }
            }
        }

        private static void Player_RedDashEnd(On.Celeste.Player.orig_RedDashEnd orig, Player self)
        {
            orig(self);
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    hook.SetRopeLengthLocked(false, self.ExactCenter());
                    self.TrySaveSwingSpeed();
                }
            }
        }

        private static int Player_RedDashUpdate(On.Celeste.Player.orig_RedDashUpdate orig, Player self)
        {
            int nextState = orig(self);
            if (self.level.GetState() == null)
                return nextState;
            if (!self.level.GetState().FeatureEnabled)
                return nextState;
            if (nextState == (int)AquaStates.StRedDash && !AquaMaths.IsApproximateZero(self.Speed) && self.GetSpecialSwingDirection() != 0.0f)
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
            if (self.level.GetState() == null)
                return;
            if (!self.level.GetState().FeatureEnabled)
                return;
            SetupSpecialSwingArguments(self, self.Speed);
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    if (self.GetSpecialSwingDirection() != 0.0f)
                        hook.SetRopeLengthLocked(true, self.ExactCenter());
                }
            }
        }

        private static void Player_DreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self)
        {
            orig(self);
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    hook.SetRopeLengthLocked(false, self.ExactCenter());
                }
            }
        }

        private static int Player_DreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            int nextState = orig(self);
            if (self.level.GetState() == null)
                return nextState;
            if (!self.level.GetState().FeatureEnabled)
                return nextState;
            if (nextState == (int)AquaStates.StDreamDash && self.GetSpecialSwingDirection() != 0.0f)
            {
                self.UpdateSpecialSwing();
            }
            return nextState;
        }

        private static void Player_FlingBirdBegin(On.Celeste.Player.orig_FlingBirdBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_CassetteFlyBegin(On.Celeste.Player.orig_CassetteFlyBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static System.Collections.IEnumerator Player_PickupCoroutine(On.Celeste.Player.orig_PickupCoroutine orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            return orig(self);
        }

        private static void Player_StarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                hook.Revoke();
            }

            orig(self);
        }

        private static void Player_SummitLaunchBegin(On.Celeste.Player.orig_SummitLaunchBegin orig, Player self)
        {
            var hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
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

        private static int Player_SwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self)
        {
            int nextState = PreHookUpdate(self);
            if (nextState < 0)
            {
                nextState = orig(self);
            }
            if (nextState == (int)AquaStates.StSwim)
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
            if (self.StateMachine.State == (int)AquaStates.StSwing)
            {
                TimeTicker keepTicker = self.GetTimeTicker("swing_jump_keeping_ticker");
                keepTicker.Reset();
                DynamicData.For(self).Set("lift_speed_y", MathF.Min(self.Speed.Y, 0.0f));
            }
        }

        private static void Player_SwingBegin(this Player self)
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

        private static void Player_SwingEnd(this Player self)
        {
            var hook = self.GetGrappleHook();
            hook.SetRopeLengthLocked(false, self.Center);
            DynamicData.For(self).Set("climb_rope_direction", 0);
            DynamicData.For(self).Set("lift_speed_y", 0.0f);
            if (self.StateMachine.State == (int)AquaStates.StBoost || self.StateMachine.State == (int)AquaStates.StRedDash)
            {
                self.TrySaveSwingSpeed();
            }
        }

        private static int Player_SwingUpdate(this Player self)
        {
            var state = self.level.GetState();
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
            else if (self.SwimCheck())
            {
                return (int)AquaStates.StSwim;
            }
            else if (self.IsExhausted())
            {
                hook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (state != null && !state.DisableGrappleBoost && hook.CanGrappleBoost())
            {
                self.GrappleBoost();
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
            else if (Input.GrabCheck && self.ClimbCheck((int)self.Facing) && !SaveData.Instance.Assists.NoGrabbing && !self.DashAttacking)
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
                if (speedTangent >= SPEED_CHECK_2_GRAPPLING_SWING_DOWN * self.CalculateDownGrapplingThresholdCoefficient())
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
            self.HandleSwingSpeed(dt);
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
                Vector2 strengthSpeed = (state == null ? AquaModule.Settings.HookSettings.SwingStrength : state.HookSettings.SwingStrength) * swingDirection * Input.MoveX.Value * dt * sign;
                strengthSpeed *= Calc.Clamp(hook.SwingRadius / hook.MaxLength, 0.1f, 1.0f);
                self.Speed += strengthSpeed;
            }
            if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                self.Speed.Y = -self.Speed.Y;

            return (int)AquaStates.StSwing;
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
            return (int)AquaStates.StElectricShock;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (MInput.Keyboard.Pressed(Microsoft.Xna.Framework.Input.Keys.Space))
            {
                Celeste.Freeze(0.1f);
                return;
            }
            if (self.StateMachine.State == (int)AquaStates.StElectricShock)
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

                var grapple = self.GetGrappleHook();
                var shotCheck = self.GetShootHookCheck();
                if (grapple != null && grapple.Active && grapple.Revoked)
                {
                    self.Scene.Remove(grapple);
                    shotCheck.EndPeriod();
                }
                else if (grapple != null && grapple.Active)
                {
                    // NerdHelper's NodedFlingBird compatibility.
                    if (self.StateMachine.State == 30)
                    {
                        if (grapple != null && grapple.Active && grapple.State != GrapplingHook.HookStates.Revoking)
                        {
                            grapple.Revoke();
                        }
                    }
                    if (self.Holding != null && grapple.State != GrapplingHook.HookStates.Revoking)
                    {
                        grapple.Revoke();
                    }
                    else if (grapple.State == GrapplingHook.HookStates.Fixed || grapple.State == GrapplingHook.HookStates.Attracted)
                    {
                        if (grapple.EnforcePlayer(self, new Segment(grapple.PlayerPreviousPosition, self.ExactCenter()), Engine.DeltaTime))
                        {
                            grapple.Revoke();
                            if (self.StateMachine.State == (int)AquaStates.StSwing)
                            {
                                self.StateMachine.ForceState((int)AquaStates.StNormal);
                            }
                        }
                    }
                    if (grapple.State == GrapplingHook.HookStates.Fixed)
                    {
                        if (self.StateMachine.State == (int)AquaStates.StSwing)
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
            }
            self.GetShootHookCheck().Update();
            {
                GrapplingHook hook = self.GetGrappleHook();
                if (hook != null)
                {
                    if ((!Input.GrabCheck && !AquaModule.Settings.AutoGrabRopeIfPossible) || self.onGround || !hook.Active || hook.State != GrapplingHook.HookStates.Fixed || (self.StateMachine.State != (int)AquaStates.StNormal && self.StateMachine.State != (int)AquaStates.StSwing))
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
            if (hook != null && hook.Active && hook.State != GrapplingHook.HookStates.Revoking)
            {
                Vector2 ropeDirection = hook.RopeDirection;
                bool swingUp = IsRopeSwingingUp(ropeDirection);
                if (swingUp)
                {
                    hook.Revoke();
                }
                else
                {
                    Vector2 swingDirection = hook.SwingDirection;
                    if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                        self.Speed.Y = -self.Speed.Y;
                    float speedTangent = MathF.Abs(Vector2.Dot(self.Speed, swingDirection));
                    if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                        self.Speed.Y = -self.Speed.Y;
                    if (speedTangent >= SPEED_CHECK_GRAPPLING_SWING_DOWN * self.CalculateDownGrapplingThresholdCoefficient())
                    {
                        hook.Revoke();
                    }
                }
            }

            orig(self);
        }

        private static void Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self)
        {
            if (self.StateMachine.State != (int)AquaStates.StSwing)
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
            float yLiftCoefficient = 1.0f;
            if (self.Holding != null && self.Holding.Entity is Actor actor)
            {
                float mass = actor.GetMass();
                if (mass == float.PositiveInfinity)
                {
                    yLiftCoefficient = 0.0f;
                }
                else if (AquaMaths.IsApproximateZero(mass))
                {
                    yLiftCoefficient = 1.0f;
                }
                else
                {
                    float split = MathF.Ceiling(mass / 0.5f);
                    yLiftCoefficient = 1.0f - MathHelper.Clamp(split / MAX_INFINITE_MASS, 0.0f, 1.0f);
                }
            }
            float keepedLiftY = DynamicData.For(self).Get<float>("lift_speed_y");
            Vector2 speed = new Vector2(self.Speed.X, MathF.Min(self.Speed.Y, keepedLiftY));
            speed = TurnToConsistentSpeed(speed, SWING_JUMP_ACCURACY_RANGE_LIST);
            self.LiftSpeed = speed * new Vector2(X_LIFT, Y_LIFT * yLiftCoefficient * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f));
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

        private static Vector2 TurnToConsistentSpeed(Vector2 speed, KeyValuePair<float, Vector2>[] accuracyConfig)
        {
            speed = Calc.SafeNormalize(speed) * MathF.Round(speed.Length() / 10.0f) * 10.0f;
            return AquaMaths.TurnToSpecificSpeed(speed, SWING_JUMP_ACCURACY_RANGE_LIST);
        }

        private static Vector2 TurnToTangentSpeed(this Player self, Vector2 speed, Vector2 swingDirection)
        {
            float lineSpeed = Vector2.Dot(speed, swingDirection);
            var levelState = self.SceneAs<Level>().GetState();
            int sign = MathF.Sign(lineSpeed);
            if (MathF.Abs(lineSpeed) > levelState.HookSettings.MaxLineSpeed)
            {
                if (sign > 0)
                {
                    lineSpeed = MathF.Max(lineSpeed - 120.0f, levelState.HookSettings.MaxLineSpeed);
                }
                else
                {
                    lineSpeed = MathF.Min(lineSpeed + 120.0f, -levelState.HookSettings.MaxLineSpeed);
                }
            }
            //lineSpeed = Calc.Clamp(lineSpeed, -levelState.HookSettings.MaxLineSpeed, levelState.HookSettings.MaxLineSpeed);
            return lineSpeed * swingDirection;
        }

        private static void GrappleBoost(this Player self)
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
            Audio.Play("event:/char/madeline/jump_superslide", self.Center);
            Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
        }

        private static int PreHookUpdate(Player self)
        {
            if (self.level.GetState() == null)
                return -1;
            if (!self.level.GetState().FeatureEnabled)
                return -1;

            var grapple = self.GetGrappleHook();
            if (grapple == null)
                return -1;
            var shotCheck = self.GetShootHookCheck();
            float dt = Engine.DeltaTime;
            bool downGrapplePressed = AquaModule.Settings.DownShoot.Pressed;
            if (!grapple.Active && (shotCheck.CanThrow || downGrapplePressed) && !self.IsExhausted() && self.Holding == null && grapple.CanEmit(self.level))
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
                if (downGrapplePressed)
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
                grapple.Emit(self.level, direction, emitSpeed, emitSpeedCoeff - 1.0f);
                self.Scene.Add(grapple);
            }

            if (grapple.Active)
            {
                if (grapple.State == GrapplingHook.HookStates.Fixed && self.IsExhausted())
                {
                    grapple.Revoke();
                }
                else if (self.Holding != null && grapple.State == GrapplingHook.HookStates.Fixed && Input.Jump.Pressed)
                {
                    self.SwingJump(dt);
                }
                else if (!self.level.GetState().DisableGrappleBoost && grapple.CanGrappleBoost())
                {
                    self.GrappleBoost();
                }
                else if (grapple.IsShooting)
                {
                    if (shotCheck.CanGrappleBoost)
                    {
                        grapple.RecordEmitElapsed();
                    }
                }
                else if (grapple.State == GrapplingHook.HookStates.Fixed)
                {
                    if (!self.level.GetState().DisableGrappleBoost && grapple.CanGrappleBoost())
                    {
                        self.GrappleBoost();
                    }
                    else if (shotCheck.CanRevoke)
                    {
                        grapple.Revoke();
                    }
                    else
                    {
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        if (!self.onGround && grapple.ReachLockedLength(self.Center))
                        {
                            Vector2 ropeDirection = grapple.RopeDirection;
                            Vector2 swingDirection = grapple.SwingDirection;
                            self.HandleSwingSpeed(dt);
                            float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                            grapple.AlongRopeSpeed = speedAlongRope;
                            if (speedAlongRope >= 0.0f)
                            {
                                DynamicData.For(self).Set("rope_is_loosen", false);
                                self.Speed = self.TurnToTangentSpeed(self.Speed, swingDirection);
                            }
                        }
                        else if (grapple.ReachLockedLength(self.Center))
                        {
                            Vector2 ropeDirection = grapple.RopeDirection;
                            float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                            grapple.AlongRopeSpeed = speedAlongRope;
                        }
                        else
                        {
                            grapple.AlongRopeSpeed = 0.0f;
                        }
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        GrappleRelatedFields com = self.Get<GrappleRelatedFields>();
                        com.HasFixingSpeed = true;
                        com.FixingSpeed = self.Speed;
                    }
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
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
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
                    if ((!self.onGround || Input.MoveY.Value < 0) && self.Holding == null && (self.StateMachine.State != (int)AquaStates.StDash || dashHangingTicker.Check()) && !self.SwimCheck())
                    {
                        Vector2 ropeDirection = hook.RopeDirection;
                        bool swingUp = IsRopeSwingingUp(ropeDirection);
                        if (swingUp)
                        {
                            return (int)AquaStates.StSwing;
                        }
                        Vector2 swingDirection = hook.SwingDirection;
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        float speedTangent = MathF.Abs(Vector2.Dot(self.Speed, swingDirection));
                        if (ModInterop.GravityHelper.IsPlayerGravityInverted())
                            self.Speed.Y = -self.Speed.Y;
                        float threshold = SPEED_CHECK_GRAPPLING_SWING_DOWN * self.CalculateDownGrapplingThresholdCoefficient();
                        if (speedTangent >= threshold)
                        {
                            return (int)AquaStates.StSwing;
                        }
                    }
                }
                if (!self.onGround)
                {
                    GrappleRelatedFields com = self.Get<GrappleRelatedFields>();
                    if (hook.ReachLockedLength(self.Center) && com.HasFixingSpeed)
                    {
                        self.Speed = com.FixingSpeed;
                    }
                }
            }
            self.Get<GrappleRelatedFields>().HasFixingSpeed = false;
            return -1;
        }

        private static void HandleSwingSpeed(this Player self, float dt)
        {
            GrapplingHook hook = self.GetGrappleHook();
            float gravity = Player.Gravity * (ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f) * ModInterop.ExtendedVariants.GetCurrentGravityMultiplier();
            self.Speed.Y += gravity * dt;
            self.Speed -= hook.Acceleration * dt * 0.5f;
            self.Speed += self.GetWindSpeed() * dt;
        }

        private static void TrySaveSwingSpeed(this Player self)
        {
            GrapplingHook grapple = self.GetGrappleHook();
            if (grapple != null)
            {
                Vector2 swingDirection = grapple.SwingDirection;
                Vector2 tangentSpd = self.TurnToTangentSpeed(self.Speed, swingDirection);
                if (tangentSpd.LengthSquared() > SPEED_CHECK_GRAPPLING_SWING_DOWN * SPEED_CHECK_GRAPPLING_SWING_DOWN)
                {
                    self.SetSavedSwingSpeed(TurnToConsistentSpeed(Vector2.Normalize(self.Speed) * SAVED_SWING_SPEED, UNIFORM_ACCURACY_RANGE_LIST));
                }
                else
                {
                    self.SetSavedSwingSpeed(Vector2.Zero);
                }
                TimeTicker saveTicker = self.GetTimeTicker("boost_speed_save_ticker");
                saveTicker.Reset();
            }
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
                else
                {
                    self.SetSpecialSwingDirection(0.0f);
                }
            }
            else
            {
                self.SetSpecialSwingDirection(0.0f);
            }
        }

        private static void UpdateSpecialSwing(this Player self)
        {
            GrapplingHook hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed)
            {
                if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    hook.SetRopeLengthLocked(false, self.ExactCenter());
                    hook.Revoke();
                    self.Speed = TurnToConsistentSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                    self.SetSpecialSwingDirection(0.0f);
                }
                else
                {
                    if ((Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible) && self.GetSpecialSwingDirection() != 0.0f)
                    {
                        // Make player follow the attached entity.
                        Entity attached = hook.AttachedEntity;
                        Vector2 movement = attached.Position - attached.GetPreviousPosition();
                        if (!AquaMaths.IsApproximateZero(movement))
                        {
                            self.MoveH(movement.X, self.OnCollideH);
                            self.MoveV(movement.Y, self.OnCollideV);
                        }
                        Vector2 swingDir = hook.SwingDirection;
                        self.Speed = swingDir * self.GetSpecialSwingDirection() * self.GetSpecialSwingSpeed();
                        if (self.StateMachine.State == (int)AquaStates.StRedDash && ModInterop.GravityHelper.IsPlayerGravityInverted())
                        {
                            // seems red bubble speed won't be handled in GravityHelper
                            self.Speed.Y *= -1.0f;
                        }
                    }
                    else
                    {
                        hook.SetRopeLengthLocked(false, self.ExactCenter());
                        self.Speed = TurnToConsistentSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                        self.SetSpecialSwingDirection(0.0f);
                    }
                }
                if (!AquaMaths.IsApproximateZero(self.Speed))
                {
                    self.DashDir = Vector2.Normalize(self.Speed);
                }
            }
            else
            {
                if (self.GetSpecialSwingDirection() != 0.0f)
                {
                    self.Speed = TurnToConsistentSpeed(self.Speed, UNIFORM_ACCURACY_RANGE_LIST);
                    self.SetSpecialSwingDirection(0.0f);
                    if (!AquaMaths.IsApproximateZero(self.Speed))
                    {
                        self.DashDir = Vector2.Normalize(self.Speed);
                    }
                }
            }
        }

        private static void RestoreSavedSwingSpeedIfPossible(this Player self, int nextState)
        {
            TimeTicker saveTicker = self.GetTimeTicker("boost_speed_save_ticker");
            GrapplingHook hook = self.GetGrappleHook();
            if (hook != null && hook.Active && hook.State == GrapplingHook.HookStates.Fixed && !AquaMaths.IsApproximateZero(self.GetSavedSwingSpeed()))
            {
                if (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    if ((nextState == (int)AquaStates.StDash || nextState == (int)AquaStates.StRedDash) && !saveTicker.Check())
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

        private static void OnGravityChanged(Player player, int gravity, float momentumModifier)
        {
            if (player == null)
                return;
            if (player.StateMachine.State == (int)AquaStates.StRedDash || player.StateMachine.State == (int)AquaStates.StDreamDash)
            {
                player.SetSpecialSwingDirection(-player.GetSpecialSwingDirection());
                player.SetSpecialSwingSpeed(player.GetSpecialSwingSpeed() * momentumModifier);
            }
        }

        private static float CalculateDownGrapplingThresholdCoefficient(this Player self)
        {
            float coeff = ModInterop.ExtendedVariants.GetCurrentGravityMultiplier();
            Vector2 windSpeed = self.GetWindSpeed();
            float ratio = MathHelper.Clamp(windSpeed.Y / Player.Gravity, -1.0f, 1.0f);
            return coeff * (1.0f + ratio);
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
