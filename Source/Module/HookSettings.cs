using Celeste.Mod.Aqua.Miscellaneous;
using System.Reflection;

namespace Celeste.Mod.Aqua.Module
{
    [SettingSubMenu]
    public class HookSettings
    {
        [SettingIgnore]
        [DefaultValue(6)]
        public int Size { get; set; }  // 钩爪大小

        [SettingRange(50, 150)]
        [DefaultValue(80)]
        public int RopeLength { get; set; }  // 钩绳的最大长度

        [SettingRange(40, 100, true)]
        [DefaultValue(80)]
        public int BreakSpeed { get; set; } // 钩绳能承受的最大速度

        [SettingRange(400, 800, true)]
        [DefaultValue(600)]
        public int EmitSpeed { get; set; }  // 钩爪的发射速度

        [SettingRange(40, 60)]
        [DefaultValue(45)]
        public int RollingSpeedUp { get; set; }   // 上爬速度

        [SettingRange(80, 120)]
        [DefaultValue(80)]
        public int RollingSpeedDown { get; set; } // 下爬速度

        [SettingRange(60, 120)]
        [DefaultValue(60)]
        public int SwingStrength { get; set; }    // 摆荡力度

        [SettingRange(400, 600)]
        [DefaultValue(450)]
        public int MaxLineSpeed { get; set; }   // 最大线速度

        [SettingRange(0, 50)]
        [DefaultValue(0)]
        public int SwingJumpXPercent { get; set; } // 钩爪摆荡跳X方向的额外速度倍率

        [SettingRange(100, 150)]
        [DefaultValue(100)]
        public int SwingJumpYPercent { get; set; } // 钩爪摆荡跳Y方向的额外速度倍率

        [SettingRange(0, 10)]
        [DefaultValue(5)]
        public int ClimbUpStaminaCost { get; set; } // 钩爪抓取上爬体力消耗

        [SettingRange(0, 10)]
        [DefaultValue(1)]
        public int GrabingStaminaCost { get; set; } // 钩爪抓取不动时体力消耗

        [SettingRange(0, 30)]
        [DefaultValue(20)]
        public int SwingJumpStaminaCost { get; set; }    // 摆荡跳体力消耗

        [SettingIgnore]
        public float FlyTowardDuration { get; set; } = 0.15f;

        [SettingRange(300, 400, true)]
        [DefaultValue(325)]
        public int FlyTowardSpeed { get; set; }

        [SettingRange(1, 10)]
        [DefaultValue(5)]
        public int InertiaCoefficient { get; set; }

        [SettingRange(1, 5)]
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
    }
}
