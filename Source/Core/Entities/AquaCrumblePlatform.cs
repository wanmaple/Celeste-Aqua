using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Crumble Platform")]
    public class AquaCrumblePlatform : CrumblePlatform
    {
        public string OutlineTexture { get; private set; }
        public bool OneUse { get; private set; }
        public float RespawnDuration { get; private set; }
        public float MinCrumbleDurationOnTop { get; private set; }
        public float MaxCrumbleDurationOnTop { get; private set; }
        public float CrumbleDurationOnSide { get; private set; }
        public float CrumbleDurationAttached { get; private set; }
        public Color CrumbleParticleColor { get; private set; }


        public AquaCrumblePlatform(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            OverrideTexture = data.Attr("platform_texture");
            OutlineTexture = data.Attr("outline_texture", "objects/crumbleBlock/outline");
            OneUse = data.Bool("one_use", false);
            RespawnDuration = MathF.Max(data.Float("respawn_duration", 2.0f), 0.0f);
            MinCrumbleDurationOnTop = MathF.Max(data.Float("min_crumble_duration_on_top", 0.2f), 0.0f);
            MaxCrumbleDurationOnTop = MathF.Max(data.Float("max_crumble_duration_on_top", 0.6f), MinCrumbleDurationOnTop);
            CrumbleDurationOnSide = MathF.Max(data.Float("crumble_duration_on_side", 1.0f), MaxCrumbleDurationOnTop);
            CrumbleDurationAttached = MathF.Max(data.Float("crumble_duration_attached", 1.0f), 0.0f);
            CrumbleParticleColor = data.HexColor("crumble_particle_color", Calc.HexToColor("847e87"));
            _crumbleParticle = new ParticleType(P_Crumble);
            _crumbleParticle.Color = CrumbleParticleColor;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (Component com in Components)
            {
                if (com is Coroutine coroutine)
                {
                    if (coroutine == outlineFader)
                        continue;
                    if (falls.Contains(coroutine))
                        continue;
                    coroutine.Replace(CustomSequence());
                }
            }
        }

        private IEnumerator CustomSequence()
        {
            while (true)
            {
                bool onTop;
                bool attached;
                if (GetPlayerOnTop() != null)
                {
                    onTop = true;
                    attached = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else if (GetPlayerClimbing() != null)
                {
                    onTop = false;
                    attached = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else if (this.IsHookAttached())
                {
                    onTop = false;
                    attached = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else
                {
                    yield return null;
                    continue;
                }

                Audio.Play("event:/game/general/platform_disintegrate", Center);
                float shakeDuration = onTop ? MaxCrumbleDurationOnTop : (attached ? CrumbleDurationAttached : CrumbleDurationOnSide);
                shaker.ShakeFor(shakeDuration, removeOnFinish: false);
                foreach (Image image in images)
                {
                    SceneAs<Level>().Particles.Emit(_crumbleParticle, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }

                float emissionTime;
                float waitTime;
                if (attached)
                {
                    emissionTime = CrumbleDurationAttached;
                    waitTime = 0.0f;
                }
                else
                {
                    emissionTime = onTop ? MinCrumbleDurationOnTop : CrumbleDurationOnSide;
                    waitTime = onTop ? MaxCrumbleDurationOnTop - MinCrumbleDurationOnTop : 0.0f;
                }
                while (emissionTime > 0.0f)
                {
                    if (emissionTime < 0.2f)
                    {
                        yield return emissionTime;
                        break;
                    }
                    yield return 0.2f;
                    foreach (Image image2 in images)
                    {
                        SceneAs<Level>().Particles.Emit(_crumbleParticle, 2, Position + image2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                    emissionTime -= 0.2f;
                }

                float timer = waitTime;
                if (onTop)
                {
                    while (timer > 0f && GetPlayerOnTop() != null)
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

                outlineFader.Replace(OutlineFade(1.0f));
                occluder.Visible = false;
                Collidable = false;
                float num = 0.05f;
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < images.Count; k++)
                    {
                        if (k % 4 - j == 0)
                        {
                            falls[k].Replace(TileOut(images[fallOrder[k]], num * (float)j));
                        }
                    }
                }

                if (OneUse)
                {
                    outlineFader.Replace(OutlineFade(0.0f));
                    break;
                }

                yield return RespawnDuration;
                while (CollideCheck<Actor>() || CollideCheck<Solid>() || CollideCheck<GrapplingHook>() || this.IntersectsWithRope())
                {
                    yield return null;
                }

                outlineFader.Replace(OutlineFade(0.0f));
                occluder.Visible = true;
                Collidable = true;
                for (int l = 0; l < 4; l++)
                {
                    for (int m = 0; m < images.Count; m++)
                    {
                        if (m % 4 - l == 0)
                        {
                            falls[m].Replace(TileIn(m, images[fallOrder[m]], 0.05f * (float)l));
                        }
                    }
                }
            }
        }

        private ParticleType _crumbleParticle;
    }
}
