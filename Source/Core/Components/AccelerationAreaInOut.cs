using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public class AccelerationAreaInOut : Component
    {
        public AccelerationAreaInOut(Action<AccelerationArea> areaIn, Action<AccelerationArea> areaOut, Action<AccelerationArea> keepIn)
            : base(true, false)
        {
            _areaIn = areaIn;
            _areaOut = areaOut;
            _keepIn = keepIn;
        }

        public override void Update()
        {
            List<Entity> areas = Scene.Tracker.GetEntities<AccelerationArea>();
            foreach (AccelerationArea area in areas)
            {
                if (area.CollideCheck(Entity))
                {
                    if (_lastIn.Add(area))
                        _areaIn?.Invoke(area);
                    else
                        _keepIn?.Invoke(area);
                }
                else if (!area.CollideCheck(Entity) && _lastIn.Remove(area))
                {
                    _areaOut?.Invoke(area);
                }
            }
        }

        private HashSet<AccelerationArea> _lastIn = new HashSet<AccelerationArea>(4);
        private Action<AccelerationArea> _areaIn;
        private Action<AccelerationArea> _areaOut;
        private Action<AccelerationArea> _keepIn;
    }
}
