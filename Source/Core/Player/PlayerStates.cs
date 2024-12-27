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

namespace Celeste.Mod.Aqua.Core
{
    public enum AquaStates
    {
        StNormal = 0, // 正常
        StClimb = 1, // 攀爬
        StDash = 2, // 冲刺
        StBoost = 4, // 绿泡泡中
        StRedDash = 5, // 红泡泡中
        StHitSquash = 6, // 红泡泡撞墙或撞地
        StLaunch = 7, // 被弹球、鱼弹开
        StPickup = 8, // 捡起抓取物
        StDreamDash = 9, // 穿果冻
        StSummitLaunch = 10, // badeline最后一次上抛
        StDummy = 11, // 剧情过场状态
        StIntroWalk = 12, // Walk类型的Intro
        StIntroJump = 13, // Jump类型的Intro(1a)
        StIntroRespawn = 14, // Respawn类型的Intro(重生)
        StIntroWakeUp = 15, // WakeUp类型的Intro(2a)
        StBirdDashTutorial = 16, // 序章教冲刺时冲刺结束后进入的状态
        StFrozen = 17, // 暂停的状态？
        StReflectionFall = 18, // 6a-2掉落剧情段
        StStarFly = 19, // 羽毛飞行
        StTempleFall = 20, // 5a镜子后的掉落段
        StCassetteFly = 21, // 捡到磁带后的泡泡包裹段
        StAttract = 22, // 6a badeline boss靠近时的吸引段
        StIntroMoonJump = 23, // 9a开场上升剧情段
        StFlingBird = 24, // 9a鸟扔状态
        StIntroThinkForABit = 25, // 9a Intro

        // Extended States
        StHanging = 26,
        StElectricShocking,

        MaxStates,
    }

    public static partial class PlayerStates
    {
        public const float ROPE_MAX_PULL_NOT_ON_GROUND = 80.0f;
        public const float ROPE_MAX_PULL_ON_GROUND = 235.0f;

        public static GrapplingHook MadelinesHook => _madelinesHook;

        public static void Initialize()
        {
            IL.Celeste.Player.ctor += Player_ILConstruct;
            IL.Celeste.Player.NormalUpdate += Player_ILNormalUpdate;
            IL.Celeste.Player.OnCollideV += Player_ILOnCollideV;
            IL.Celeste.Player.ClimbCheck += Player_ILClimbCheck;
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.SceneBegin += Player_SceneBegin;
            On.Celeste.Player.SceneEnd += Player_SceneEnd;
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
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
            On.Celeste.Player.WindMove += Player_WindMove;
            On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor -= Player_ILConstruct;
            IL.Celeste.Player.NormalUpdate -= Player_ILNormalUpdate;
            IL.Celeste.Player.OnCollideV -= Player_ILOnCollideV;
            IL.Celeste.Player.ClimbCheck -= Player_ILClimbCheck;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.SceneBegin -= Player_SceneBegin;
            On.Celeste.Player.SceneEnd -= Player_SceneEnd;
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
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
            On.Celeste.Player.WindMove -= Player_WindMove;
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
                cursor.EmitDelegate(CalculateNormalMoveCoefficient);
                cursor.EmitMul();
                cursor.EmitStloc(6);
            }
        }

        private static float CalculateNormalMoveCoefficient()
        {
            if (_madelinesHook.Active && (_madelinesHook.State == GrapplingHook.HookStates.Emitting || _madelinesHook.State == GrapplingHook.HookStates.Bouncing))
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
            self.SetTimeTicker("climb_jump_ticker", 1.0f);
            self.SetTimeTicker("hook_break_ticker", 0.15f);
            self.SetTimeTicker("elec_shock_ticker", 1.0f);
            DynamicData.For(self).Set("rope_is_loosen", true);
            DynamicData.For(self).Set("is_booster_dash", false);
            self.SetSlideState(SlideStates.None);
        }

        private static void Player_SceneBegin(On.Celeste.Player.orig_SceneBegin orig, Player self, Scene scene)
        {
            orig(self, scene);
            var levelState = (self.Scene as Level).GetState();
            _madelinesHook = new GrapplingHook(GrapplingHook.HOOK_SIZE, levelState.HookSettings.RopeLength, levelState.RopeMaterial);
            _madelinesHook.Mode = levelState.GameplayMode;
        }

        private static void Player_SceneEnd(On.Celeste.Player.orig_SceneEnd orig, Player self, Scene scene)
        {
            orig(self, scene);
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            var levelState = (self.Scene as Level).GetState();
            if (levelState != null)
            {
                _madelinesHook = new GrapplingHook(GrapplingHook.HOOK_SIZE, levelState.HookSettings.RopeLength, levelState.RopeMaterial);
                _madelinesHook.Mode = levelState.GameplayMode;
            }
            _throwHookCheck = new ShotHookCheck(AquaModule.Settings.ThrowHook, AquaModule.Settings.ThrowHookMode);
            DynamicData.For(self).Set("previous_facing", (int)self.Facing);
        }

        private static void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene)
        {
            if (_madelinesHook.Active)
            {
                scene.Remove(_madelinesHook);
            }

            orig(self, scene);
        }

        private static void Player_DreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static void Player_RedDashBegin(On.Celeste.Player.orig_RedDashBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static void Player_BoostBegin(On.Celeste.Player.orig_BoostBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static void Player_FlingBirdBegin(On.Celeste.Player.orig_FlingBirdBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static void Player_CassetteFlyBegin(On.Celeste.Player.orig_CassetteFlyBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static System.Collections.IEnumerator Player_PickupCoroutine(On.Celeste.Player.orig_PickupCoroutine orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            return orig(self);
        }

        private static void Player_StarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
            }

            orig(self);
        }

        private static void Player_SummitLaunchBegin(On.Celeste.Player.orig_SummitLaunchBegin orig, Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State != GrapplingHook.HookStates.Revoking)
            {
                _madelinesHook.Revoke();
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
            _madelinesHook.SetRopeLengthLocked(true, self.Center);
            DynamicData.For(self).Set("climb_rope_direction", 0);
        }

        private static void Player_HangingEnd(this Player self)
        {
            _madelinesHook.SetRopeLengthLocked(false, self.Center);
            DynamicData.For(self).Set("climb_rope_direction", 0);
        }

        private static int Player_HangingUpdate(this Player self)
        {
            float dt = Engine.DeltaTime;
            Vector2 ropeDirection = _madelinesHook.RopeDirection;
            bool swingUp = AquaMaths.Cross(Vector2.UnitX, ropeDirection) >= 0.0f;
            if (_madelinesHook.State != GrapplingHook.HookStates.Fixed)
            {
                return (int)AquaStates.StNormal;
            }
            else if (self.CanDash)
            {
                return self.StartDash();
            }
            else if (self.IsExhausted())
            {
                _madelinesHook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (_throwHookCheck.CanFlyTowards && _madelinesHook.CanFlyToward())
            {
                FlyTowardHook(self);
            }
            else if (_throwHookCheck.CanRevoke)
            {
                _madelinesHook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (!Input.GrabCheck && !(self.level.GetState().AutoGrabHookRope))
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

            DynamicData.For(self).Set("climb_rope_direction", 0);
            if (self.onGround)
            {
                _madelinesHook.AlongRopeSpeed = 0.0f;
                return (int)AquaStates.StNormal;
            }
            else
            {
                Vector2 swingDirection = _madelinesHook.SwingDirection;
                self.HandleHangingSpeed(dt);
                float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                _madelinesHook.AlongRopeSpeed = speedAlongRope;
                TimeTicker breakTicker = self.GetTimeTicker("hook_break_ticker");
                if (speedAlongRope >= 0.0f)
                {
                    _madelinesHook.SetRopeLengthLocked(true, self.Center);
                    DynamicData.For(self).Set("rope_is_loosen", false);
                    if (Input.Jump.Pressed)
                    {
                        self.SwingJump(dt);
                        return (int)AquaStates.StNormal;
                    }
                    if (speedAlongRope > ROPE_MAX_PULL_NOT_ON_GROUND)
                    {
                        breakTicker.Tick(dt);
                        if (breakTicker.Check())
                        {
                            _madelinesHook.Revoke();
                            return (int)AquaStates.StNormal;
                        }
                    }
                    else
                    {
                        breakTicker.Reset();
                    }
                    self.Speed = self.TurnToTangentSpeed(self.Speed, swingDirection);
                }
                else
                {
                    _madelinesHook.SetRopeLengthLocked(false, self.Center);
                    breakTicker.Reset();
                    if (!swingUp)
                    {
                        return (int)AquaStates.StNormal;
                    }
                }
                if (swingUp)
                {
                    DynamicData.For(self).Set("climb_rope_direction", MathF.Sign(Input.MoveY.Value));
                }
                var levelState = (self.Scene as Level).GetState();
                if (Input.MoveY.Value != 0 && swingUp && speedAlongRope >= 0.0f)
                {
                    float rollingSpeed = Input.MoveY.Value > 0 ? levelState.HookSettings.RollingSpeedDown : -levelState.HookSettings.RollingSpeedUp;
                    _madelinesHook.AddLockedRopeLength(rollingSpeed * dt);
                }
                if (Input.MoveX.Value != 0)
                {
                    float sign = swingUp ? 1.0f : -1.0f;
                    Vector2 strengthSpeed = levelState.HookSettings.SwingStrength * swingDirection * Input.MoveX.Value * dt * sign;
                    strengthSpeed *= Calc.Clamp(_madelinesHook.SwingRadius / _madelinesHook.MaxLength, 0.1f, 1.0f);
                    self.Speed += strengthSpeed;
                }
            }

            return (int)AquaStates.StHanging;
        }

        private static void Player_ElectricShockingBegin(this Player self)
        {
            self.Speed = Vector2.Zero;
            TimeTicker ticker = self.GetTimeTicker("elec_shock_ticker");
            ticker.Reset();
            self.Sprite.Play("electricshock");
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
                TimeTicker climbJumpTicker = self.GetTimeTicker("climb_jump_ticker");
                climbJumpTicker.Tick(dt);
                DynamicData.For(self).Set("rope_is_loosen", true);
                DynamicData.For(self).Set("previous_facing", (int)self.Facing);
                if (!PERMIT_SHOOT_STATES.Contains(self.StateMachine.State))
                {
                    DynamicData.For(self).Set("start_emitting", false);
                }
                if (!self.CheckOnSlidable())
                    self.SetSlideState(SlideStates.None);

                orig(self);

                if (_madelinesHook.Active && _madelinesHook.Revoked)
                {
                    self.Scene.Remove(_madelinesHook);
                    _throwHookCheck.EndPeriod();
                }
                else if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
                {
                    if (_madelinesHook.EnforcePlayer(self, new Segment(_madelinesHook.PlayerPreviousPosition, self.Center), Engine.DeltaTime))
                    {
                        _madelinesHook.Revoke();
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
            _throwHookCheck.Update();
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);
            TimeTicker climbJumpTicker = self.GetTimeTicker("climb_jump_ticker");
            climbJumpTicker.Reset();
        }

        private static void Player_WindMove(On.Celeste.Player.orig_WindMove orig, Player self, Vector2 move)
        {
            if (!DynamicData.For(self).Get<bool>("rope_is_loosen"))
                return;

            orig(self, move);
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
                                    self.Sprite.Play("icestumble");
                                }
                            }
                            break;
                        case SlideStates.HighSpeed:
                            if (self.Sprite.CurrentAnimationID != "fliponice")
                            {
                                self.Sprite.Play("iceslide");
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
                    self.Sprite.Play("hookflip", true);
                }
                else if (self.Sprite.CurrentAnimationID != "hookflip")
                {
                    if (climbRopeDirection < 0)
                    {
                        self.Sprite.Play("hookup");
                    }
                    else if (climbRopeDirection > 0)
                    {
                        self.Sprite.Play("hookdown");
                    }
                    else
                    {
                        self.Sprite.Play("hookidle");
                    }
                }
            }
        }

        private static void SwingJump(this Player self, float dt)
        {
            _madelinesHook.Revoke();
            var levelState = self.SceneAs<Level>().GetState();
            self.LiftSpeed = self.Speed * new Vector2(levelState.HookSettings.SwingJumpXPercent / 100.0f, levelState.HookSettings.SwingJumpYPercent / 100.0f);
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
            Vector2 ropeDirection = _madelinesHook.RopeDirection;
            var levelState = self.level.GetState();
            float origAlongSpeed = MathF.Max(Vector2.Dot(self.Speed, -ropeDirection), 0.0f);
            self.Speed = -ropeDirection * MathF.Min(self.SceneAs<Level>().GetState().HookSettings.FlyTowardSpeed + origAlongSpeed, levelState.HookSettings.MaxLineSpeed);
        }

        private static int PreHookUpdate(Player self)
        {
            if (!AquaModule.Settings.FeatureEnabled && !self.level.GetState().FeatureEnabled)
                return -1;

            float dt = Engine.DeltaTime;
            if (DynamicData.For(self).Get<bool>("start_emitting"))
            {
                TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
                emittingTicker.Tick(Engine.RawDeltaTime);
                if (emittingTicker.CheckRate(0.5f))
                {
                    if (!_madelinesHook.Active)
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
                        if (Input.Aim.Value != Vector2.Zero)
                        {
                            direction = Input.GetAimVector(self.Facing);
                        }
                        float emitSpeed = self.SceneAs<Level>().GetState().HookSettings.EmitSpeed;
                        float emitSpeedCoeff = self.CalculateEmitParameters(emitSpeed * direction, ref direction);
                        if (self.CanEmitHook(direction) && _madelinesHook.CanEmit())
                        {
                            _madelinesHook.Emit(direction, emitSpeed, emitSpeedCoeff - 1.0f);
                            self.Scene.Add(_madelinesHook);
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
            if (!_madelinesHook.Active && _throwHookCheck.CanThrow && !self.IsExhausted() && self.Holding == null)
            {
                Engine.TimeRateB = 0.1f;
                DynamicData.For(self).Set("start_emitting", true);
                TimeTicker emittingTicker = self.GetTimeTicker("emit_ticker");
                emittingTicker.Reset();
                return -1;
            }

            if (_madelinesHook.Active)
            {
                if (_madelinesHook.State == GrapplingHook.HookStates.Fixed && self.IsExhausted())
                {
                    _madelinesHook.Revoke();
                }
                else if (_madelinesHook.JustFixed && _madelinesHook.CanFlyToward())
                {
                    FlyTowardHook(self);
                }
                else if (_throwHookCheck.CanRevoke || _throwHookCheck.CanFlyTowards)
                {
                    if (_madelinesHook.State == GrapplingHook.HookStates.Emitting)
                    {
                        _madelinesHook.RecordEmitElapsed();
                    }
                    else if (_madelinesHook.State == GrapplingHook.HookStates.Fixed)
                    {
                        if (_throwHookCheck.CanFlyTowards && _madelinesHook.CanFlyToward())
                        {
                            FlyTowardHook(self);
                        }
                        else if (_throwHookCheck.CanRevoke)
                        {
                            _madelinesHook.Revoke();
                        }
                    }
                }
                else if (_madelinesHook.State == GrapplingHook.HookStates.Fixed)
                {
                    if (!self.onGround && _madelinesHook.ReachLockedLength(self.Center))
                    {
                        Vector2 ropeDirection = _madelinesHook.RopeDirection;
                        Vector2 swingDirection = _madelinesHook.SwingDirection;
                        self.HandleHangingSpeed(dt);
                        float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                        _madelinesHook.AlongRopeSpeed = speedAlongRope;
                        TimeTicker breakTicker = DynamicData.For(self).Get<TimeTicker>("hook_break_ticker");
                        if (speedAlongRope >= 0.0f)
                        {
                            DynamicData.For(self).Set("rope_is_loosen", false);
                            if (speedAlongRope > ROPE_MAX_PULL_NOT_ON_GROUND)
                            {
                                breakTicker.Tick(dt);
                                if (breakTicker.Check())
                                {
                                    _madelinesHook.Revoke();
                                    return (int)AquaStates.StNormal;
                                }
                            }
                            else
                            {
                                breakTicker.Reset();
                            }
                            self.Speed = self.TurnToTangentSpeed(self.Speed, swingDirection);
                        }
                        else
                        {
                            breakTicker.Reset();
                        }
                    }
                    else if (_madelinesHook.ReachLockedLength(self.Center))
                    {
                        Vector2 ropeDirection = _madelinesHook.RopeDirection;
                        float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                        _madelinesHook.AlongRopeSpeed = speedAlongRope;
                        TimeTicker breakTicker = DynamicData.For(self).Get<TimeTicker>("hook_break_ticker");
                        if (speedAlongRope > ROPE_MAX_PULL_ON_GROUND)
                        {
                            breakTicker.Tick(dt);
                            if (breakTicker.Check())
                            {
                                _madelinesHook.Revoke();
                                return (int)AquaStates.StNormal;
                            }
                        }
                        else
                        {
                            breakTicker.Reset();
                        }
                    }
                    else
                    {
                        _madelinesHook.AlongRopeSpeed = 0.0f;
                    }
                    DynamicData.For(self).Set("fixing_speed", self.Speed);
                }
                //else if (_madelinesHook.State == GrapplingHook.HookStates.Bouncing && Input.Jump.Pressed)
                //{
                //    float checkDot = Vector2.Dot(-_madelinesHook.RopeDirection, _madelinesHook.BouncingVelocity);
                //    if (checkDot > 0.0f)
                //    {
                //        if (self.onGround)
                //        {
                //            self.LiftSpeed += -_madelinesHook.RopeDirection * _madelinesHook.BouncingVelocity.Length() * dt * AquaModule.Settings.HookSettings.HookBounceJumpCoefficient;
                //            self.Jump(true);
                //        }
                //        else
                //        {
                //            self.Speed += -_madelinesHook.RopeDirection * _madelinesHook.BouncingVelocity.Length() * dt * AquaModule.Settings.HookSettings.HookBounceMoveCoefficient;
                //        }
                //        _madelinesHook.Revoke();
                //    }
                //}
            }
            return -1;
        }

        private static int PostHookUpdate(Player self)
        {
            if (!AquaModule.Settings.FeatureEnabled && !self.level.GetState().FeatureEnabled)
                return -1;

            float dt = Engine.DeltaTime;
            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
            {
                TimeTicker climbJumpTicker = self.GetTimeTicker("climb_jump_ticker");
                TimeTicker dashHangingTicker = self.GetTimeTicker("dash_hanging_ticker");
                if (self.StateMachine.State == (int)AquaStates.StDash)
                {
                    dashHangingTicker.Tick(dt);
                }
                if (!self.onGround && (self.StateMachine.State != (int)AquaStates.StDash || dashHangingTicker.Check()) && climbJumpTicker.Check() && (Input.GrabCheck || self.level.GetState().AutoGrabHookRope))
                {
                    Vector2 ropeDirection = _madelinesHook.RopeDirection;
                    bool swingUp = AquaMaths.Cross(Vector2.UnitX, ropeDirection) >= 0.0f;
                    if (swingUp)
                    {
                        return (int)AquaStates.StHanging;
                    }
                    float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                    if (speedAlongRope >= 0.0f)
                    {
                        return (int)AquaStates.StHanging;
                    }
                }
                if (!self.onGround && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
                {
                    // NormalUpdate限制太多，这种情况下强制改为之前计算的速度
                    if (_madelinesHook.ReachLockedLength(self.Center))
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
            self.Speed.Y += Player.Gravity * dt;
            self.Speed -= _madelinesHook.Acceleration * dt * 0.8f;
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

        private static GrapplingHook _madelinesHook;
        private static ShotHookCheck _throwHookCheck;
    }
}
