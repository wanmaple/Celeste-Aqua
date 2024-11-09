using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using static Celeste.TrackSpinner;
using System;
using MonoMod.Utils;
using System.IO.Pipes;

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
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Player.ClimbJump += Player_ClimbJump;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor -= Player_ILConstruct;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Player.ClimbJump -= Player_ClimbJump;
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
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            _madelinesHook = new GrapplingHook(AquaModule.Settings.HookSettings.HookSize, AquaModule.Settings.HookSettings.HookLength, AquaModule.Settings.HookSettings.HookBreakSpeed);
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

        private static void Player_HangingBegin(this Player self)
        {
            _madelinesHook.SetRopeLengthLocked(true, self.Center);
            //self.Speed = TurnToTangentSpeed(self.Speed, _madelinesHook.SwingDirection);
        }

        private static void Player_HangingEnd(this Player self)
        {
            _madelinesHook.SetRopeLengthLocked(false, self.Center);
        }

        private static int Player_HangingUpdate(this Player self)
        {
            float dt = Engine.DeltaTime;
            Vector2 ropeDirection = _madelinesHook.RopeDirection;
            bool swingUp = AquaMaths.Cross(Vector2.UnitX, ropeDirection) >= 0.0f;
            if (self.CanDash)
            {
                _madelinesHook.Revoke();
                return self.StartDash();
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
                return (int)AquaStates.StNormal;
            }

            Vector2 alongRopeSpeed = Vector2.Zero;
            if (self.onGround)
            {
                return (int)AquaStates.StNormal;
            }
            else
            {
                Vector2 swingDirection = _madelinesHook.SwingDirection;
                self.Speed.Y += Player.Gravity * dt;
                self.Speed.Y = MathF.Min(self.Speed.Y, self.maxFall);
                self.Speed -= _madelinesHook.Velocity;
                float speedAlongRope = Vector2.Dot(self.Speed, ropeDirection);
                if (speedAlongRope >= 0.0f)
                {
                    self.Speed = TurnToTangentSpeed(self.Speed, swingDirection);
                }
                if (Input.MoveY.Value != 0 && swingUp && speedAlongRope >= 0.0f)
                {
                    float rollingSpeed = Input.MoveY > 0 ? AquaModule.Settings.HookSettings.HookRollingSpeedDown : -AquaModule.Settings.HookSettings.HookRollingSpeedUp;
                    _madelinesHook.SetRopeLengthLocked(false, self.Center);
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
                if (self.StateMachine.State != (int)AquaStates.StClimb)
                {
                    self.Stamina = MathF.Min(self.Stamina + AquaModule.Settings.HookSettings.HookStaminaRecovery * dt, Player.ClimbMaxStamina);
                }
            }
        }

        private static void Player_ClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self)
        {
            orig(self);
            TimeTicker climbJumpTicker = DynamicData.For(self).Get<TimeTicker>("climb_jump_ticker");
            climbJumpTicker.Reset();
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
            if (!_madelinesHook.Active && AquaModule.Settings.ThrowHook.Pressed)
            {
                Vector2 direction = new Vector2(0.0f, -1.0f);
                if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    direction.X = Input.MoveX;
                    direction.Y = Input.MoveY;
                    direction.Normalize();
                }
                _madelinesHook.Emit(direction, AquaModule.Settings.HookSettings.HookEmitSpeed);
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
                if (_madelinesHook.JustFixed && _madelinesHook.CanFlyToward())
                {
                    FlyTowardHook(self);
                }
                if (AquaModule.Settings.ThrowHook.Pressed)
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

                //if (_madelinesHook.ReachLockedLength(self.Center))
                //{
                //    Vector2 targetPosition = _madelinesHook.Position + Vector2.UnitY * _madelinesHook.LockedRadius;
                //    Vector2 velocity = (targetPosition - self.Center) / dt;
                //    if (!AquaMaths.IsApproximateZero(velocity))
                //    {
                //        self.Speed = Vector2.Normalize(velocity) * MathF.Min(velocity.Length() * 1.5f, self.maxFall);
                //    }
                //    else
                //    {
                //        self.Speed = Vector2.Zero;
                //    }
                //    self.Speed -= _madelinesHook.Velocity;
                //}
            }
            return -1;
        }

        private static GrapplingHook _madelinesHook;
    }
}
