using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class AccelerationAreaInOut : Component
    {
        public AccelerationAreaInOut(Action<AccelerationArea> keepIn, Action<AccelerationArea> areaIn, Action<AccelerationArea> areaOut)
            : base(true, false)
        {
            _keepIn = keepIn;
            _areaIn = areaIn;
            _areaOut = areaOut;
        }

        public override void Update()
        {
            List<Entity> areas = Scene.Tracker.GetEntities<AccelerationArea>();
            foreach (AccelerationArea area in areas)
            {
                if (area.CollideCheck(Entity))
                {
                    if (area.EnterEntity(Entity))
                        _areaIn?.Invoke(area);
                    _keepIn?.Invoke(area);
                }
                else if (!area.CollideCheck(Entity) && area.ExitEntity(Entity))
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
