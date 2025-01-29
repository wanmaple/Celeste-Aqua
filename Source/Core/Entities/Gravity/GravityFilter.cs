using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Gravity Filter")]
    public class GravityFilter : Filter
    {
        public bool EnableOnGravityInverted { get; private set; }
        public float ActiveOpacity { get; private set; }
        public float SolidOpacity { get; private set; }

        public GravityFilter(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            ActiveOpacity = data.Float("active_opacity", 0.15f);
            SolidOpacity = data.Float("solidify_opacity", 0.8f);
            switch (data.Attr("gravity", "Normal"))
            {
                case "Inverted":
                    EnableOnGravityInverted = true;
                    break;
                default:
                    EnableOnGravityInverted = false;
                    break;
            }
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

            base.Update();
        }

        private void SetEnabled(bool enabled)
        {
            if (enabled != _enabled)
            {
                _enabled = enabled;
                Collidable = _enabled;
                if (_enabled)
                {
                    EnableStaticMovers();
                }
                else
                {
                    DisableStaticMovers();
                }
                this.SetHookable(_enabled);
            }
        }

        protected override bool CanCollide(Entity other)
        {
            if (other is Platform)
            {
                return CollideSolids;
            }
            return true;
        }

        private bool _enabled = true;
    }
}
