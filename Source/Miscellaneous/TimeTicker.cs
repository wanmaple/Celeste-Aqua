namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class TimeTicker
    {
        public float Duration
        {
            get => _time;
            set => _time = value;
        }

        public TimeTicker(float time)
        {
            _time = _ticker = time;
        }

        public bool Check()
        {
            return _ticker <= 0.0f && !_expired;
        }

        public bool CheckRate(float rate)
        {
            return _ticker <= _time * (1.0f - rate);
        }

        public void Reset()
        {
            _ticker = _time;
            _expired = false;
        }

        public void Tick(float dt)
        {
            _ticker -= dt;
        }

        public void Expire()
        {
            _expired = true;
        }

        private float _time;
        private float _ticker;
        private bool _expired;
    }
}
