using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class GrappleInertia
    {
        private struct GrappleAcceleration
        {
            public float acceleration;
            public int last;
        }

        public Vector2 Inertia
        {
            get
            {
                float attenuationX = Calc.Clamp(1.0f - (float)_inertiaX.last / _maxFrames, 0.0f, 1.0f);
                attenuationX = MathF.Pow(attenuationX, 2.0f);
                float attenuationY = Calc.Clamp(1.0f - (float)_inertiaY.last / _maxFrames, 0.0f, 1.0f);
                attenuationY = MathF.Pow(attenuationY, 2.0f);
                return new Vector2(_inertiaX.acceleration * attenuationX, _inertiaY.acceleration * attenuationY);
            }
        }

        public GrappleInertia(int maxFrames)
        {
            _maxFrames = maxFrames;
        }

        public void SetX(float acceleration)
        {
            _inertiaX.acceleration = acceleration;
            _inertiaX.last = 0;
        }

        public void SetY(float acceleration)
        {
            _inertiaY.acceleration = acceleration;
            _inertiaY.last = 0;
        }

        public void Clear()
        {
            _inertiaX.acceleration = _inertiaY.acceleration = 0.0f;
            _inertiaX.last = _maxFrames;
            _inertiaY.last = _maxFrames;
        }

        public void Update()
        {
            _inertiaX.last++;
            _inertiaY.last++;
        }

        private int _maxFrames;
        private GrappleAcceleration _inertiaX;
        private GrappleAcceleration _inertiaY;
    }
}
