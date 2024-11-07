using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Aqua.Module
{
    [SettingSubMenu]
    public class HookSettings
    {
        [SettingIgnore]
        public int HookSize { get; set; } = 8;  // 钩爪大小

        [SettingRange(50, 100)]
        public int HookLength { get; set; } = 70;  // 钩绳的最大长度

        [SettingIgnore]
        public int HookBreakSpeed { get; set; } = 235; // 强制收回Madeline所需要的速度阈值

        [SettingRange(300, 500)]
        public int HookEmitSpeed { get; set; } = 400;  // 钩爪的发射速度

        [SettingRange(20, 40)]
        public int HookRollingSpeedUp { get; set; } = 20;   // 上爬速度

        [SettingRange(40, 80)]
        public int HookRollingSpeedDown { get; set; } = 60; // 下爬速度

        [SettingRange(50, 150)]
        public int HookSwingSpeed { get; set; } = 100;

        [SettingRange(-50, 50)]
        public int HookJumpXPercent { get; set; } = 10; // 钩爪抓取跳跃X方向的速度倍率

        [SettingRange(50, 150)]
        public int HookJumpYPercent { get; set; } = 100; // 钩爪抓取跳跃Y方向的速度倍率
    }
}
