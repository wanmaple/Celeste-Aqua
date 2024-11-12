using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using MonoMod.Utils;
using Celeste.Mod.Aqua.Debug;

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

        MaxStates,
    }

    public static class PlayerStates
    {
        public static void Initialize()
        {
            IL.Celeste.Player.ctor += Player_ILConstruct;
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.NormalUpdate += Player_NormalUpdate;
            On.Celeste.Player.DashUpdate += Player_DashUpdate;
            On.Celeste.Player.LaunchUpdate += Player_LaunchUpdate;
            On.Celeste.Player.BoostBegin += Player_BoostBegin;
            On.Celeste.Player.RedDashBegin += Player_RedDashBegin;
            On.Celeste.Player.DreamDashBegin += Player_DreamDashBegin;
            On.Celeste.Player.SummitLaunchBegin += Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin += Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin += Player_FlingBirdBegin;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
            On.Celeste.Player.WindMove += Player_WindMove;
            On.Celeste.Player.UpdateSprite += Player_UpdateSprite;
#if DEBUG
            On.Celeste.Player.Render += Player_Render;
#endif
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor -= Player_ILConstruct;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.LaunchUpdate -= Player_LaunchUpdate;
            On.Celeste.Player.BoostBegin -= Player_BoostBegin;
            On.Celeste.Player.RedDashBegin -= Player_RedDashBegin;
            On.Celeste.Player.DreamDashBegin -= Player_DreamDashBegin;
            On.Celeste.Player.SummitLaunchBegin -= Player_SummitLaunchBegin;
            On.Celeste.Player.StarFlyBegin -= Player_StarFlyBegin;
            On.Celeste.Player.FlingBirdBegin -= Player_FlingBirdBegin;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
            On.Celeste.Player.WindMove -= Player_WindMove;
            On.Celeste.Player.UpdateSprite -= Player_UpdateSprite;
#if DEBUG
            On.Celeste.Player.Render -= Player_Render;
#endif
        }

        public static Vector2 ExactCenter(this Player self)
        {
            return self.ExactPosition + self.Center - self.Position;
        }

        private static void Player_ILConstruct(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdcI4(26)))
            {
                cursor.Index++;
                cursor.EmitLdcI4((int)AquaStates.MaxStates - (int)AquaStates.StHanging);
                cursor.EmitAdd();
            }
        }

        private static void Player_Construct(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            self.StateMachine.SetCallbacks((int)AquaStates.StHanging, self.Player_HangingUpdate, null, self.Player_HangingBegin, self.Player_HangingEnd);
            self.StateMachine.SetStateName((int)AquaStates.StHanging, "Hanging");
            var climbJumpTicker = new TimeTicker(1.0f);
            DynamicData.For(self).Set("climb_jump_ticker", climbJumpTicker);
            DynamicData.For(self).Set("rope_is_loosen", true);
#if DEBUG
            DynamicData.For(self).Set("tangent_speed", 0.0f);
            DynamicData.For(self).Set("along_speed", 0.0f);
#endif
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            _madelinesHook = new GrapplingHook(AquaModule.Settings.HookSettings.HookSize, AquaModule.Settings.HookSettings.HookLength);
        }

        private static void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene)
        {
            if (_madelinesHook.Active)
            {
                scene.Remove(_madelinesHook);
            }
            _madelinesHook = null;
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
                _madelinesHook.Revoke();
                return self.StartDash();
            }
            else if (self.IsExhausted())
            {
                _madelinesHook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (AquaModule.Settings.ThrowHook.Pressed)
            {
                if (_madelinesHook.CanFlyToward())
                {
                    FlyTowardHook(self);
                }
                else
                {
                    _madelinesHook.Revoke();
                    return (int)AquaStates.StNormal;
                }
            }
            else if (!Input.GrabCheck)
            {
                return (int)AquaStates.StNormal;
            }
            else if (Input.GrabCheck && self.ClimbCheck((int)self.Facing))
            {
                return (int)AquaStates.StClimb;
            }
            else if (Input.Jump.Pressed)
            {
                _madelinesHook.Revoke();
                self.LiftSpeed = self.Speed * new Vector2(AquaModule.Settings.HookSettings.HookJumpXPercent / 100.0f, AquaModule.Settings.HookSettings.HookJumpYPercent / 100.0f);
                self.Jump(false);
                float staminaCost = AquaModule.Settings.HookSettings.HookJumpStaminaCost * dt;
                self.Stamina = MathF.Max(0.0f, self.Stamina - staminaCost);
                return (int)AquaStates.StNormal;
            }

            Vector2 alongRopeSpeed = Vector2.Zero;
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
#if DEBUG
                DynamicData.For(self).Set("tangent_speed", TurnToTangentSpeed(self.Speed, swingDirection).Length());
                DynamicData.For(self).Set("along_speed", Vector2.Dot(self.Speed, ropeDirection));
#endif
                float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                _madelinesHook.AlongRopeSpeed = speedAlongRope;
                if (speedAlongRope >= 0.0f)
                {
                    DynamicData.For(self).Set("rope_is_loosen", false);
                    if (speedAlongRope > AquaModule.Settings.HookSettings.HookBreakSpeed)
                    {
                        _madelinesHook.Revoke();
                        return (int)AquaStates.StNormal;
                    }
                    self.Speed = TurnToTangentSpeed(self.Speed, swingDirection);
                }
                if (Input.MoveY.Value != 0 && swingUp && speedAlongRope >= 0.0f)
                {
                    float rollingSpeed = Input.MoveY > 0 ? AquaModule.Settings.HookSettings.HookRollingSpeedDown : -AquaModule.Settings.HookSettings.HookRollingSpeedUp;
                    _madelinesHook.SetRopeLengthLocked(false, self.Center);
                    if (Input.MoveY < 0 && _madelinesHook.SwingRadius <= 4.0f)
                    {
                        rollingSpeed = 0.0f;
                    }
                    DynamicData.For(self).Set("climb_rope_direction", MathF.Sign(rollingSpeed));
                    alongRopeSpeed = rollingSpeed * ropeDirection;
                }
                else
                {
                    _madelinesHook.SetRopeLengthLocked(true, self.Center);
                }
                if (Input.MoveX.Value != 0)
                {
                    float sign = swingUp ? 1.0f : -1.0f;
                    self.Speed += AquaModule.Settings.HookSettings.HookSwingStrength * swingDirection * Input.MoveX.Value * dt * sign;
                }
            }
            self.Speed += alongRopeSpeed;

            return (int)AquaStates.StHanging;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            float dt = Engine.DeltaTime;
            TimeTicker climbJumpTicker = DynamicData.For(self).Get<TimeTicker>("climb_jump_ticker");
            climbJumpTicker.Tick(dt);
            DynamicData.For(self).Set("rope_is_loosen", true);

            orig(self);

            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
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
                        staminaCost = AquaModule.Settings.HookSettings.HookClimbUpStaminaCost * dt;
                    }
                    else if (climbRopeDirection == 0)
                    {
                        staminaCost = AquaModule.Settings.HookSettings.HookGrabingStaminaCost * dt;
                    }
                    self.Stamina = MathF.Max(0.0f, self.Stamina - staminaCost);
                }
            }
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);
            TimeTicker climbJumpTicker = DynamicData.For(self).Get<TimeTicker>("climb_jump_ticker");
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
            orig(self);

            if (self.StateMachine.State == (int)AquaStates.StHanging)
            {
                int climbRopeDirection = DynamicData.For(self).Get<int>("climb_rope_direction");
                if (climbRopeDirection < 0)
                {
                    self.Sprite.Play("climbUp");
                }
                else if (climbRopeDirection > 0)
                {
                    self.Sprite.Play("wallslide");
                }
                else
                {
                    self.Sprite.Play("dangling");
                }
            }
        }

        private static Vector2 TurnToTangentSpeed(Vector2 speed, Vector2 swingDirection)
        {
            return Vector2.Dot(speed, swingDirection) * swingDirection;
        }

        private static void FlyTowardHook(Player player)
        {
            Vector2 ropeDirection = _madelinesHook.RopeDirection;
            player.Speed = -ropeDirection * AquaModule.Settings.HookSettings.HookFlyTowardSpeed;
        }

        private static int PreHookUpdate(Player self)
        {
            float dt = Engine.DeltaTime;
            if (!_madelinesHook.Active && AquaModule.Settings.ThrowHook.Pressed && !self.IsExhausted() && self.Holding == null)
            {
                Vector2 direction = new Vector2(0.0f, -1.0f);
                if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    direction.X = Input.MoveX;
                    direction.Y = Input.MoveY;
                    direction.Normalize();
                }
                float emitSpeed = AquaModule.Settings.HookSettings.HookEmitSpeed;
                self.CalculateEmitParameters(ref direction, ref emitSpeed);
                _madelinesHook.Emit(direction, emitSpeed);
                self.Scene.Add(_madelinesHook);
                return -1;
            }
            else if (_madelinesHook.Active && _madelinesHook.Revoked)
            {
                self.Scene.Remove(_madelinesHook);
                return -1;
            }

            if (_madelinesHook.Active)
            {
                if (_madelinesHook.State == GrapplingHook.HookStates.Fixed && self.IsExhausted())
                {
                    _madelinesHook.Revoke();
                }
                else if (self.Holding != null)
                {
                    _madelinesHook.Revoke();
                }
                else if (_madelinesHook.JustFixed && _madelinesHook.CanFlyToward())
                {
                    FlyTowardHook(self);
                }
                else if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    if (_madelinesHook.State == GrapplingHook.HookStates.Emitting)
                    {
                        _madelinesHook.RecordEmitElapsed();
                    }
                    else if (_madelinesHook.State == GrapplingHook.HookStates.Fixed)
                    {
                        if (_madelinesHook.CanFlyToward())
                        {
                            FlyTowardHook(self);
                        }
                        else
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
#if DEBUG
                        DynamicData.For(self).Set("tangent_speed", TurnToTangentSpeed(self.Speed, swingDirection).Length());
                        DynamicData.For(self).Set("along_speed", Vector2.Dot(self.Speed, ropeDirection));
#endif
                        float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                        _madelinesHook.AlongRopeSpeed = speedAlongRope;
                        if (speedAlongRope >= 0.0f)
                        {
                            DynamicData.For(self).Set("rope_is_loosen", false);
                            if (speedAlongRope > AquaModule.Settings.HookSettings.HookBreakSpeed)
                            {
                                _madelinesHook.Revoke();
                                return -1;
                            }
                            self.Speed = TurnToTangentSpeed(self.Speed, swingDirection);
                        }
                    }
                    else
                    {
                        _madelinesHook.AlongRopeSpeed = 0.0f;
                    }
                    DynamicData.For(self).Set("fixing_speed", self.Speed);
                }
            }
            return -1;
        }

        private static int PostHookUpdate(Player self)
        {
            float dt = Engine.DeltaTime;
            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
            {
                TimeTicker climbJumpTicker = DynamicData.For(self).Get<TimeTicker>("climb_jump_ticker");
                if (!self.onGround && !climbJumpTicker.Check() && Input.GrabCheck)
                {
                    return (int)AquaStates.StHanging;
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

        private static void HandleHangingSpeed(this Player self, float dt)
        {
            self.Speed.Y += Player.Gravity * dt;
            self.Speed -= _madelinesHook.Velocity * dt * AquaModule.Settings.HookSettings.HookInertiaCoefficient;
            self.Speed += self.CalculateWindSpeed() * dt * AquaModule.Settings.HookSettings.HookWindCoefficient;
        }

        private static Vector2 CalculateWindSpeed(this Player self)
        {
            WindController windCtrl = self.Scene.Entities.FindFirst<WindController>();
            if (windCtrl == null)
                return Vector2.Zero;

            return windCtrl.targetSpeed;
        }

        private static void CalculateEmitParameters(this Player self, ref Vector2 direction, ref float speed)
        {
            WindController windCtrl = self.Scene.Entities.FindFirst<WindController>();
            if (windCtrl == null)
                return;

            Vector2 windSpeed = windCtrl.targetSpeed;
            float maxSpeedUp = 0.25f;
            direction.X *= (1.0f + windSpeed.X / 800.0f * maxSpeedUp * MathF.Sign(direction.X));
            direction.Y *= (1.0f + windSpeed.Y / 800.0f * maxSpeedUp * MathF.Sign(direction.Y));
            speed *= direction.Length();
            direction.Normalize();
        }

        private static bool IsExhausted(this Player self)
        {
            return self.CheckStamina <= 0.0f;
        }

#if DEBUG
        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            orig(self);

            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed && !self.onGround)
            {
                int tangentSpd = Math.Abs((int)MathF.Floor(DynamicData.For(self).Get<float>("tangent_speed")));
                int alongSpd = Math.Abs((int)MathF.Floor(DynamicData.For(self).Get<float>("along_speed")));
                float scale = 1.0f;
                Draw.TextCentered(Draw.DefaultFont, tangentSpd.ToString(), self.Center + new Vector2(0.0f, 8.0f), Color.White, scale);
                Draw.TextCentered(Draw.DefaultFont, alongSpd.ToString(), self.Position + new Vector2(0.0f, 16.0f), Color.White, scale);
            }
        }
#endif

        private static GrapplingHook _madelinesHook;
#if DEBUG
        private static ConditionTrigger _cond = new ConditionTrigger();
#endif
    }
}
