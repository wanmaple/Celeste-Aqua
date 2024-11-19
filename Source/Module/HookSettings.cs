﻿using Celeste.Mod.Aqua.Miscellaneous;
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
        [DefaultValue(80)]
        public int HookLength { get; set; }  // 钩绳的最大长度

        [SettingRange(200, 300, true)]
        [DefaultValue(235)]
        public int HookBreakSpeed { get; set; } // 钩绳能承受的最大速度

        [SettingRange(400, 800, true)]
        [DefaultValue(600)]
        public int HookEmitSpeed { get; set; }  // 钩爪的发射速度

        [SettingRange(40, 60)]
        [DefaultValue(45)]
        public int HookRollingSpeedUp { get; set; }   // 上爬速度

        [SettingRange(80, 120)]
        [DefaultValue(80)]
        public int HookRollingSpeedDown { get; set; } // 下爬速度

        [SettingRange(60, 120)]
        [DefaultValue(60)]
        public int HookSwingStrength { get; set; }    // 摆荡力度

        [SettingRange(0, 50)]
        [DefaultValue(0)]
        public int HookJumpXPercent { get; set; } // 钩爪抓取跳跃X方向的速度倍率

        [SettingRange(100, 150)]
        [DefaultValue(100)]
        public int HookJumpYPercent { get; set; } // 钩爪抓取跳跃Y方向的速度倍率

        [SettingRange(0, 10)]
        [DefaultValue(5)]
        public int HookClimbUpStaminaCost { get; set; } // 钩爪抓取上爬体力消耗

        [SettingRange(0, 10)]
        [DefaultValue(1)]
        public int HookGrabingStaminaCost { get; set; } // 钩爪抓取不动时体力消耗

        [SettingRange(0, 20)]
        [DefaultValue(10)]
        public int HookJumpStaminaCost { get; set; }    // 勾跳体力消耗

        [SettingIgnore]
        public float HookFlyTowardDuration { get; set; } = 0.15f;

        [SettingRange(300, 400, true)]
        [DefaultValue(325)]
        public int HookFlyTowardSpeed { get; set; }

        [SettingRange(1, 20)]
        [DefaultValue(10)]
        public int HookInertiaCoefficient { get; set; }

        [SettingRange(1, 5)]
        [DefaultValue(2)]
        public int HookWindCoefficient { get; set; }

        //[SettingRange(800, 1200, true)]
        [DefaultValue(1000)]
        public int HookBouncingSpeed { get; set; }

        //[SettingRange(100, 200, true)]
        [DefaultValue(150)]
        public int HookBounceSpeedAddition { get; set; }

        //[SettingRange(5, 30)]
        [DefaultValue(15)]
        public int HookBounceJumpCoefficient { get; set; }

        //[SettingRange(20, 40)]
        [DefaultValue(30)]
        public int HookBounceMoveCoefficient { get; set; }

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
