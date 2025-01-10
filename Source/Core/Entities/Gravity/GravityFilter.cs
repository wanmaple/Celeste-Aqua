using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Gravity Filter")]
    public class GravityFilter : Solid, IBarrierRenderable
    {
        Vector2 IBarrierRenderable.Position => this.Position;
        public Color Color { get; private set; }
        public float Flash { get; private set; } = 0.0f;
        public float Solidify { get; private set; } = 0.0f;
        bool IBarrierRenderable.Visible => this.Visible;
        public Color ParticleColor { get; private set; }
        public bool EnableOnGravityInverted { get; private set; }
        public float ActiveOpacity { get; private set; }
        public float SolidOpacity { get; private set; }

        public GravityFilter(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, true)
        {
            Color mainColor = data.HexColor("color", new Color(0.15f, 0.15f, 0.15f));
            ActiveOpacity = data.Float("active_opacity", 0.15f);
            SolidOpacity = data.Float("solidify_opacity", 0.8f);
            mainColor.A = (byte)(ActiveOpacity * 255);
            Color = mainColor;
            Color particleColor = data.HexColor("particle_color", new Color(0.5f, 0.5f, 0.5f));
            float particleOpacity = data.Float("particle_opacity", 0.5f);
            particleColor.A = (byte)(particleOpacity * 255);
            ParticleColor = particleColor;
            switch (data.Attr("gravity", "Normal"))
            {
                case "Inverted":
                    EnableOnGravityInverted = true;
                    break;
                default:
                    EnableOnGravityInverted = false;
                    break;
            }
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

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bool currentGravityInverted = ModInterop.GravityHelper.IsPlayerGravityInverted();
            bool enabled = currentGravityInverted == EnableOnGravityInverted;
            SetEnabled(enabled);
        }

        public override void Update()
        {
            bool currentGravityInverted = ModInterop.GravityHelper.IsPlayerGravityInverted();
            bool enabled = currentGravityInverted == EnableOnGravityInverted;
            SetEnabled(enabled);
            float sign = _enabled ? 1.0f : -1.0f;
            Solidify = Calc.Clamp(Solidify + Engine.DeltaTime * 4.0f * sign, 0.0f, 1.0f);
            Color color = Color;
            float alpha = MathHelper.Lerp(ActiveOpacity, SolidOpacity, Solidify);
            color.A = (byte)(alpha * 255.0f);
            Color = color;
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
            return 11;
        }

        private void SetEnabled(bool enabled)
        {
            if (enabled != _enabled)
            {
                _enabled = enabled;
                Collidable = _enabled;
                this.SetHookable(_enabled);
            }
        }

        private void OnPlayerCollide(Player player)
        {
            player.Die(Vector2.Zero);
        }

        private List<Vector2> _particlePositions = new List<Vector2>(16);
        private bool _enabled = true;

        public static readonly float[] SPEEDS = new float[3] { 12f, 20f, 40f };
    }
}
