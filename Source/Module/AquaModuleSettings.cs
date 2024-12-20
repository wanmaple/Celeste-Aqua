using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSettings : EverestModuleSettings
{
    [SettingName("SETTINGS_THROW_HOOK")]
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.V)]
    public ButtonBinding ThrowHook { get; set; }

    [SettingName("SETTINGS_SWITCH_AUTO_GRAB")]
    [DefaultButtonBinding(Buttons.Y, Keys.Tab)]
    public ButtonBinding SwitchAutoGrab { get; set; }

    [SettingName("SETTINGS_FEATURE_ENABLED")]
    public bool FeatureEnabled { get; set; } = true;

    [SettingName("SETTINGS_HOOK_PARAMETERS")]
    public HookSettings HookSettings { get; set; } = new HookSettings();

    public bool ResetHookSettings { get; set; }

    [SettingName("SETTINGS_AUTO_GRAB_ON")]
    public bool AutoGrabRopeIfPossible { get; set; }

    [SettingName("SETTINGS_THROW_HOOK_MODE")]
    public ThrowHookModes ThrowHookMode { get; set; }

    public void CreateResetHookSettingsEntry(TextMenu menu, bool inGame)
    {
        menu.Add(new TextMenu.Button(Dialog.Get("SETTINGS_RESET_HOOK_PARAMETERS")).Pressed(OnResetHookSettings));
    }

    private void OnResetHookSettings()
    {
        HookSettings.Reset();
    }
}