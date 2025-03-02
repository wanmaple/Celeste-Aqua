using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Invisible Grapple Attractor")]
    public class InvisibleGrappleAttractor : GrappleAttractor
    {
        public override Vector2 AttractionTarget => Center;
        public override float MinRange => _minRange;
        public override float MaxRange => _maxRange;

        public InvisibleGrappleAttractor(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            SetupShape(data);
            bool attachToSolid = data.Bool("attach_to_solid", true);
            if (attachToSolid)
            {
                var staticMover = new StaticMover();
                staticMover.SolidChecker = solid => CollideCheck(solid);
                staticMover.OnEnable = OnEnable;
                staticMover.OnDisable = OnDisable;
                staticMover.OnDestroy = OnDestroy;
                Add(staticMover);
            }
        }

        private void SetupShape(EntityData data)
        {
            string shape = data.Attr("shape", "Rectangle");
            switch (shape)
            {
                case "Circle":
                    float radius = MathF.Max(data.Int("radius_in_tiles", 4), 1) * 8.0f;
                    Collider = new Circle(radius);
                    _minRange = 0.25f * radius;
                    _maxRange = radius;
                    break;
                case "Rectangle":
                default:
                    Collider = new Hitbox(data.Width, data.Height);
                    _maxRange = MathF.Sqrt(data.Width * data.Width + data.Height * data.Height);
                    _minRange = 0.25f * _maxRange;
                    break;
            }
        }

        private void OnEnable()
        {
            Collidable = true;
        }

        private void OnDisable()
        {
            Collidable = false;
        }

        private void OnDestroy()
        {
            Collidable = false;
        }

        private float _minRange;
        private float _maxRange;
    }
}
