using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public abstract class Filter : Solid, IBarrierRenderable
    {
        public Color Color { get; protected set; }
        public float Flash { get; protected set; }
        public float Solidify { get; protected set; }
        Vector2 IBarrierRenderable.Position => Position;
        bool IBarrierRenderable.Visible => Visible;
        public Color ParticleColor { get; protected set; }
        public bool CollideSolids { get; protected set; }
        public int LandSoundIndex { get; protected set; }

        protected Filter(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            Color mainColor = data.HexColor("color", new Color(0.15f, 0.15f, 0.15f));
            float opacity = data.Float("opacity", 0.15f);
            mainColor.A = (byte)(opacity * 255);
            Color = mainColor;
            Color particleColor = data.HexColor("particle_color", new Color(0.5f, 0.5f, 0.5f));
            float particleOpacity = data.Float("particle_opacity", 0.5f);
            particleColor.A = (byte)(particleOpacity * 255);
            ParticleColor = particleColor;
            CollideSolids = data.Bool("collide_solids", false);
            LandSoundIndex = data.Int("land_sound_index", 11);
            this.MakeExtraCollideCondition();
            this.SetHookable(false);
            for (int i = 0; (float)i < Width * Height / 16f; i++)
            {
                _particlePositions.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            }
            Add(new PlayerExactCollider(OnPlayerCollide));
        }

        protected virtual bool CanCollide(Entity other)
        {
            if (other is Player)
                return true;
            if (other is Platform)
                return CollideSolids;
            return true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Tracker.GetEntity<BarrierRenderer>().Track(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Tracker.GetEntity<BarrierRenderer>().Untrack(this);
        }

        public override void Update()
        {
            int num = SPEEDS.Length;
            float height = Height;
            int i = 0;
            for (int count = _particlePositions.Count; i < count; i++)
            {
                Vector2 value = _particlePositions[i] + Vector2.UnitY * SPEEDS[i % num] * Engine.DeltaTime * (1.0f - Solidify);
                value.Y %= height - 1f;
                _particlePositions[i] = value;
            }

            base.Update();
        }

        public override void Render()
        {
            foreach (Vector2 particle in _particlePositions)
            {
                Draw.Pixel.Draw(Position + particle, Vector2.Zero, ParticleColor);
            }
        }

        public override int GetLandSoundIndex(Entity entity)
        {
            return LandSoundIndex;
        }

        protected void OnPlayerCollide(Player player)
        {
            player.Die(Vector2.Zero);
        }

        protected List<Vector2> _particlePositions = new List<Vector2>(16);
        protected static readonly float[] SPEEDS = new float[3] { 12f, 20f, 40f };
    }
}
