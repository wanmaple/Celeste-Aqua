using System;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class MoveInputTimer
    {
        public bool MoveXPressed(int dir, float time)
        {
            return MathF.Sign(_lastX) == dir && _timerX >= time;
        }

        public bool MoveYPressed(int dir, float time)
        {
            return MathF.Sign(_lastY) == dir && _timerY >= time;
        }

        public void Update(float dt)
        {
            if (Input.MoveX != _lastX)
            {
                _lastX = Input.MoveX;
                _timerX = 0.0f;
            }
            else
            {
                _timerX += dt;
            }

            if (Input.MoveY != _lastY)
            {
                _lastY = Input.MoveY;
                _timerY = 0.0f;
            }
            else
            {
                _timerY += dt;
            }
        }

        public void Reset()
        {
            _lastX = _lastY = 0;
            _timerX = _timerY = 0.0f;
        }

        private int _lastX;
        private int _lastY;
        private float _timerX;
        private float _timerY;
    }
}
