using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlayerExtensions
    {
        public static void InitializeGrapplingHook(this Player self, float size, float length, GrapplingHook.RopeMaterial material, GrapplingHook.GameplayMode mode, int initialCounter, int style = 0)
        {
            GrapplingHook hook = DataContainer.For(self).GrappleHook;
            if (hook == null)
            {
                hook = new GrapplingHook(size, length, material);
                hook.ChangeGameplayMode(mode, self.level, initialCounter);
                hook.SetStyle(style);
                hook.Owner = self;
                DataContainer.For(self).GrappleHook = hook;
                GrappleIndicator indicator = new GrappleIndicator(GFX.Game["objects/hook/indicator"], self);
                self.Scene.Add(indicator);
                DataContainer.For(self).GrappleIndicator = indicator;
            }
        }

        public static void UninitializeGrapplingHook(this Player self)
        {
            DataContainer.For(self).GrappleHook = null;
            DataContainer.For(self).GrappleIndicator = null;
        }

        public static GrapplingHook GetGrappleHook(this Player self)
        {
            return DataContainer.For(self).GrappleHook;
        }

        public static GrappleIndicator GetGrappleIndicator(this Player self)
        {
            return DataContainer.For(self).GrappleIndicator;
        }

        public static ShotHookCheck GetShootHookCheck(this Player self)
        {
            return DataContainer.For(self).ShootCheck;
        }

        public static Vector2 ExactCenter(this Player self)
        {
            return self.ExactPosition + self.Center - self.Position;
        }

        public static bool IsBoosterDash(this Player self)
        {
            return self.StateMachine.State == (int)AquaStates.StDash && DataContainer.For(self).IsBoosterDash;
        }

        public static void BounceDown(this Player self, float fromY)
        {
            if (self.StateMachine.State == 4 && self.CurrentBooster != null)
            {
                self.CurrentBooster.PlayerReleased();
                self.CurrentBooster = null;
            }

            Collider collider = self.Collider;
            self.Collider = self.normalHitbox;
            self.MoveV(self.Top - fromY);
            if (!self.Inventory.NoRefills)
            {
                self.RefillDash();
            }

            self.RefillStamina();
            self.StateMachine.State = 0;
            self.jumpGraceTimer = 0f;
            self.varJumpTimer = 0.2f;
            self.AutoJump = true;
            self.AutoJumpTimer = 0f;
            self.dashAttackTimer = 0f;
            self.gliderBoostTimer = 0f;
            self.wallSlideTimer = 1.2f;
            self.wallBoostTimer = 0f;
            self.Speed.X = 0f;
            self.varJumpSpeed = (self.Speed.Y = 185f);
            self.launched = false;
            self.level.DirectionalShake(Vector2.UnitY, 0.1f);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            self.Sprite.Scale = new Vector2(0.5f, 1.5f);
            self.Collider = collider;
        }
    }
}
