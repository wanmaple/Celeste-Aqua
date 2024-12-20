using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Puzzle Wheel")]
    public class PuzzleWheel : PuzzleEntity, IRodControllable
    {
        public string Flag { get; private set; }
        public float WheelRadius { get; private set; }
        public float TimeLimit { get; private set; }
        public bool IsRunning { get; private set; }

        public PuzzleWheel(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Flag = data.Attr("flag");
            WheelRadius = 16.0f;
            TimeLimit = data.Float("time_limit", 30.0f);
            Add(new Coroutine(RotateWheel()));
            var imgBg = new Image(GFX.Game["objects/puzzle_wheel/background"]);
            imgBg.SetColor(Calc.HexToColor("5a5a5a"));
            imgBg.JustifyOrigin(0.5f, 0.5f);
            Add(imgBg);
            Add(_imgPin = new Image(GFX.Game["objects/puzzle_wheel/pin"]));
            _imgPin.SetColor(Calc.HexToColor("05ecfb"));
            _stopwatch = new SoundSource();
            _imgPin.JustifyOrigin(0.5f, 0.5f);
            BonusPosition = offset + data.Nodes[0];
            _puzzleClosed = IsRunning = AquaModule.Session.HasFlag(AquaModuleSession.CH2A_WHEEL_SOLVED);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            RodEntityManager.Instance.Add(this);
            Level level = scene as Level;
            if (!level.Session.HeartGem && AquaModule.Session.HasFlag(AquaModuleSession.CH2A_WHEEL_SOLVED))
            {
                HeartGem entity = new HeartGem(BonusPosition);
                scene.Add(entity);
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RodEntityManager.Instance.Remove(this);
            _stopwatch.Stop();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RodEntityManager.Instance.Remove(this);
            _stopwatch.Stop();
        }

        public override void Update()
        {
            base.Update();
            if (!_initialized)
            {
                GenerateCircles();
                _initialized = true;
            }
            if (_puzzleClosed)
            {
                foreach (PuzzleLight light in RelatedLights)
                {
                    light.SwitchOn = true;
                }
            }
            Level level = SceneAs<Level>();
            if (level.Session.GetFlag(Flag))
            {
                _trigged = true;
            }
        }

        public override bool CanLitOn(PuzzleLight light)
        {
            return _pointingLight == light;
        }

        protected override bool IsSolved()
        {
            return _solved;
        }

        protected override IEnumerator GenerateBonus()
        {
            AquaModule.Session.MarkFlag(AquaModuleSession.CH2A_WHEEL_SOLVED);
            yield return 0.25f;

            Level level = Scene as Level;
            level.Displacement.Clear();
            yield return null;

            level.Frozen = true;
            Tag = Tags.FrozenUpdate;
            yield return 0.25f;

            HeartGem gem = new HeartGem(Position)
            {
                Tag = Tags.FrozenUpdate,
            };
            level.Add(gem);
            yield return null;

            gem.ScaleWiggler.Start();
            yield return 0.85f;

            SimpleCurve curve = new SimpleCurve(Position, BonusPosition, (gem.Position + BonusPosition) / 2.0f + new Vector2(0f, -64.0f));
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                yield return null;
                gem.Position = curve.GetPoint(Ease.CubeInOut(t));
            }

            yield return 0.5f;
            level.Frozen = false;
        }

        private void GenerateCircles()
        {
            if (RelatedLights.Count > 0)
            {
                float radianGap = MathF.PI * 2.0f / RelatedLights.Count;
                int index = 0;
                foreach (PuzzleLight light in RelatedLights)
                {
                    Image image = new Image(GFX.Game["objects/puzzle_wheel/ring"]).SetColor(light.LitColor);
                    image.JustifyOrigin(0.5f, 0.5f);
                    image.RenderPosition = Calc.AngleToVector(radianGap * index - MathF.PI * 0.5f, WheelRadius);
                    Add(image);
                    ++index;
                }
            }
        }

        private IEnumerator RotateWheel()
        {
            while (!_puzzleClosed)
            {
                _trigged = false;
                while (!_trigged)
                    yield return null;

                IsRunning = true;
                if (RelatedLights.Count > 0)
                {
                    PuzzleLight[] lights = RelatedLights.ToArray();
                    PuzzleLight[] origLights = RelatedLights.ToArray();
                    _stopwatch.Play("event:/game/general/stopwatch");
                    int randomRange = lights.Length;
                    var random = new Random();
                    int currentIndex = random.Next(randomRange);
                    _pointingLight = lights[currentIndex];
                    lights[currentIndex] = lights[lights.Length - 1];
                    randomRange--;
                    float radianGap = MathF.PI * 2.0f / lights.Length;
                    float targetRadian = currentIndex * radianGap;
                    float startRadian = 0.0f;
                    float rotateDuration = 1.0f;
                    float elapsed = 0.0f;
                    float rotateElapsed = 0.0f;
                    do
                    {
                        elapsed += Engine.DeltaTime;
                        rotateElapsed += Engine.DeltaTime;
                        float diffRadian = targetRadian - startRadian;
                        if (diffRadian > MathF.PI)
                            diffRadian = diffRadian - MathF.PI * 2.0f;
                        else if (diffRadian < -MathF.PI)
                            diffRadian = diffRadian + MathF.PI * 2.0f;
                        float t = Calc.Clamp(rotateElapsed / rotateDuration, 0.0f, 1.0f);
                        _imgPin.Rotation = MathHelper.Lerp(startRadian, startRadian + diffRadian, Ease.SineInOut(t));
                        if (_pointingLight != null && _pointingLight.SwitchOn)
                        {
                            if (randomRange > 0)
                            {
                                // find furthest puzzle light.
                                float maxDistance = float.MinValue;
                                for (int i = 0; i < randomRange; ++i)
                                {
                                    PuzzleLight light = lights[i];
                                    float disSq = (light.Position - _pointingLight.Position).LengthSquared();
                                    if (disSq > maxDistance)
                                    {
                                        maxDistance = disSq;
                                        _pointingLight = light;
                                        currentIndex = i;
                                    }
                                }
                                startRadian = _imgPin.Rotation;
                                targetRadian = Array.IndexOf(origLights, _pointingLight) * radianGap;
                                lights[currentIndex] = lights[--randomRange];
                                rotateElapsed = 0.0f;
                            }
                            else
                            {
                                _solved = true;
                                break;
                            }
                        }
                        yield return null;
                    }
                    while (elapsed < TimeLimit);
                    _stopwatch.Stop();
                    startRadian = _imgPin.Rotation;
                    targetRadian = 0.0f;
                    float diff = targetRadian - startRadian;
                    if (diff > MathF.PI)
                        diff = diff - MathF.PI * 2.0f;
                    else if (diff < -MathF.PI)
                        diff = diff + MathF.PI * 2.0f;
                    for (float t = 0.0f; t < rotateDuration; t = Calc.Clamp(t + Engine.DeltaTime, 0.0f, rotateDuration))
                    {
                        _imgPin.Rotation = MathHelper.Lerp(startRadian, startRadian + diff, Ease.SineInOut(t));
                        yield return null;
                    }
                }
                _pointingLight = null;
                if (!_puzzleClosed)
                {
                    foreach (PuzzleLight light in RelatedLights)
                    {
                        light.SwitchOn = false;
                    }
                }
                SceneAs<Level>().Session.SetFlag(Flag, false);
                IsRunning = _puzzleClosed;
            }
        }

        private bool _initialized = false;
        private PuzzleLight _pointingLight;
        private bool _trigged;
        private Image _imgPin;
        private bool _solved;
        private SoundSource _stopwatch;
    }
}
