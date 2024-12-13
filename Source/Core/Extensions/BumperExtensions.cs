using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class BumperExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Bumper.ctor_Vector2_Nullable1 += Bumper_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Bumper.ctor_Vector2_Nullable1 -= Bumper_Construct;
        }

        private static void Bumper_Construct(On.Celeste.Bumper.orig_ctor_Vector2_Nullable1 orig, Bumper self, Vector2 position, Vector2? node)
        {
            orig(self, position, node);
            self.SetHookable(true);
            self.Add(new HookInteractable(self.OnInteractHook));
        }

        private static bool OnInteractHook(this Bumper self, GrapplingHook hook, Vector2 at)
        {
            Vector2 direction = Vector2.Normalize(at - self.Center);
            if (hook.Bounce(direction, GrapplingHook.BOUNCE_SPEED_ADDITION))
            {
                self.Hit(direction);
                return true;
            }
            return false;
        }

        private static void Hit(this Bumper self, Vector2 direction)
        {
            if ((self.Scene as Level).Session.Area.ID == 9)
            {
                Audio.Play("event:/game/09_core/pinballbumper_hit", self.Position);
            }
            else
            {
                Audio.Play("event:/game/06_reflection/pinballbumper_hit", self.Position);
            }

            self.respawnTimer = 0.6f;
            self.sprite.Play("hit", restart: true);
            self.spriteEvil.Play("hit", restart: true);
            self.light.Visible = false;
            self.bloom.Visible = false;
            //self.SceneAs<Level>().DirectionalShake(direction, 0.15f);
            self.SceneAs<Level>().Displacement.AddBurst(self.Center, 0.3f, 8f, 32f, 0.8f);
            self.SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 12, self.Center + direction * 12f, Vector2.One * 3f, direction.Angle());
        }
    }
}
