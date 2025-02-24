using Celeste.Mod.Aqua.Debug;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class ShotHookCheck
    {
        public bool CanThrow
        {
            get
            {
                if (_mode == ShotHookModes.Default)
                    return _key.Pressed;
                return (_lastFirstNotPressFrame >= _lastRevokedFrame) && _key.Check;
            }
        }

        public bool CanRevoke
        {
            get
            {
                if (_mode == ShotHookModes.Default)
                    return _key.Pressed;
                return !_key.Check;
            }
        }

        public bool CanGrappleBoost
        {
            get
            {
                if (_mode == ShotHookModes.Default)
                    return _key.Pressed;
                return _key.Check && _lastFirstNotPressFrame > _lastFirstPressFrame;
            }
        }

        public ShotHookCheck(ButtonBinding key, ShotHookModes mode)
        {
            _key = key;
            _mode = mode;
            Reset();
        }

        public void EndPeriod()
        {
            _lastRevokedFrame = Engine.FrameCounter;
            if (!_key.Check)
                _lastFirstNotPressFrame = _lastRevokedFrame;
        }

        public void Reset()
        {
            _lastFirstPressFrame = 0;
            _lastFirstNotPressFrame = 0;
            _lastPressed = false;
            _lastRevokedFrame = 0;
        }

        public void Update()
        {
            if (_key.Check && !_lastPressed)
            {
                _lastFirstPressFrame = Engine.FrameCounter;
            }
            else if (!_key.Check && _lastPressed)
            {
                _lastFirstNotPressFrame = Engine.FrameCounter;
            }
            _lastPressed = _key.Check;
        }

        private ButtonBinding _key;
        private ShotHookModes _mode;
        private ulong _lastFirstPressFrame;
        private ulong _lastFirstNotPressFrame;
        private bool _lastPressed;
        private ulong _lastRevokedFrame;
    }
}
