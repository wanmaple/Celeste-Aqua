using Celeste.Mod.Aqua.Debug;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class ThrowHookCheck
    {
        public bool CanThrow
        {
            get
            {
                if (_mode == ThrowHookModes.Default)
                    return _key.Pressed;
                return (_lastFirstNotPressFrame >= _lastRevokedFrame) && _key.Check;
            }
        }

        public bool CanRevoke
        {
            get
            {
                if (_mode == ThrowHookModes.Default)
                    return _key.Pressed;
                return !_key.Check;
            }
        }

        public bool CanFlyTowards
        {
            get
            {
                if (_mode == ThrowHookModes.Default)
                    return _key.Pressed;
                return _key.Check && _lastFirstNotPressFrame > _lastFirstPressFrame;
            }
        }

        public ThrowHookCheck(ButtonBinding key, ThrowHookModes mode)
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
                AquaDebugger.LogInfo("Last Press {0}", _lastFirstPressFrame);
            }
            else if (!_key.Check && _lastPressed)
            {
                _lastFirstNotPressFrame = Engine.FrameCounter;
                AquaDebugger.LogInfo("Last Unpress {0}", _lastFirstNotPressFrame);
            }
            _lastPressed = _key.Check;
        }

        private ButtonBinding _key;
        private ThrowHookModes _mode;
        private ulong _lastFirstPressFrame;
        private ulong _lastFirstNotPressFrame;
        private bool _lastPressed;
        private ulong _lastRevokedFrame;
    }
}
