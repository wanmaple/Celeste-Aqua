using Celeste.Mod.Aqua.Rendering;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Rod Zip Mover")]
    [Tracked(false)]
    public class RodZipMover : ZipMover, IRodControllable, ICustomRenderEntity
    {
        public string Flag { get; private set; }
        public float Duration { get; private set; }
        public float HueOffset { get; private set; }
        public float SaturationOffset { get; private set; }
        public bool State { get; set; }
        public bool IsRunning { get; private set; }

        public RodZipMover(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            Duration = data.Has("duration") ? Calc.Max(data.Float("duration"), 0.1f) : 0.5f;
            HueOffset = data.Float("hue_offset");
            SaturationOffset = data.Float("saturation_offset");
            Coroutine coroutine = Get<Coroutine>();
            Remove(coroutine);
            Add(new Coroutine(RodSequence()));
            _fx = FXCenter.Instance.GetFX("hue_offset");
            if (_fx != null)
            {
                _fx.Parameters["HueOffset"].SetValue(HueOffset);
                _fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
            AddTag(RenderTags.CustomEntity);
        }

        public Effect GetEffect()
        {
            return _fx;
        }

        public void OnReload()
        {
            _fx = FXCenter.Instance.GetFX("hue_offset");
            if (_fx != null)
            {
                _fx.Parameters["HueOffset"].SetValue(HueOffset);
                _fx.Parameters["SaturationOffset"].SetValue(SaturationOffset);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            RodEntityManager.Instance.Add(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RodEntityManager.Instance.Remove(this);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RodEntityManager.Instance.Remove(this);
        }

        public override void Update()
        {
            _lastState = State;
            streetlight.SetAnimationFrame(IsRunning ? 3 : 1);
            if (!IsRunning && SceneAs<Level>().Session.GetFlag(Flag))
            {
                State = !State;
            }
            base.Update();
        }

        private IEnumerator RodSequence()
        {
            while (true)
            {
                if (!IsRunning && _lastState != State)
                {
                    IsRunning = true;
                    sfx.Play((theme == Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover_no_return" : "event:/new_content/game/10_farewell/zip_mover_no_return");
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    StartShaking(0.1f);
                    yield return 0.1f;
                    StopPlayerRunIntoAnimation = false;
                    Vector2 begin = State ? target : start;
                    Vector2 end = State ? start : target;
                    float at2 = 0.0f;
                    while (at2 < Duration)
                    {
                        yield return null;
                        at2 = Calc.Approach(at2, Duration, Engine.DeltaTime);
                        percent = Ease.SineIn(at2 / Duration);
                        Vector2 vector = Vector2.Lerp(end, begin, percent);
                        ScrapeParticlesCheck(vector);
                        if (Scene.OnInterval(0.1f))
                        {
                            pathRenderer.CreateSparks();
                        }
                        MoveTo(vector);
                    }
                    StopPlayerRunIntoAnimation = true;
                    yield return 0.2f;
                    SceneAs<Level>().Session.SetFlag(Flag, false);
                    IsRunning = false;
                }
                yield return null;
            }
        }

        private bool _lastState = false;
        private Effect _fx;
    }
}
