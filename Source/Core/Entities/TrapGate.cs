using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Trap Gate")]
    [Tracked(false)]
    public class TrapGate : Solid
    {
        public int Group { get; private set; }
        public IList<TrapButton> RelatedButtons { get; private set; } = new List<TrapButton>(4);

        public TrapGate(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            _openPosition = data.Nodes[0] + offset;
            _closePosition = Position;
            Add(_icon = new Sprite(GFX.Game, "objects/switchgate/icon"));
            _icon.Add("spin", "", 0.1f, "spin");
            _icon.Play("spin");
            _icon.Rate = 0f;
            _icon.Color = data.HexColor("color");
            _icon.Position = (_iconOffset = new Vector2(data.Width * 0.5f, data.Height * 0.5f));
            _icon.CenterOrigin();

            Add(_wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                _icon.Scale = Vector2.One * (1f + f);
            }));
            string spriteName = data.Attr("sprite", "block");
            MTexture mTexture = GFX.Game["objects/switchgate/" + spriteName];
            _nineSlice = new MTexture[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            Add(new LightOcclude(0.5f));

            if (P_Behind == null)
            {
                P_Behind = new ParticleType
                {
                    Color = Calc.HexToColor("ffeb6b"),
                    Color2 = Calc.HexToColor("d39332"),
                    ColorMode = ParticleType.ColorModes.Blink,
                    FadeMode = ParticleType.FadeModes.Late,
                    LifeMin = 1f,
                    LifeMax = 1.5f,
                    Size = 1f,
                    SpeedMin = 5f,
                    SpeedMax = 10f,
                    Acceleration = new Vector2(0f, 6f),
                    DirectionRange = MathF.PI * 2f
                };
                P_Dust = new ParticleType(ParticleTypes.Dust)
                {
                    LifeMin = 0.5f,
                    LifeMax = 1f,
                    SpeedMin = 2f,
                    SpeedMax = 4f
                };
            }
        }

        public override void Update()
        {
            base.Update();
            bool on = CheckButtons();
            if (_on != on)
            {
                if (_tween != null)
                {
                    _tween.Stop();
                    Remove(_tween);
                }
                if (on)
                {
                    if (_anim != null)
                    {
                        Remove(_anim);
                    }
                    Add(_anim = new Coroutine(OpenGate(_openPosition)));
                }
                else
                {
                    if (_anim != null)
                    {
                        Remove(_anim);
                    }
                    Add(_anim = new Coroutine(CloseGate(_closePosition)));
                }
                _on = on;
            }
        }

        public override void Render()
        {
            float num = Collider.Width / 8f - 1f;
            float num2 = Collider.Height / 8f - 1f;
            for (int i = 0; i <= num; i++)
            {
                for (int j = 0; j <= num2; j++)
                {
                    int num3 = ((i < num) ? Math.Min(i, 1) : 2);
                    int num4 = ((j < num2) ? Math.Min(j, 1) : 2);
                    _nineSlice[num3, num4].Draw(Position + Shake + new Vector2(i * 8, j * 8));
                }
            }

            _icon.Position = _iconOffset + Shake;
            _icon.DrawOutline();
            base.Render();
        }

        private IEnumerator OpenGate(Vector2 position)
        {
            Vector2 start = Position;
            yield return 0.1f;
            Audio.Play("event:/game/general/touchswitch_gate_open", Position);
            StartShaking(0.5f);
            while (_icon.Rate < 1.0f)
            {
                _icon.Rate += Engine.DeltaTime * 2.0f;
                yield return null;
            }

            yield return 0.1f;
            int particleAt = 0;
            float duration = (position - start).Length() / (position - _closePosition).Length() * 2.0f;
            _tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, true);
            _tween.OnUpdate = delegate (Tween t)
            {
                MoveTo(Vector2.Lerp(start, position, t.Eased));
                if (Scene.OnInterval(0.1f))
                {
                    particleAt++;
                    particleAt %= 2;
                    for (int x = 0; x < Width / 8; x++)
                    {
                        for (int y = 0; y < Height / 8; y++)
                        {
                            if ((x + y) % 2 == particleAt)
                            {
                                SceneAs<Level>().ParticlesBG.Emit(P_Behind, Position + new Vector2(x * 8, y * 8) + Calc.Random.Range(Vector2.One * 2.0f, Vector2.One * 6.0f));
                            }
                        }
                    }
                }
            };
            Add(_tween);
            yield return duration;
            bool collidable = Collidable;
            Collidable = false;
            if (position.X <= start.X)
            {
                Vector2 vector = new Vector2(0f, 2.0f);
                for (int i = 0; i < Height / 8.0f; i++)
                {
                    Vector2 vector2 = new Vector2(Left - 1.0f, Top + 4.0f + i * 8);
                    Vector2 point = vector2 + Vector2.UnitX;
                    if (Scene.CollideCheck<Solid>(vector2) && !Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector2 + vector, MathF.PI);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector2 - vector, MathF.PI);
                    }
                }
            }

            if (position.X >= start.X)
            {
                Vector2 vector3 = new Vector2(0f, 2.0f);
                for (int i = 0; i < Height / 8.0f; i++)
                {
                    Vector2 vector4 = new Vector2(Right + 1.0f, Top + 4.0f + i * 8);
                    Vector2 point2 = vector4 - Vector2.UnitX * 2f;
                    if (Scene.CollideCheck<Solid>(vector4) && !Scene.CollideCheck<Solid>(point2))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector4 + vector3, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector4 - vector3, 0f);
                    }
                }
            }

            if (position.Y <= start.Y)
            {
                Vector2 vector5 = new Vector2(2.0f, 0f);
                for (int i = 0; i < Width / 8.0f; i++)
                {
                    Vector2 vector6 = new Vector2(Left + 4.0f + i * 8, Top - 1.0f);
                    Vector2 point3 = vector6 + Vector2.UnitY;
                    if (Scene.CollideCheck<Solid>(vector6) && !Scene.CollideCheck<Solid>(point3))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector6 + vector5, -MathF.PI * 0.5f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector6 - vector5, -MathF.PI * 0.5f);
                    }
                }
            }

            if (position.Y >= start.Y)
            {
                Vector2 vector7 = new Vector2(2.0f, 0f);
                for (int i = 0; i < Width / 8.0f; i++)
                {
                    Vector2 vector8 = new Vector2(Left + 4f + (float)(i * 8), Bottom + 1.0f);
                    Vector2 point4 = vector8 - Vector2.UnitY * 2.0f;
                    if (Scene.CollideCheck<Solid>(vector8) && !Scene.CollideCheck<Solid>(point4))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector8 + vector7, MathF.PI * 0.5f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector8 - vector7, MathF.PI * 0.5f);
                    }
                }
            }

            Collidable = collidable;
            Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
            StartShaking(0.2f);
            while (_icon.Rate > 0.0f)
            {
                _icon.Rate -= Engine.DeltaTime * 4.0f;
                yield return null;
            }

            _icon.Rate = 0f;
            _icon.SetAnimationFrame(0);
            _wiggler.Start();
            collidable = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center))
            {
                for (int m = 0; m < 32; m++)
                {
                    float num = Calc.Random.NextFloat(MathF.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + _iconOffset + Calc.AngleToVector(num, 4f), num);
                }
            }

            Collidable = collidable;
        }

        private IEnumerator CloseGate(Vector2 position)
        {
            Vector2 start = Position;
            yield return 0.1f;
            Audio.Play("event:/game/general/touchswitch_gate_open", Position);
            //StartShaking(0.5f);
            _icon.Rate = 1.0f;
            int particleAt = 0;
            float duration = (position - start).Length() / (position - _openPosition).Length() * 1.0f;
            _tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, true);
            _tween.OnUpdate = delegate (Tween t)
            {
                MoveTo(Vector2.Lerp(start, position, t.Eased));
                if (Scene.OnInterval(0.1f))
                {
                    particleAt++;
                    particleAt %= 2;
                    for (int x = 0; x < Width / 8; x++)
                    {
                        for (int y = 0; y < Height / 8; y++)
                        {
                            if ((x + y) % 2 == particleAt)
                            {
                                SceneAs<Level>().ParticlesBG.Emit(P_Behind, Position + new Vector2(x * 8, y * 8) + Calc.Random.Range(Vector2.One * 2.0f, Vector2.One * 6.0f));
                            }
                        }
                    }
                }
            };
            Add(_tween);
            yield return duration;
            bool collidable = Collidable;
            Collidable = false;
            if (position.X <= start.X)
            {
                Vector2 vector = new Vector2(0f, 2.0f);
                for (int i = 0; i < Height / 8.0f; i++)
                {
                    Vector2 vector2 = new Vector2(Left - 1.0f, Top + 4.0f + i * 8);
                    Vector2 point = vector2 + Vector2.UnitX;
                    if (Scene.CollideCheck<Solid>(vector2) && !Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector2 + vector, MathF.PI);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector2 - vector, MathF.PI);
                    }
                }
            }

            if (position.X >= start.X)
            {
                Vector2 vector3 = new Vector2(0f, 2.0f);
                for (int i = 0; i < Height / 8.0f; i++)
                {
                    Vector2 vector4 = new Vector2(Right + 1.0f, Top + 4.0f + i * 8);
                    Vector2 point2 = vector4 - Vector2.UnitX * 2f;
                    if (Scene.CollideCheck<Solid>(vector4) && !Scene.CollideCheck<Solid>(point2))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector4 + vector3, 0f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector4 - vector3, 0f);
                    }
                }
            }

            if (position.Y <= start.Y)
            {
                Vector2 vector5 = new Vector2(2.0f, 0f);
                for (int i = 0; i < Width / 8.0f; i++)
                {
                    Vector2 vector6 = new Vector2(Left + 4.0f + i * 8, Top - 1.0f);
                    Vector2 point3 = vector6 + Vector2.UnitY;
                    if (Scene.CollideCheck<Solid>(vector6) && !Scene.CollideCheck<Solid>(point3))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector6 + vector5, -MathF.PI * 0.5f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector6 - vector5, -MathF.PI * 0.5f);
                    }
                }
            }

            if (position.Y >= start.Y)
            {
                Vector2 vector7 = new Vector2(2.0f, 0f);
                for (int i = 0; i < Width / 8.0f; i++)
                {
                    Vector2 vector8 = new Vector2(Left + 4f + (float)(i * 8), Bottom + 1.0f);
                    Vector2 point4 = vector8 - Vector2.UnitY * 2.0f;
                    if (Scene.CollideCheck<Solid>(vector8) && !Scene.CollideCheck<Solid>(point4))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector8 + vector7, MathF.PI * 0.5f);
                        SceneAs<Level>().ParticlesFG.Emit(P_Dust, vector8 - vector7, MathF.PI * 0.5f);
                    }
                }
            }

            Collidable = collidable;
            Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
            StartShaking(0.2f);
            while (_icon.Rate > 0.0f)
            {
                _icon.Rate -= Engine.DeltaTime * 4.0f;
                yield return null;
            }

            _icon.Rate = 0.0f;
            _icon.SetAnimationFrame(0);
            _wiggler.Start();
            collidable = Collidable;
            Collidable = false;
            if (!Scene.CollideCheck<Solid>(Center))
            {
                for (int m = 0; m < 32; m++)
                {
                    float num = Calc.Random.NextFloat(MathF.PI * 2f);
                    SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + _iconOffset + Calc.AngleToVector(num, 4f), num);
                }
            }

            Collidable = collidable;
        }

        private bool CheckButtons()
        {
            bool ret = true;
            foreach (TrapButton button in RelatedButtons)
            {
                if (!button.Pressed)
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        private MTexture[,] _nineSlice;
        private Sprite _icon;
        private Vector2 _iconOffset;
        private Wiggler _wiggler;
        private Vector2 _openPosition;
        private Vector2 _closePosition;
        private SoundSource _openSfx;
        private bool _on;
        private Coroutine _anim;
        private Tween _tween;

        private static ParticleType P_Behind;
        private static ParticleType P_Dust;
    }
}
