using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Booster Transporter")]
    [Tracked(false)]
    public class BoosterTransporter : Entity
    {
        public Vector2 TargetPosition { get; private set; }
        public float AbsorbDuration { get; private set; }
        public float ReleaseDuration { get; private set; }
        public float TransportDuration { get; private set; }
        public bool ChangeRespawnPosition { get; private set; }
        public string InBaseTexture { get; private set; }
        public string OutBaseTexture { get; private set; }
        public string Sprite { get; private set; }
        public Color Color { get; private set; }
        public Color ParticleColor { get; private set; }
        public bool UseDefaultSprite { get; private set; }

        public bool IsBusy => _busy;

        public BoosterTransporter(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Circle(10.0f);
            TargetPosition = data.Nodes[0] + offset;
            AbsorbDuration = MathF.Max(data.Float("absorb_duration", 0.2f), 0.0f);
            ReleaseDuration = MathF.Max(data.Float("release_duration", 0.2f), 0.0f);
            TransportDuration = MathF.Max(data.Float("transport_duration", 0.6f), 0.0f);
            ChangeRespawnPosition = data.Bool("change_respawn", false);
            InBaseTexture = data.Attr("in_base_texture", "objects/booster_transporter/in_base00");
            OutBaseTexture = data.Attr("out_base_texture", "objects/booster_transporter/out_base");
            Sprite = data.Attr("sprite", "Aqua_BoosterTransporterCustom");
            Color = data.HexColor("color", Color.White);
            ParticleColor = data.HexColor("particle_color", Color.Gray);
            UseDefaultSprite = data.Bool("use_default_sprite", true);
            Add(new Coroutine(TransportCoroutine()));
            Add(new VertexLight(Color.White, 1.0f, 16, 32));
            Add(new BloomPoint(0.1f, 16.0f));
            _particle = new ParticleType(Booster.P_Appear) { Color = ParticleColor, };
            Depth = Depths.SolidsBelow;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string inBaseTexture = "objects/booster_transporter/in_base00";
            string outBaseTexture = "objects/booster_transporter/out_base";
            string sprite = "Aqua_BoosterTransporterCustom";
            if (!UseDefaultSprite)
            {
                inBaseTexture = InBaseTexture;
                outBaseTexture = OutBaseTexture;
                sprite = Sprite;
            }
            Add(_inSprite = GFX.SpriteBank.Create(sprite));
            _inSprite.SetColor(Color);
            _inSprite.Play("in_idle");
            _inSprite.RenderPosition = Position;
            Add(_outSprite = GFX.SpriteBank.Create(sprite));
            _outSprite.SetColor(Color);
            _outSprite.Play("out_idle");
            _outSprite.RenderPosition = TargetPosition;
            var inBase = new Image(GFX.Game[inBaseTexture]);
            Add(inBase);
            inBase.JustifyOrigin(0.5f, 0.5f);
            inBase.RenderPosition = Position;
            _outBaseTextures = GFX.Game.GetAtlasSubtextures(outBaseTexture);
            Add(_outBase = new Image(_outBaseTextures[0]));
            _outBase.JustifyOrigin(0.5f, 0.5f);
            _outBase.RenderPosition = TargetPosition;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _soundTravel.Stop();
        }

        public override void Update()
        {
            base.Update();
            if (_transporting && !_soundTravel.Playing)
                _soundTravel.Play("event:/char/madeline/dreamblock_travel");
            else if (!_transporting && _soundTravel.Playing)
                _soundTravel.Stop();
        }

        public void BeginTransport(Booster booster)
        {
            _transportingBooster = booster;
            MoveToward com = booster.Get<MoveToward>();
            if (com != null)
            {
                com.Target = null;
                com.Active = false;
            }
            booster.Collidable = false;
            if (booster is AquaBooster myBooster)
            {
                myBooster.OnTransport();
            }
            booster.cannotUseTimer = AbsorbDuration + ReleaseDuration + TransportDuration;
            _busy = true;
        }

        public void EndTransport()
        {
            _transportingBooster.Collidable = true;
            _transportingBooster.cannotUseTimer = 0.0f;
            _transportingBooster = null;
            _busy = false;
        }

        private IEnumerator TransportCoroutine()
        {
            while (true)
            {
                while (_transportingBooster == null)
                    yield return null;

                Vector2 from = _transportingBooster.Position;
                Vector2 to = Position;
                Audio.Play("event:/char/madeline/dreamblock_enter", Position);
                if (AbsorbDuration > 0.0f)
                {
                    DisplacementRenderer.Burst burst = (Scene as Level).Displacement.AddBurst(Center, AbsorbDuration, 0.0f, 10.0f);
                    burst.WorldClipCollider = Collider;
                    burst.WorldClipPadding = 2;
                    float elapsed = 0.0f;
                    while (elapsed < AbsorbDuration)
                    {
                        float t = Calc.Clamp(elapsed / AbsorbDuration, 0.0f, 1.0f);
                        _transportingBooster.Position = Vector2.Lerp(from, to, t);
                        _transportingBooster.sprite.Scale = Vector2.Lerp(Vector2.One, Vector2.Zero, t);
                        elapsed += Engine.DeltaTime;
                        yield return null;
                    }
                }
                _transportingBooster.sprite.Scale = Vector2.Zero;
                _transportingBooster.Position = TargetPosition;
                ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(_particle, 1, Position, Vector2.One * 1.5f, i * (MathF.PI / 180.0f));
                }
                if (TransportDuration > 0.0f)
                {
                    int steps = _outBaseTextures.Count - 1;
                    if (steps > 0)
                    {
                        _transporting = true;
                        float elapsed = 0.0f;
                        while (elapsed < TransportDuration)
                        {
                            float t = Calc.Clamp(elapsed / TransportDuration, 0.0f, 1.0f);
                            _soundTravel.Position = Vector2.Lerp(Position, TargetPosition, t);
                            int index = (int)MathF.Floor(t * steps);
                            if (index > 0)
                            {
                                _outBase.Texture = _outBaseTextures[index];
                            }
                            elapsed += Engine.DeltaTime;
                            yield return null;
                        }
                        _transporting = false;
                    }
                    else
                    {
                        yield return TransportDuration;
                    }
                }
                if (ReleaseDuration > 0.0f)
                {
                    float elapsed = 0.0f;
                    while (elapsed < ReleaseDuration)
                    {
                        float t = Calc.Clamp(elapsed / ReleaseDuration, 0.0f, 1.0f);
                        _transportingBooster.sprite.Scale = Vector2.Lerp(Vector2.Zero, Vector2.One, t);
                        elapsed += Engine.DeltaTime;
                        yield return null;
                    }
                }
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(_particle, 1, TargetPosition, Vector2.One * 1.5f, i * (MathF.PI / 180.0f));
                }
                Audio.Play("event:/game/general/assist_nonsolid_out", TargetPosition);
                _outBase.Texture = _outBaseTextures[0];
                if (ChangeRespawnPosition)
                {
                    _transportingBooster.outline.Position = TargetPosition;
                }
                EndTransport();
                yield return null;
            }
        }

        private bool _busy;
        private Booster _transportingBooster;
        private Image _outBase;
        private List<MTexture> _outBaseTextures;
        private Sprite _inSprite;
        private Sprite _outSprite;
        private SoundSource _soundTravel = new SoundSource();
        private bool _transporting;
        private ParticleType _particle;
    }
}
