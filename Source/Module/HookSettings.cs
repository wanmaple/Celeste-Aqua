using Celeste.Mod.Aqua.Miscellaneous;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Module
{
    [Serializable]
    [SettingSubMenu]
    public class HookSettings
    {
        [SettingName("SETTINGS_ROPE_LENGTH")]
        [SettingRange(50, 150)]
        [DefaultValue(80)]
        public int RopeLength { get; set; }  // 钩绳的最大长度

        [SettingName("SETTINGS_EMIT_SPEED")]
        [SettingRange(400, 800, true)]
        [DefaultValue(600)]
        public int EmitSpeed { get; set; }  // 钩爪的发射速度

        [SettingIgnore]
        [DefaultValue(60)]
        public int SwingStrength { get; set; }    // 摆荡力度

        [SettingName("SETTINGS_MAX_LINE_SPEED")]
        [SettingRange(400, 2000, true)]
        [DefaultValue(450)]
        public int MaxLineSpeed { get; set; }   // 最大线速度

        [SettingIgnore]
        [DefaultValue(5)]
        public int ClimbUpStaminaCost { get; set; } // 钩爪抓取上爬体力消耗

        [SettingIgnore]
        [DefaultValue(1)]
        public int GrabingStaminaCost { get; set; } // 钩爪抓取不动时体力消耗

        [SettingIgnore]
        [DefaultValue(20)]
        public int SwingJumpStaminaCost { get; set; }    // 摆荡跳体力消耗

        [SettingIgnore]
        public float FlyTowardDuration { get; set; } = 0.15f;

        [SettingName("SETTINGS_FLY_TOWARD_SPEED")]
        [SettingRange(300, 1000, true)]
        [DefaultValue(325)]
        public int FlyTowardSpeed { get; set; }

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
