using Celeste.Mod.Aqua.Core;
using Celeste.Mod.Aqua.Miscellaneous;
using Monocle;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Module
{
    [Serializable]
    [SettingSubMenu]
    public class HookSettings
    {
        [SettingName("SETTINGS_ROPE_LENGTH")]
        [SettingRange(50, 200)]
        [DefaultValue(80)]
        public int RopeLength
        {
            get => _ropeLength;
            set
            {
                if (_ropeLength != value)
                {
                    _ropeLength = value;
                    if (Engine.Instance.scene is Level level)
                    {
                        level.SyncPropertyIfPossible(state =>
                        {
                            state.HookSettings._ropeLength = _ropeLength;
                            Player player = level.Tracker.GetEntity<Player>();
                            if (player != null)
                            {
                                var hook = player.GetGrappleHook();
                                hook.SetRopeLength(_ropeLength);
                            }
                        });
                    }
                }
            }
        }

        private int _ropeLength = 80;

        [SettingName("SETTINGS_EMIT_SPEED")]
        [SettingRange(600, 2000, true)]
        [DefaultValue(600)]
        public int EmitSpeed
        {
            get => _emitSpeed;
            set
            {
                if (_emitSpeed != value)
                {
                    _emitSpeed = value;
                    if (Engine.Instance.scene is Level level)
                    {
                        level.SyncPropertyIfPossible(state => state.HookSettings._emitSpeed = _emitSpeed);
                    }
                }
            }
        }

        private int _emitSpeed = 600;

        [SettingIgnore]
        [DefaultValue(60)]
        public int SwingStrength { get; set; }

        [SettingName("SETTINGS_MAX_LINE_SPEED")]
        [SettingRange(400, 2000, true)]
        [DefaultValue(450)]
        public int MaxLineSpeed
        {
            get => _maxLineSpeed;
            set
            {
                if (_maxLineSpeed != value)
                {
                    _maxLineSpeed = value;
                    if (Engine.Instance.scene is Level level)
                    {
                        level.SyncPropertyIfPossible(state => state.HookSettings._maxLineSpeed = _maxLineSpeed);
                    }
                }
            }
        }

        private int _maxLineSpeed = 450;

        [SettingIgnore]
        [DefaultValue(5)]
        public int ClimbUpStaminaCost { get; set; }

        [SettingIgnore]
        [DefaultValue(1)]
        public int GrabingStaminaCost { get; set; }

        [SettingIgnore]
        [DefaultValue(20)]
        public int SwingJumpStaminaCost { get; set; }

        [SettingIgnore]
        public float FlyTowardDuration { get; set; } = 0.15f;

        [SettingName("SETTINGS_FLY_TOWARD_SPEED")]
        [SettingRange(300, 500, true)]
        [DefaultValue(325)]
        public int FlyTowardSpeed
        {
            get => _flyTowardSpeed;
            set
            {
                if (_flyTowardSpeed != value)
                {
                    _flyTowardSpeed = value;
                    if (Engine.Instance.scene is Level level)
                    {
                        level.SyncPropertyIfPossible(state => state.HookSettings._flyTowardSpeed = _flyTowardSpeed);
                    }
                }
            }
        }

        private int _flyTowardSpeed = 325;

        [SettingIgnore]
        [DefaultValue(2)]
        public int WindCoefficient { get; set; }

        public HookSettings()
        {
            Reset();
        }

        public void Reset()
        {
            var props = GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DefaultValueAttribute defAttr = prop.GetCustomAttribute<DefaultValueAttribute>();
                if (defAttr != null)
                {
                    prop.SetValue(this, defAttr.DefaultValue);
                }
            }
        }

        public HookSettings Clone()
        {
            var clone = new HookSettings();
            var props = GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                prop.SetValue(clone, prop.GetValue(this));
            }
            return clone;
        }
    }
}
