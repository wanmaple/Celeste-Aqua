using Celeste.Mod.Aqua.Miscellaneous;
using System.Reflection;

namespace Celeste.Mod.Aqua.Module
{
    [SettingSubMenu]
    public class HookSettings
    {
        [SettingIgnore]
        [DefaultValue(6)]
        public int HookSize { get; set; }  // 钩爪大小

        [SettingRange(50, 150)]
        [DefaultValue(90)]
        public int HookLength { get; set; }  // 钩绳的最大长度

        [SettingIgnore]
        [DefaultValue(235)]
        public int HookBreakSpeed { get; set; } // 强制收回Madeline所需要的速度阈值

        [SettingRange(400, 800, true)]
        [DefaultValue(800)]
        public int HookEmitSpeed { get; set; }  // 钩爪的发射速度

        [SettingRange(40, 60)]
        [DefaultValue(45)]
        public int HookRollingSpeedUp { get; set; }   // 上爬速度

        [SettingRange(80, 120)]
        [DefaultValue(80)]
        public int HookRollingSpeedDown { get; set; } // 下爬速度

        [SettingRange(60, 120)]
        [DefaultValue(90)]
        public int HookSwingStrength { get; set; }    // 摆荡力度

        [SettingRange(0, 50)]
        [DefaultValue(0)]
        public int HookJumpXPercent { get; set; } // 钩爪抓取跳跃X方向的速度倍率

        [SettingRange(100, 150)]
        [DefaultValue(100)]
        public int HookJumpYPercent { get; set; } // 钩爪抓取跳跃Y方向的速度倍率

        [SettingRange(20, 40)]
        [DefaultValue(30)]
        public int HookStaminaRecovery { get; set; }    // 钩爪固定状态时的体力恢复速度

        [SettingIgnore]
        public float HookFlyTowardDuration { get; set; } = 0.15f;

        [SettingRange(300, 500, true)]
        [DefaultValue(400)]
        public int HookFlyTowardSpeed { get; set; }

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
    }
}
