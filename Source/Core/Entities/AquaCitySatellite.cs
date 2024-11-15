using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua City Satellite")]
    public class AquaCitySatellite : PuzzleEntity
    {
        public AquaCitySatellite(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Add(_sprite = new Image(GFX.Game["objects/citysatellite/dish"]));
            Add(_pulse = new Image(GFX.Game["objects/citysatellite/light"]));
            Add(_computer = new Image(GFX.Game["objects/citysatellite/computer"]));
            Add(_computerScreen = new Image(GFX.Game["objects/citysatellite/computerscreen"]));
            Add(_computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise"));
            Add(_computerScreenShine = new Image(GFX.Game["objects/citysatellite/computerscreenShine"]));
            _sprite.JustifyOrigin(0.5f, 1f);
            _pulse.JustifyOrigin(0.5f, 1f);
            Add(new Coroutine(PulseRoutine()));
            Add(_pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f));
            Add(_screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f));
            _computerScreenNoise.AddLoop("static", "", 0.05f);
            _computerScreenNoise.Play("static");
            _computer.Position = (_computerScreen.Position = (_computerScreenShine.Position = (_computerScreenNoise.Position = new Vector2(8f, 8f))));
            BonusPosition = offset + data.Nodes[0];
            Add(_staticLoopSfx = new SoundSource());
            _staticLoopSfx.Position = _computer.Position;
            _puzzleClosed = AquaModule.Session.HasFlag(AquaModuleSession.CH1A_SATELLITE_SOLVED);
        }

        private bool _enabled;
        private Image _sprite;
        private Image _pulse;
        private Image _computer;
        private Image _computerScreen;
        private Sprite _computerScreenNoise;
        private Image _computerScreenShine;
        private BloomPoint _pulseBloom;
        private BloomPoint _screenBloom;
        private SoundSource _staticLoopSfx;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _enabled = true;
            if (!_enabled)
            {
                _staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
            }

            Level level = scene as Level;
            if (!level.Session.HeartGem && AquaModule.Session.HasFlag(AquaModuleSession.CH1A_SATELLITE_SOLVED))
            {
                HeartGem entity = new HeartGem(BonusPosition);
                scene.Add(entity);
            }
        }

        public override void Update()
        {
            base.Update();
            _computerScreenNoise.Visible = !_pulse.Visible;
            _computerScreen.Visible = _pulse.Visible;
            _screenBloom.Visible = _pulseBloom.Visible;
            if (_puzzleClosed)
            {
                foreach (PuzzleLight light in RelatedLights)
                {
                    light.SwitchOn = true;
                }
            }
        }

        public override bool CanLitOn(PuzzleLight light)
        {
            if (_puzzleClosed)
                return false;

            PuzzleLight[] lights = RelatedLights.ToArray();
            int index = Array.IndexOf(lights, light);
            if (index == 0 || lights[index - 1].SwitchOn)
            {
                return true;
            }
            return false;
        }

        public override bool CanLitOff(PuzzleLight light)
        {
            return false;
        }

        protected override bool IsSolved()
        {
            foreach (PuzzleLight light in RelatedLights)
            {
                if (!light.SwitchOn)
                {
                    return false;
                }
            }
            return true;
        }

        protected override IEnumerator GenerateBonus()
        {
            AquaModule.Session.MarkFlag(AquaModuleSession.CH1A_SATELLITE_SOLVED);
            yield return 0.25f;

            Level level = Scene as Level;
            level.Displacement.Clear();
            yield return null;

            level.Frozen = true;
            Tag = Tags.FrozenUpdate;
            yield return 0.25f;

            HeartGem gem = new HeartGem(BonusPosition)
            {
                Tag = Tags.FrozenUpdate
            };
            level.Add(gem);
            yield return null;

            gem.ScaleWiggler.Start();
            yield return 0.85f;

            SimpleCurve curve = new SimpleCurve(gem.Position, BonusPosition, (gem.Position + BonusPosition) / 2f + new Vector2(0f, -64f));
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                yield return null;
                gem.Position = curve.GetPoint(Ease.CubeInOut(t));
            }

            yield return 0.5f;
            level.Frozen = false;
        }

        private IEnumerator PulseRoutine()
        {
            _pulseBloom.Visible = (_pulse.Visible = false);
            while (_enabled)
            {
                yield return 2f;
                int i = 0;
                foreach (PuzzleLight light in RelatedLights)
                {
                    if (!_enabled || !light.SwitchOn)
                    {
                        break;
                    }

                    _pulse.Color = _computerScreen.Color = light.LitColor;
                    _pulseBloom.Visible = (_pulse.Visible = true);
                    Audio.Play(SIGNAL_SOUNDS[i], Position + _computer.Position);
                    yield return 0.5f;
                    _pulseBloom.Visible = (_pulse.Visible = false);
                    Audio.Play((i < RelatedLights.Count - 1) ? "event:/game/01_forsaken_city/console_static_short" : "event:/game/01_forsaken_city/console_static_long", Position + _computer.Position);
                    ++i;
                    yield return 0.2f;
                }
            }

            _pulseBloom.Visible = (_pulse.Visible = false);
        }

        private static readonly List<string> SIGNAL_SOUNDS = new List<string>
        {
            "event:/game/01_forsaken_city/console_yellow",
            "event:/game/01_forsaken_city/console_red",
            "event:/game/01_forsaken_city/console_blue",
            "event:/game/01_forsaken_city/console_purple",
            "event:/game/01_forsaken_city/console_white",
        };
    }
}
