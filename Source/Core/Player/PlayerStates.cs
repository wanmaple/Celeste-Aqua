using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlayerStates
    {
        private const float GRAPPLING_HOOK_SIZE = 8;
        private const float GRAPPLING_HOOK_LENGTH = 100.0f;
        private const float EMITTING_SPEED = 500.0f;

        public static void Initialize()
        {
            On.Celeste.Player.ctor += Player_Construct;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Removed += Player_Removed;
            On.Celeste.Player.NormalUpdate += Player_NormalUpdate;
        }

        public static void Uninitialize()
        {
            On.Celeste.Player.ctor -= Player_Construct;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.Removed -= Player_Removed;
            On.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
        }

        private static void Player_Construct(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
        }

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);
            _madelineHook = new GrapplingHook(GRAPPLING_HOOK_SIZE, GRAPPLING_HOOK_LENGTH);
        }

        private static void Player_Removed(On.Celeste.Player.orig_Removed orig, Player self, Scene scene)
        {
            if (_madelineHook.Active)
            {
                scene.Remove(_madelineHook);
            }
            _madelineHook = null;
            orig(self, scene);
        }

        private static int Player_NormalUpdate(On.Celeste.Player.orig_NormalUpdate orig, Player self)
        {
            int nextState = HandleMadelinesHook(self);
            if (nextState == 0)
            {
                return orig(self);
            }
            return nextState;
        }

        private static int HandleMadelinesHook(Player self)
        {
            if (!_madelineHook.Active && AquaModule.Settings.ThrowHook.Pressed)
            {
                Vector2 direction = new Vector2(0.0f, -1.0f);
                if (Input.MoveX.Value != 0 || Input.MoveY.Value != 0)
                {
                    direction.X = Input.MoveX;
                    direction.Y = Input.MoveY;
                    direction.Normalize();
                }
                _madelineHook.Emit(direction, EMITTING_SPEED);
                self.Scene.Add(_madelineHook);
                return 0;
            }
            else if (_madelineHook.Active && _madelineHook.Revoked)
            {
                self.Scene.Remove(_madelineHook);
                return 0;
            }
            else if (_madelineHook.Active && _madelineHook.State == GrapplingHook.HookStates.Fixed)
            {
                if (AquaModule.Settings.ThrowHook.Pressed)
                {
                    _madelineHook.Revoke();
                }
            }
            return 0;
        }

        private static GrapplingHook _madelineHook;
    }
}
