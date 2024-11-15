using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class BumperExtensions
    {
        public static void Hit(this Bumper self, Vector2 direction)
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
