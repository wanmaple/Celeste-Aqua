using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Magnet Zipper")]
    public class GrappleMagnetZipMover : GrappleMagnet
    {
        public IReadOnlyList<Vector2> Nodes { get; private set; }
        public IReadOnlyList<float> SpeedMultipliers { get; private set; }
        public IReadOnlyList<float> Delays { get; private set; }
        public IReadOnlyList<string> MoveFlags { get; private set; }
        public float ReturnSpeedMultiplier { get; private set; }
        public float DelayBeforeReturn { get; private set; }
        public bool UseFlagToStart { get; private set; }
        public bool OneUse { get; private set; }
        public string CogTexture { get; private set; }
        public Color ChainColor { get; private set; }
        public Color ChainLightColor { get; private set; }

        public GrappleMagnetZipMover(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Nodes = new Vector2[] { Position, }.Concat(data.Nodes.Select(node => node + offset)).ToArray();
            string speedMultAttr = data.Attr("speed_multipliers", string.Empty);
            string delayAttr = data.Attr("delays", string.Empty);
            string flagAttr = data.Attr("move_flags", string.Empty);
            ReturnSpeedMultiplier = data.Float("return_speed_multiplier", 1.0f);
            DelayBeforeReturn = data.Float("delay_before_return", 0.5f);
            UseFlagToStart = data.Bool("use_flag_to_trig", false);
            OneUse = data.Bool("one_use", false);
            CogTexture = data.Attr("cog_texture", "objects/zipmover/cog");
            ChainColor = data.HexColor("chain_color", Calc.HexToColor("663931"));
            ChainLightColor = data.HexColor("chain_light_color", Calc.HexToColor("9b6157"));
            string[] spdMults = speedMultAttr.Split(',');
            string[] delays = delayAttr.Split(',');
            string[] flags = flagAttr.Split(',');
            float lastSpdMultiplier = 1.0f;
            float lastDelay = 0.2f;
            string lastFlag = string.Empty;
            List<float> spdMultList = new List<float>(data.Nodes.Length);
            List<float> delayList = new List<float>(data.Nodes.Length);
            List<string> flagList = new List<string>(data.Nodes.Length);
            for (int i = 0; i < data.Nodes.Length; i++)
            {
                if (i < spdMults.Length && float.TryParse(spdMults[i], out float spdMult))
                {
                    lastSpdMultiplier = MathF.Max(spdMult, 0.1f);
                }
                if (i < delays.Length && float.TryParse(delays[i], out float delay))
                {
                    lastDelay = MathF.Max(delay, 0.0f);
                }
                if (i < flags.Length)
                {
                    lastFlag = flags[i];
                }
                spdMultList.Add(lastSpdMultiplier);
                delayList.Add(lastDelay);
                flagList.Add(lastFlag);
            }
            SpeedMultipliers = spdMultList;
            Delays = delayList;
            MoveFlags = flagList;
            if (!GFX.Game.Has(CogTexture))
                CogTexture = "objects/zipmover/cog";
            _texCog = GFX.Game[CogTexture];
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (Nodes.Count > 1)
                Add(new Coroutine(Sequence()));
        }

        public override void Update()
        {
            base.Update();
            _sfx.Position = Position;
            _sound.Position = Position;
        }

        public override void Render()
        {
            DrawCogs(Vector2.UnitY, Color.Black);
            DrawCogs(Vector2.Zero);
            base.Render();
        }

        private void DrawCogs(Vector2 offset, Color? colorOverride = null)
        {
            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                Vector2 to = Nodes[i + 1];
                Vector2 from = Nodes[i];
                Vector2 vector = (to - from).SafeNormalize();
                Vector2 vector2 = vector.Perpendicular() * 3.0f;
                Vector2 vector3 = -vector.Perpendicular() * 4.0f;
                float rotation = _cogPercent * MathF.PI * 2.0f;
                Draw.Line(from + vector2 + offset, to + vector2 + offset, colorOverride.HasValue ? colorOverride.Value : ChainColor);
                Draw.Line(from + vector3 + offset, to + vector3 + offset, colorOverride.HasValue ? colorOverride.Value : ChainColor);
                for (float num = 4.0f - _cogPercent * MathF.PI * 8.0f % 4.0f; num < (to - from).Length(); num += 4.0f)
                {
                    Vector2 vector4 = from + vector2 + vector.Perpendicular() + vector * num;
                    Vector2 vector5 = to + vector3 - vector * num;
                    Draw.Line(vector4 + offset, vector4 + vector * 2.0f + offset, colorOverride.HasValue ? colorOverride.Value : ChainLightColor);
                    Draw.Line(vector5 + offset, vector5 - vector * 2.0f + offset, colorOverride.HasValue ? colorOverride.Value : ChainLightColor);
                }

                _texCog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1.0f, rotation);
                _texCog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1.0f, rotation);
            }
        }

        private IEnumerator Sequence()
        {
            float dt = Engine.DeltaTime;
            while (true)
            {
                int currentIndex = 0;
                while (currentIndex < Nodes.Count - 1)
                {
                    while ((!UseFlagToStart && !this.IsHookAttached()) || (UseFlagToStart && !SceneAs<Level>().Session.GetFlag(MoveFlags[currentIndex])))
                    {
                        yield return null;
                    }
                    if (UseFlagToStart)
                        SceneAs<Level>().Session.SetFlag(MoveFlags[currentIndex], false);
                    yield return Delays[currentIndex];
                    // Move to the next node
                    _sfx.Play("event:/game/01_forsaken_city/zip_mover_touch");
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                    Vector2 moveDir = Calc.SafeNormalize(Nodes[currentIndex + 1] - Nodes[currentIndex]);
                    float speedMultiplier = SpeedMultipliers[currentIndex];
                    int nextIndex = currentIndex + 1;
                    float t = 0.0f;
                    while (t < 1.0f)
                    {
                        yield return null;
                        t = Calc.Approach(t, 1.0f, 2.0f * Engine.DeltaTime * speedMultiplier);
                        _cogPercent = Ease.SineIn(t);
                        Vector2 nextPos = Vector2.Lerp(Nodes[currentIndex], Nodes[nextIndex], _cogPercent);
                        NaiveMove(nextPos - Position);
                        if (Scene.OnInterval(0.02f))
                        {
                            EmitParticles(moveDir);
                        }
                    }
                    _sfx.Play("event:/game/01_forsaken_city/zip_mover_impact");
                    currentIndex++;
                    if (currentIndex >= Nodes.Count - 1)
                    {
                        yield return DelayBeforeReturn;
                        if (OneUse)
                        {
                            SetActivated(false);
                            yield break;
                        }
                    }
                }
                if (!OneUse)
                {
                    while (currentIndex > 0)
                    {
                        _sfx.Play("event:/game/01_forsaken_city/zip_mover_return");
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                        int nextIndex = currentIndex - 1;
                        float t = 0.0f;
                        while (t < 1.0f)
                        {
                            yield return null;
                            t = Calc.Approach(t, 1.0f, 0.5f * Engine.DeltaTime * ReturnSpeedMultiplier);
                            _cogPercent = 1.0f - Ease.SineIn(t);
                            Vector2 nextPos = Vector2.Lerp(Nodes[nextIndex], Nodes[currentIndex], _cogPercent);
                            NaiveMove(nextPos - Position);
                        }
                        currentIndex--;
                    }
                    _sfx.Play("event:/game/01_forsaken_city/zip_mover_reset");
                    yield return 0.2f;
                }
            }
        }

        private float _cogPercent;
        private MTexture _texCog;
        private SoundSource _sfx = new SoundSource();
    }
}
