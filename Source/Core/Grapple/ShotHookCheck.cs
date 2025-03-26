using Celeste.Mod.Aqua.Module;
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
                    return AquaModule.Settings.ThrowHook.Pressed;
                return (_lastFirstNotPressFrame >= _lastRevokedFrame) && AquaModule.Settings.ThrowHook.Check;
            }
        }

        public bool CanRevoke
        {
            get
            {
                if (_mode == ShotHookModes.Default)
                    return AquaModule.Settings.ThrowHook.Pressed;
                return !AquaModule.Settings.ThrowHook.Check;
            }
        }

        public bool CanGrappleBoost
        {
            get
            {
                if (_mode == ShotHookModes.Default)
                    return AquaModule.Settings.ThrowHook.Pressed;
                return AquaModule.Settings.ThrowHook.Check && _lastFirstNotPressFrame > _lastFirstPressFrame;
            }
        }

        public ShotHookCheck(ShotHookModes mode)
        {
            _mode = mode;
            Reset();
        }

        public void EndPeriod()
        {
            _lastRevokedFrame = Engine.FrameCounter;
            if (!AquaModule.Settings.ThrowHook.Check)
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
            if (AquaModule.Settings.ThrowHook.Check && !_lastPressed)
            {
                _lastFirstPressFrame = Engine.FrameCounter;
            }
            else if (!AquaModule.Settings.ThrowHook.Check && _lastPressed)
            {
                _lastFirstNotPressFrame = Engine.FrameCounter;
            }
            _lastPressed = AquaModule.Settings.ThrowHook.Check;
        }

        private ShotHookModes _mode;
        private ulong _lastFirstPressFrame;
        private ulong _lastFirstNotPressFrame;
        private bool _lastPressed;
        private ulong _lastRevokedFrame;
    }
}
