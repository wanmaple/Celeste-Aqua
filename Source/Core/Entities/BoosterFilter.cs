using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Booster Filter")]
    [Tracked(false)]
    public class BoosterFilter : Solid, IBarrierRenderable
    {
        Vector2 IBarrierRenderable.Position => this.Position;
        public Color Color { get; private set; }
        public Color ParticleColor { get; private set; }
        public bool CanPassGreenBooster { get; private set; }
        public bool CanPassRedBooster { get; private set; }
        public float Flash { get; private set; } = 0.0f;
        public float Solidify { get; private set; } = 0.0f;
        bool IBarrierRenderable.Visible => this.Visible;

        public BoosterFilter(EntityData data, Vector2 offset)
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
            switch (data.Attr("can_pass", "Both"))
            {
                case "Green":
                    CanPassGreenBooster = true;
                    break;
                case "Red":
                    CanPassRedBooster = true;
                    break;
                case "Both":
                default:
                    CanPassGreenBooster = CanPassRedBooster = true;
                    break;
            }
            this.MakeExtraCollideCondition();
            this.SetHookable(false);
            for (int i = 0; (float)i < Width * Height / 16f; i++)
            {
                _particlePositions.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            }

            Add(new PlayerExactCollider(OnPlayerCollide));
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
                Vector2 value = _particlePositions[i] + Vector2.UnitY * SPEEDS[i % num] * Engine.DeltaTime;
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

        private bool CanCollide(Entity other)
        {
            if (other is Player player)
            {
                if (CanPassGreenBooster && player.IsBoosterDash())
                    return false;
                if (CanPassRedBooster && player.StateMachine.State == (int)AquaStates.StRedDash)
                    return false;
                return true;
            }
            else if (other is Platform)
            {
                return false;
            }
            return true;
        }

        private void OnPlayerCollide(Player player)
        {
            player.Die(Vector2.Zero);
        }

        private List<Vector2> _particlePositions = new List<Vector2>(16);

        public static readonly float[] SPEEDS = new float[3] { 12f, 20f, 40f };
    }
}
