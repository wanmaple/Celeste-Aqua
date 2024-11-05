using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

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
        private const float GRAPPLING_HOOK_SIZE = 8;
        private const float GRAPPLING_HOOK_LENGTH = 70.0f;
        private const float GRAPPLING_HOOK_BREAK_SPEED = 235.0f;
        private const float GRAPPLING_HOOK_EMIT_SPEED = 400.0f;

        public static void Initialize()
        {
            IL.Celeste.Player.ctor += Player_ILConstruct;
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.NormalUpdate += Player_NormalUpdate;
            On.Celeste.Player.DashUpdate += Player_DashUpdate;
            On.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Player.ctor-=Player_ILConstruct;
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            On.Celeste.Player.DashUpdate -= Player_DashUpdate;
            On.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
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
            _madelinesHook = new GrapplingHook(GRAPPLING_HOOK_SIZE, GRAPPLING_HOOK_LENGTH, GRAPPLING_HOOK_BREAK_SPEED);
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
            int nextState = PreNormalUpdate(self);
            if (nextState < 0)
            {
                nextState = orig(self);
            }
            return nextState;
        }

        private static int Player_ClimbUpdate(On.Celeste.Player.orig_ClimbUpdate orig, Player self)
        {
            return orig(self);
        }

        private static int Player_DashUpdate(On.Celeste.Player.orig_DashUpdate orig, Player self)
        {
            return orig(self);
        }

        private static void Player_HangingBegin(this Player self)
        {
            self.Speed = Vector2.Zero;
        }

        private static void Player_HangingEnd(this Player self)
        {

        }

        private static int Player_HangingUpdate(this Player self)
        {
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
            return (int)AquaStates.StHanging;
        }

        private static int PreNormalUpdate(Player self)
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
                _madelinesHook.Emit(direction, GRAPPLING_HOOK_EMIT_SPEED);
                self.Scene.Add(_madelinesHook);
                return 0;
            }
            else if (_madelinesHook.Active && _madelinesHook.Revoked)
            {
                self.Scene.Remove(_madelinesHook);
                return 0;
            }
            else if (_madelinesHook.Active && _madelinesHook.State == GrapplingHook.HookStates.Fixed)
            {
                if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    _madelinesHook.Revoke();
                }
                else if (_madelinesHook.JustFixed && !self.onGround)
                {
                    return (int)AquaStates.StHanging;
                }
            }
            return -1;
        }

        private static GrapplingHook _madelinesHook;
    }
}
