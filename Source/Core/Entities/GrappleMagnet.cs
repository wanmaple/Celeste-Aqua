using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Magnet")]
    public class GrappleMagnet : GrappleAttractor
    {
        public class SpriteMover
        {
            public Sprite sprite;
            public float lifetime;
            public float fadeInTime;
            public float fadeOutTime;
            public Vector2 startPosition;
            public Vector2 endPosition;
            public Vector2 moveDirection;

            public bool Expired => _elapsed >= lifetime;

            public void SetupImage(Entity owner, float rotation, Vector2 offset, Color color, float rate)
            {
                if (sprite == null)
                {
                    sprite = new Sprite();
                    GFX.SpriteBank.CreateOn(sprite, "Aqua_GrappleMagnetParticle");
                }
                sprite.Play("idle", true);
                sprite.Rate = rate;
                owner.Add(sprite);
                sprite.Position = offset;
                startPosition = offset;
                sprite.Rotation = rotation;
                sprite.Color = color;
                moveDirection = Calc.SafeNormalize(endPosition - startPosition);
                _elapsed = 0.0f;
            }

            public void Update(float dt)
            {
                _elapsed += dt;
                float t = MathHelper.Clamp(_elapsed / lifetime, 0.0f, 1.0f);
                sprite.Position = AquaMaths.Lerp(startPosition, endPosition, t);
            }

            private float _elapsed = 0.0f;
        }

        public int RadiusInTiles { get; private set; }
        public float AttractRadius { get; private set; }
        public bool DefaultOn { get; private set; }
        public bool UseDefaultSprite { get; private set; }
        public string Skin { get; private set; }

        public Vector2 ExactPosition => Position + _movementCounter;
        public override Vector2 AttractionTarget => Center;
        public override float MinRange => 0.25f * AttractRadius;
        public override float MaxRange => AttractRadius;

        public GrappleMagnet(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            RadiusInTiles = AquaMaths.Clamp(data.Int("radius_in_tiles", 4), 2, 8);
            AttractRadius = RadiusInTiles * 8.0f;
            DefaultOn = data.Bool("on", true);
            Skin = data.Attr("sprite", "Aqua_GrappleMagnet");
            UseDefaultSprite = data.Bool("use_default_sprite", true);
            Collider = new Circle(AttractRadius);
            _sound.Position = AttractionTarget;
            _particle1 = new ParticleType(SwapBlock.P_Move)
            {
                Color = Calc.HexToColor("2959dd"),
                Color2 = Calc.HexToColor("517fff"),
            };
            _particle2 = new ParticleType(SwapBlock.P_Move)
            {
                Color = Calc.HexToColor("ff3b3b"),
                Color2 = Calc.HexToColor("dd1e1e"),
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(_spriteMagnet = new Sprite());
            Add(_spriteFlash = new Sprite());
            string spriteName = Skin;
            if (UseDefaultSprite || !GFX.SpriteBank.Has(spriteName))
            {
                spriteName = "Aqua_GrappleMagnet";
            }
            GFX.SpriteBank.CreateOn(_spriteMagnet, spriteName);
            GFX.SpriteBank.CreateOn(_spriteFlash, spriteName);
            this.SetAttachCallbacks(OnGrappleAttached, OnGrappleDetached);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _spriteMagnet.Play("idle");
            _spriteFlash.Play("flash");
            SetActivated(DefaultOn);
        }

        public override void Update()
        {
            DynamicData.For(this).Set("prev_position", Position);
            base.Update();
            if (_activated)
                UpdateImageMovers();
            _spriteFlash.Visible = _spriteMagnet.CurrentAnimationID == "idle";
            if (Scene.OnInterval(2.0f) && _spriteFlash.Visible)
            {
                _spriteFlash.Play("flash");
            }
        }

        private void RandomGenerateMovers(int radiusInTiles)
        {
            int parts = radiusInTiles <= 4 ? 16 : 32;
            float possibility = radiusInTiles <= 4 ? radiusInTiles / 4.0f * 0.25f : (radiusInTiles - 4) / 4.0f * 0.125f + 0.125f;
            for (int j = 0; j < parts; ++j)
            {
                if (Calc.Random.NextFloat(1.0f) >= possibility)
                    continue;
                float angle = (float)j / parts * MathF.PI * 2.0f;
                float radius = radiusInTiles * 8.0f;
                Vector2 angleVec = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                Vector2 offset = angleVec * radius;
                float lifetime = 0.25f * radiusInTiles;
                var mover = _cycledMovers.Count > 0 ? _cycledMovers.Pop() : new SpriteMover();
                mover.lifetime = lifetime;
                mover.fadeInTime = lifetime * 0.4f;
                mover.fadeOutTime = lifetime * 0.6f;
                mover.endPosition = angleVec * 8.0f;
                mover.moveDirection = -angleVec;
                Color color = Color.White;
                float rd = Calc.Random.NextFloat(1.0f);
                if (rd < 0.5f)
                    color = Color.Lerp(Calc.HexToColor("2959dd"), Calc.HexToColor("517fff"), Calc.Random.NextFloat(1.0f));
                else
                    color = Color.Lerp(Calc.HexToColor("ff3b3b"), Calc.HexToColor("dd1e1e"), Calc.Random.NextFloat(1.0f));
                _particleMovers.Add(mover);
                mover.SetupImage(this, angle, offset, color, 1.0f / radiusInTiles);
            }
        }

        private void UpdateImageMovers()
        {
            float dt = Engine.DeltaTime;
            for (int i = 0; i < _particleMovers.Count;)
            {
                var mover = _particleMovers[i];
                mover.Update(dt);
                if (mover.Expired)
                {
                    Remove(mover.sprite);
                    _cycledMovers.Push(mover);
                    _particleMovers.RemoveAtFast(i);
                }
                else
                    ++i;
            }
            if (Scene.OnInterval(0.25f))
            {
                RandomGenerateMovers(RadiusInTiles);
            }
        }

        protected override void OnActivated()
        {
            foreach (var mover in _particleMovers)
            {
                mover.sprite.Visible = true;
            }
            _spriteMagnet.Play("open");
        }

        protected override void OnDeactivated()
        {
            foreach (var mover in _particleMovers)
            {
                mover.sprite.Visible = false;
            }
            _spriteMagnet.Play("deactive");
        }

        private void OnGrappleAttached(GrapplingHook grapple)
        {
            grapple.SetHookVisible(false);
            _spriteMagnet.Play("close");
            _sound.Play("event:/game/09_core/switch_to_hot");
        }

        private void OnGrappleDetached(GrapplingHook grapple)
        {
            grapple.SetHookVisible(true);
            if (_activated)
            {
                _spriteMagnet.Play("open");
            }
            _sound.Play("event:/game/09_core/switch_to_cold");
        }

        public void NaiveMove(Vector2 movement)
        {
            Vector2 oldPos = Position;
            NaiveMoveH(movement.X);
            NaiveMoveV(movement.Y);
            _spriteMagnet.RenderPosition = _spriteFlash.RenderPosition = Position;
            Vector2 exactMovement = Position - oldPos;
            foreach (var mover in _particleMovers)
            {
                mover.sprite.RenderPosition += exactMovement;
            }
        }

        private void NaiveMoveH(float moveH)
        {
            _movementCounter.X += moveH;
            int num = (int)MathF.Round(_movementCounter.X);
            if (num != 0)
            {
                _movementCounter.X -= num;
                Position.X += num;
            }
        }

        private void NaiveMoveV(float moveV)
        {
            _movementCounter.Y += moveV;
            int num = (int)Math.Round(_movementCounter.Y);
            if (num != 0)
            {
                _movementCounter.Y -= num;
                Position.Y += num;
            }
        }

        public void MoveH(float moveH, Func<Vector2, bool> collisionCheck)
        {
            _movementCounter.X += moveH;
            int num = (int)MathF.Round(_movementCounter.X);
            if (num != 0)
            {
                int step = MathF.Sign(num);
                for (int i = 0; i < Math.Abs(num); i++)
                {
                    if (collisionCheck.Invoke(Vector2.UnitX * step))
                        break;
                    _movementCounter.X -= step;
                    Position.X += step;
                }
            }
        }

        public void MoveV(float moveV, Func<Vector2, bool> collisionCheck)
        {
            _movementCounter.Y += moveV;
            int num = (int)MathF.Round(_movementCounter.Y);
            if (num != 0)
            {
                int step = MathF.Sign(num);
                for (int i = 0; i < Math.Abs(num); i++)
                {
                    if (collisionCheck.Invoke(Vector2.UnitY * step))
                        break;
                    _movementCounter.Y -= step;
                    Position.Y += step;
                }
            }
        }

        protected void EmitParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 positionRange;
            float direction;
            float num;
            float rangeOffset = 6.0f;
            float density = 14.0f;
            float width = 24.0f;
            float height = 24.0f;
            if (normal.X > 0f)
            {
                position = Center;
                positionRange = Vector2.UnitY * (height - rangeOffset);
                direction = MathF.PI;
                num = Math.Max(2.0f, height / density);
            }
            else if (normal.X < 0f)
            {
                position = Center;
                positionRange = Vector2.UnitY * (height - rangeOffset);
                direction = 0.0f;
                num = Math.Max(2.0f, height / density);
            }
            else if (normal.Y > 0f)
            {
                position = Center;
                positionRange = Vector2.UnitX * (width - rangeOffset);
                direction = -MathF.PI * 0.5f;
                num = Math.Max(2.0f, width / density);
            }
            else
            {
                position = Center;
                positionRange = Vector2.UnitX * (width - rangeOffset);
                direction = MathF.PI * 0.5f;
                num = Math.Max(2.0f, width / density);
            }

            _particlesRemainder += num;
            int num2 = (int)_particlesRemainder;
            _particlesRemainder -= num2;
            positionRange *= 0.5f;
            SceneAs<Level>().Particles.Emit(_particle1, num2 / 2, position, positionRange, direction);
            SceneAs<Level>().Particles.Emit(_particle2, num2 / 2, position, positionRange, direction);
        }

        protected Sprite _spriteMagnet;
        protected Sprite _spriteFlash;
        protected SoundSource _sound = new SoundSource();
        protected Vector2 _movementCounter = Vector2.Zero;
        protected List<SpriteMover> _particleMovers = new List<SpriteMover>(128);
        private Stack<SpriteMover> _cycledMovers = new Stack<SpriteMover>();
        private ParticleType _particle1;
        private ParticleType _particle2;
        private float _particlesRemainder;
    }
}
