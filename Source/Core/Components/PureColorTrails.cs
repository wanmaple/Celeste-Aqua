using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class PureColorTrails : Component
    {
        public PureColorTrails(Predicate<Entity> condition, Func<Entity, Color> colorFunc, Vector2 origin)
            : base(true, false)
        {
            _condition = condition;
            _colorFunc = colorFunc;
            _origin = origin;
        }

        public override void Update()
        {
            float dt = Engine.DeltaTime;
            if (_condition == null || _condition.Invoke(Entity))
            {
                Color startColor = _colorFunc == null ? Color.White : _colorFunc(Entity);
                var trail = new Trail(Entity.Position, _origin, "SolidRectTrail", "trail", startColor, new Vector2(Entity.Width / 8.0f, Entity.Height / 8.0f));
                Scene.Add(trail);
            }
        }

        private Predicate<Entity> _condition;
        private Func<Entity, Color> _colorFunc;
        private Vector2 _origin;
    }
}
