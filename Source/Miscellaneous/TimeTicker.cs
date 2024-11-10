namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class TimeTicker
    {
        public TimeTicker(float time)
        {
            _time = _ticker = time;
        }

        public bool Check()
        {
            return _ticker >= 0.0f;
        }

        public void Reset()
        {
            _ticker = _time;
        }

        public void Tick(float dt)
        {
            _ticker -= dt;
        }

        private float _time;
        private float _ticker;
    }
}
