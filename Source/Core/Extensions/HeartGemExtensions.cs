using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class HeartGemExtensions
    {
        public static void Initialize()
        {
            On.Celeste.HeartGem.ctor_Vector2 += HeartGem_Construct;
            On.Celeste.SaveData.RegisterHeartGem += SaveData_RegisterHeartGem;
        }

        public static void Uninitialize()
        {
            On.Celeste.HeartGem.ctor_Vector2 -= HeartGem_Construct;
            On.Celeste.SaveData.RegisterHeartGem -= SaveData_RegisterHeartGem;
        }

        private static void HeartGem_Construct(On.Celeste.HeartGem.orig_ctor_Vector2 orig, HeartGem self, Vector2 position)
        {
            orig(self, position);
            self.SetHookable(true);
            self.Add(new HookCollider(self.OnHookCollide));
        }

        private static void SaveData_RegisterHeartGem(On.Celeste.SaveData.orig_RegisterHeartGem orig, SaveData self, AreaKey area)
        {
            Level level = (Engine.Instance.scene as Level);
            if (level != null)
            {
                Player player = level.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    GrapplingHook hook = player.GetGrappleHook();
                    if (hook != null && hook.State == GrapplingHook.HookStates.Fixed)
                    {
                        hook.Revoke();
                    }
                }
            }
            orig(self, area);
        }

        private static void OnHookCollide(this HeartGem self, GrapplingHook hook)
        {
            hook.Revoke();
            if (self.bounceSfxDelay <= 0f)
            {
                if (self.IsFake)
                {
                    Audio.Play("event:/new_content/game/10_farewell/fakeheart_bounce", self.Position);
                }
                else
                {
                    Audio.Play("event:/game/general/crystalheart_bounce", self.Position);
                }

                self.bounceSfxDelay = 0.1f;
            }

            self.moveWiggler.Start();
            self.ScaleWiggler.Start();
            self.moveWiggleDir = (self.Center - hook.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }
    }
}
