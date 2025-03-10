using Monocle;
using System.Runtime.CompilerServices;
using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [Pooled]
    public class CustomDebris : Actor
    {
        public CustomDebris()
            : base(Vector2.Zero)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(4f, 4f, -2f, -2f);
            _onCollideH = [MethodImpl(MethodImplOptions.NoInlining)] (CollisionData c) =>
            {
                _speed.X = (0f - _speed.X) * 0.5f;
            };
            _onCollideV = [MethodImpl(MethodImplOptions.NoInlining)] (CollisionData c) =>
            {
                if (_firstHit || _speed.Y > 50f)
                {
                    Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(_speed.Y, 0f, 600f));
                }
                if (_speed.Y > 0f && _speed.Y < 40f)
                {
                    _speed.Y = 0f;
                }
                else
                {
                    _speed.Y = (0f - _speed.Y) * 0.25f;
                }
                _firstHit = false;
            };
        }

        public override void OnSquish(CollisionData data)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public CustomDebris Init(string texture, Vector2 position, Vector2 center, Vector2 returnTo, int specificIndex = -1)
        {
            Collidable = true;
            Position = position;
            var textures = GFX.Game.GetAtlasSubtextures(texture);
            MTexture chooseTexture = null;
            if (specificIndex < 0)
                chooseTexture = Calc.Random.Choose(textures);
            else
                chooseTexture = textures[specificIndex % textures.Count];
            if (_sprite == null)
                Add(_sprite = new Image(chooseTexture));
            else
                _sprite.Texture = chooseTexture;
            _sprite.CenterOrigin();
            _sprite.FlipX = Calc.Random.Chance(0.5f);
            _speed = (position - center).SafeNormalize(60.0f + Calc.Random.NextFloat(60.0f));
            _home = returnTo;
            _sprite.Position = Vector2.Zero;
            _sprite.Rotation = Calc.Random.NextAngle();
            _returning = false;
            _shaking = false;
            _sprite.Scale.X = 1.0f;
            _sprite.Scale.Y = 1.0f;
            _sprite.Color = Color.White;
            _alpha = 1f;
            _firstHit = false;
            _spin = Calc.Random.Range(3.49065852f, 10.4719753f) * (float)Calc.Random.Choose(1, -1);
            return this;
        }

        public override void Update()
        {
            base.Update();
            if (!_returning)
            {
                if (Collidable)
                {
                    _speed.X = Calc.Approach(_speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        _speed.Y += 400f * Engine.DeltaTime;
                    }

                    MoveH(_speed.X * Engine.DeltaTime, _onCollideH);
                    MoveV(_speed.Y * Engine.DeltaTime, _onCollideV);
                }

                if (_shaking && base.Scene.OnInterval(0.05f))
                {
                    _sprite.X = -1 + Calc.Random.Next(3);
                    _sprite.Y = -1 + Calc.Random.Next(3);
                }
            }
            else
            {
                Position = _returnCurve.GetPoint(Ease.CubeOut(_returnEase));
                _returnEase = Calc.Approach(_returnEase, 1f, Engine.DeltaTime / _returnDuration);
                _sprite.Scale = Vector2.One * (1f + _returnEase * 0.5f);
            }

            if ((Scene as Level).Transitioning)
            {
                _alpha = Calc.Approach(_alpha, 0f, Engine.DeltaTime * 4f);
                _sprite.Color = Color.White * _alpha;
            }

            _sprite.Rotation += _spin * Calc.ClampedMap(Math.Abs(_speed.Y), 50f, 150f) * Engine.DeltaTime;
        }

        public void StopMoving()
        {
            Collidable = false;
        }

        public void StartShaking()
        {
            _shaking = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ReturnHome(float duration)
        {
            if (Scene != null)
            {
                Camera camera = (Scene as Level).Camera;
                if (X < camera.X)
                {
                    X = camera.X - 8f;
                }
                if (Y < camera.Y)
                {
                    Y = camera.Y - 8f;
                }
                if (X > camera.X + 320f)
                {
                    X = camera.X + 320f + 8f;
                }
                if (Y > camera.Y + 180f)
                {
                    Y = camera.Y + 180f + 8f;
                }
            }
            _returning = true;
            _returnEase = 0f;
            _returnDuration = duration;
            Vector2 vector = (_home - Position).SafeNormalize();
            Vector2 control = (Position + _home) / 2f + new Vector2(vector.Y, 0f - vector.X) * (Calc.Random.NextFloat(16f) + 16f) * Calc.Random.Facing();
            _returnCurve = new SimpleCurve(Position, _home, control);
        }

        public Image _sprite;
        public Vector2 _home;
        public Vector2 _speed;
        public bool _shaking;
        public bool _returning;
        public float _returnEase;
        public float _returnDuration;
        public SimpleCurve _returnCurve;
        public bool _firstHit;
        public float _alpha;
        public Collision _onCollideH;
        public Collision _onCollideV;
        public float _spin;
    }
}
