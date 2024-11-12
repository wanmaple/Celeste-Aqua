using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class ZipMoverExtensions
    {
        public static void Initialize()
        {
            On.Celeste.ZipMover.Sequence += ZipMover_Sequence;
        }

        public static void Uninitialize()
        {
            On.Celeste.ZipMover.Sequence -= ZipMover_Sequence;
        }

        private static System.Collections.IEnumerator ZipMover_Sequence(On.Celeste.ZipMover.orig_Sequence orig, ZipMover self)
        {
            Vector2 start = self.Position;
            while (true)
            {
                if (!self.HasPlayerRider() && !self.IsHookAttached())
                {
                    yield return null;
                    continue;
                }

                self.sfx.Play((self.theme == ZipMover.Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                self.StartShaking(0.1f);
                yield return 0.1f;
                self.streetlight.SetAnimationFrame(3);
                self.StopPlayerRunIntoAnimation = false;
                float at2 = 0f;
                while (at2 < 1f)
                {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime);
                    self.percent = Ease.SineIn(at2);
                    Vector2 vector = Vector2.Lerp(start, self.target, self.percent);
                    self.ScrapeParticlesCheck(vector);
                    if (self.Scene.OnInterval(0.1f))
                    {
                        self.pathRenderer.CreateSparks();
                    }

                    self.MoveTo(vector);
                }

                self.StartShaking(0.2f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                self.SceneAs<Level>().Shake();
                self.StopPlayerRunIntoAnimation = true;
                yield return 0.5f;
                self.StopPlayerRunIntoAnimation = false;
                self.streetlight.SetAnimationFrame(2);
                at2 = 0f;
                while (at2 < 1f)
                {
                    yield return null;
                    at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
                    self.percent = 1f - Ease.SineIn(at2);
                    Vector2 position = Vector2.Lerp(self.target, start, Ease.SineIn(at2));
                    self.MoveTo(position);
                }

                self.StopPlayerRunIntoAnimation = true;
                self.StartShaking(0.2f);
                self.streetlight.SetAnimationFrame(1);
                yield return 0.5f;
            }
        }
    }
}
