namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class Scheduler
    {
        public float Interval { get; set; }
        public bool OnInterval
        {
            get
            {
                if (_elapsed > Interval)
                {
                    _elapsed -= Interval;
                    return true;
                }
                return false;
            }
        }

        public Scheduler(float interval)
        {
            Interval = interval;
        }

        public void Update(float dt)
        {
            _elapsed += dt;
        }

        public void Reset()
        {
            _elapsed = 0.0f;
        }

        private float _elapsed;
    }
}
