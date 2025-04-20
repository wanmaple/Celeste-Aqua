using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

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
            Add(new Coroutine(TransportCoroutine()));
            Add(new VertexLight(Color.White, 1.0f, 16, 32));
            Depth = Depths.SolidsBelow;
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
                if (AbsorbDuration > 0.0f)
                {
                    float elapsed = 0.0f;
                    while (elapsed < AbsorbDuration)
                    {
                        elapsed += Engine.DeltaTime;
                        float t = Calc.Clamp(elapsed / AbsorbDuration, 0.0f, 1.0f);
                        _transportingBooster.Position = Vector2.Lerp(from, to, t);
                        _transportingBooster.sprite.Scale = Vector2.Lerp(Vector2.One, Vector2.Zero, t);
                        yield return null;
                    }
                }
                _transportingBooster.sprite.Scale = Vector2.Zero;
                _transportingBooster.Position = TargetPosition;
                if (TransportDuration > 0.0f)
                    yield return TransportDuration;
                if (ReleaseDuration > 0.0f)
                {
                    float elapsed = 0.0f;
                    while (elapsed < ReleaseDuration)
                    {
                        elapsed += Engine.DeltaTime;
                        float t = Calc.Clamp(elapsed / ReleaseDuration, 0.0f, 1.0f);
                        _transportingBooster.sprite.Scale = Vector2.Lerp(Vector2.Zero, Vector2.One, t);
                        yield return null;
                    }
                }
                if (ChangeRespawnPosition)
                {
                    _transportingBooster.outline.Position = TargetPosition;
                }
                EndTransport();
                yield return null;
            }
        }

        public override void Render()
        {
            Draw.Circle(Position, 12.0f, Color.Red, 16);
            Draw.Circle(TargetPosition, 12.0f, Color.Yellow, 16);
        }

        private bool _busy;
        private Booster _transportingBooster;
    }
}
