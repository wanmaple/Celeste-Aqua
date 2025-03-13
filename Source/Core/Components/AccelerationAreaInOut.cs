using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class AccelerationAreaInOut : Component
    {
        public Collider SpecificCollider { get; set; }

        public AccelerationAreaInOut(Action<AccelerationArea> keepIn, Action<AccelerationArea> areaIn, Action<AccelerationArea> areaOut, Collider specificCollider = null)
            : base(true, false)
        {
            _keepIn = keepIn;
            _areaIn = areaIn;
            _areaOut = areaOut;
            SpecificCollider = specificCollider;
        }

        public override void Update()
        {
            List<Entity> areas = Scene.Tracker.GetEntities<AccelerationArea>();
            foreach (AccelerationArea area in areas)
            {
                Collider old = Entity.Collider;
                if (SpecificCollider != null)
                    Entity.Collider = SpecificCollider;
                bool collided = area.CollideCheck(Entity);
                Entity.Collider = old;
                if (collided)
                {
                    if (area.EnterEntity(Entity))
                        _areaIn?.Invoke(area);
                    _keepIn?.Invoke(area);
                }
                else if (!collided && area.ExitEntity(Entity))
                {
                    _areaOut?.Invoke(area);
                }
            }
        }

        private Action<AccelerationArea> _keepIn;
        private Action<AccelerationArea> _areaIn;
        private Action<AccelerationArea> _areaOut;
    }
}
