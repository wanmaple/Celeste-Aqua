using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    public static class CrumblePlatformExtensions
    {
        public static void Initialize()
        {
            On.Celeste.CrumblePlatform.Sequence += CrumblePlatform_Sequence;
        }

        public static void Uninitialize()
        {
            On.Celeste.CrumblePlatform.Sequence -= CrumblePlatform_Sequence;
        }

        private static IEnumerator CrumblePlatform_Sequence(On.Celeste.CrumblePlatform.orig_Sequence orig, CrumblePlatform self)
        {
            while (true)
            {
                bool onTop;
                if (self.GetPlayerOnTop() != null)
                {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else
                {
                    if (self.GetPlayerClimbing() == null && !self.IsHookAttached())
                    {
                        yield return null;
                        continue;
                    }

                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                Audio.Play("event:/game/general/platform_disintegrate", self.Center);
                self.shaker.ShakeFor(onTop ? 0.6f : 1f, removeOnFinish: false);
                foreach (Image image in self.images)
                {
                    self.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, self.Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }

                for (int i = 0; i < (onTop ? 1 : 3); i++)
                {
                    yield return 0.2f;
                    foreach (Image image2 in self.images)
                    {
                        self.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, self.Position + image2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }

                float timer = 0.4f;
                if (onTop)
                {
                    while (timer > 0f && self.GetPlayerOnTop() != null)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                else
                {
                    while (timer > 0f)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }

                self.outlineFader.Replace(self.OutlineFade(1f));
                self.occluder.Visible = false;
                self.Collidable = false;
                float num = 0.05f;
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < self.images.Count; k++)
                    {
                        if (k % 4 - j == 0)
                        {
                            self.falls[k].Replace(self.TileOut(self.images[self.fallOrder[k]], num * (float)j));
                        }
                    }
                }

                yield return 2f;
                while (self.CollideCheck<Actor>() || self.CollideCheck<Solid>() || self.CollideCheck<GrapplingHook>() || self.IntersectsWithRope())
                {
                    yield return null;
                }

                self.outlineFader.Replace(self.OutlineFade(0f));
                self.occluder.Visible = true;
                self.Collidable = true;
                for (int l = 0; l < 4; l++)
                {
                    for (int m = 0; m < self.images.Count; m++)
                    {
                        if (m % 4 - l == 0)
                        {
                            self.falls[m].Replace(self.TileIn(m, self.images[self.fallOrder[m]], 0.05f * (float)l));
                        }
                    }
                }
            }
        }
    }
}
