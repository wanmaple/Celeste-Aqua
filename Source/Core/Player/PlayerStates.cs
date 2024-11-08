using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using static Celeste.TrackSpinner;
using System;

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
            On.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
            On.Celeste.Player.Update += Player_Update;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor -= Player_ILConstruct;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
            On.Celeste.Player.Update -= Player_Update;
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

        private static int Player_ClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
        {
            return orig(self);
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
            self.Speed = TurnToTangentSpeed(self.Speed, _madelinesHook.SwingDirection);
        }

        private static void Player_HangingEnd(this Player self)
        {
            _madelinesHook.SetRopeLengthLocked(false, self.Center);
        }

        private static int Player_HangingUpdate(this Player self)
        {
            float dt = Engine.DeltaTime;
            if (self.CanDash)
            {
                _madelinesHook.Revoke();
                return self.StartDash();
            }
            else if (AquaModule.Settings.ThrowHook.Pressed)
            {
                _madelinesHook.Revoke();
                return (int)AquaStates.StNormal;
            }
            else if (!Input.Grab.Check)
            {
                return (int)AquaStates.StNormal;
            }
            else if (Input.Jump.Pressed)
            {
                _madelinesHook.Revoke();
                self.LiftSpeed = self.Speed * new Vector2(AquaModule.Settings.HookSettings.HookJumpXPercent / 100.0f, AquaModule.Settings.HookSettings.HookJumpYPercent / 100.0f);
                self.Jump(false);
                return (int)AquaStates.StNormal;
            }

            UpdateGravity(self, dt);
            Vector2 ropeDirection = _madelinesHook.RopeDirection;
            Vector2 alongRopeSpeed = Vector2.Zero;
            if (Input.MoveY.Value != 0)
            {
                float rollingSpeed = Input.MoveY > 0 ? AquaModule.Settings.HookSettings.HookRollingSpeedDown : -AquaModule.Settings.HookSettings.HookRollingSpeedUp * 0.25f;
                float rollingDistance = rollingSpeed * dt;
                _madelinesHook.AddLockedLength(rollingDistance);
                alongRopeSpeed = rollingSpeed * ropeDirection;
            }
            Vector2 swingDirection = _madelinesHook.SwingDirection;
            if (Input.MoveX.Value != 0)
            {
                float sign = AquaMaths.Cross(Vector2.UnitX, ropeDirection) >= 0.0f ? 1.0f : -1.0f;
                self.Speed += AquaModule.Settings.HookSettings.HookSwingSpeed * swingDirection * Input.MoveX.Value * dt * sign;
            }
            self.Speed = TurnToTangentSpeed(self.Speed, swingDirection);
            self.Speed += alongRopeSpeed;

            return (int)AquaStates.StHanging;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
            {
                if (_madelinesHook.EnforcePlayer(self, new Segment(self.PreviousPosition, self.Position), Engine.DeltaTime))
                {
                    _madelinesHook.Revoke();
                    if (self.StateMachine.State == (int)AquaStates.StHanging)
                    {
                        self.StateMachine.ForceState((int)AquaStates.StNormal);
                    }
                }
            }
        }

        private static Vector2 TurnToTangentSpeed(Vector2 speed, Vector2 swingDirection)
        {
            return Vector2.Dot(speed, swingDirection) * swingDirection;
        }

        private static void UpdateGravity(Player self, float dt)
        {
            float target2 = self.maxFall;
            if (self.Holding != null && self.Holding.SlowFall)
            {
                self.holdCannotDuck = (float)Input.MoveY == 1f;
            }
            if ((self.moveX == (int)self.Facing || (self.moveX == 0 && Input.GrabCheck)) && Input.MoveY.Value != 1)
            {
                if (self.Speed.Y >= 0f && self.wallSlideTimer > 0f && self.Holding == null && self.ClimbBoundsCheck((int)self.Facing) && self.CollideCheck<Solid>(self.Position + Vector2.UnitX * (float)self.Facing) && !ClimbBlocker.EdgeCheck(self.level, self, (int)self.Facing) && self.CanUnDuck)
                {
                    self.Ducking = false;
                    self.wallSlideDir = (int)self.Facing;
                }

                if (self.wallSlideDir != 0)
                {
                    if (Input.GrabCheck)
                    {
                        self.ClimbTrigger(self.wallSlideDir);
                    }

                    if (self.wallSlideTimer > 0.6f && ClimbBlocker.Check(self.level, self, self.Position + Vector2.UnitX * self.wallSlideDir))
                    {
                        self.wallSlideTimer = 0.6f;
                    }

                    target2 = MathHelper.Lerp(160f, 20f, self.wallSlideTimer / 1.2f);
                    if (self.wallSlideTimer / 1.2f > 0.65f)
                    {
                        self.CreateWallSlideParticles(self.wallSlideDir);
                    }
                }
            }

            float num7 = ((Math.Abs(self.Speed.Y) < 40f && (Input.Jump.Check || self.AutoJump)) ? 0.5f : 1f);
            if (self.Holding != null && self.Holding.SlowFall && self.forceMoveXTimer <= 0f)
            {
                num7 *= 0.5f;
            }

            if (self.level.InSpace)
            {
                num7 *= 0.6f;
            }

            self.Speed.Y = Calc.Approach(self.Speed.Y, target2, (float)(Player.Gravity * (double)num7 * dt));
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
                if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    _madelinesHook.Revoke();
                }
            }
            return -1;
        }

        private static int PostHookUpdate(Player self)
        {
            if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
            {
                if (Input.Grab.Check)
                {
                    return (int)AquaStates.StHanging;
                }
            }
            return -1;
        }

        private static GrapplingHook _madelinesHook;
    }
}
