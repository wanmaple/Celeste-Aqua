using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework.Input;
using System;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSettings : EverestModuleSettings
{
    public event Action<bool> FeatureEnableChanged;
    public event Action<bool> DisableGrappleBoostChanged;
    public event Action<bool> ShortDistanceGrappleBoostChanged;
    public event Action<bool> Ungrapple16DirectionChanged;

    [SettingName("SETTINGS_THROW_HOOK")]
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.V)]
    public ButtonBinding ThrowHook { get; set; }

    [SettingName("SETTINGS_SWITCH_AUTO_GRAB")]
    [DefaultButtonBinding(Buttons.Y, Keys.Tab)]
    public ButtonBinding SwitchAutoGrab { get; set; }

    [SettingName("SETTINGS_DOWN_SHOOT")]
    [DefaultButtonBinding(Buttons.RightShoulder, Keys.None)]
    public ButtonBinding DownShoot { get; set; }

    [SettingName("SETTINGS_FEATURE_ENABLED")]
    public bool FeatureEnabled
    {
        get => _featureEnabled;
        set
        {
            if (_featureEnabled != value)
            {
                _featureEnabled = value;
                FeatureEnableChanged?.Invoke(_featureEnabled);
            }
        }
    }

    private bool _featureEnabled = false;

    [SettingName("SETTINGS_HOOK_PARAMETERS")]
    public HookSettings HookSettings { get; set; } = new HookSettings();

    public bool ResetHookSettings { get; set; }

    [SettingName("SETTINGS_AUTO_GRAB_ON")]
    public bool AutoGrabRopeIfPossible { get; set; }

    [SettingName("SETTINGS_DISABLE_GRAPPLE_BOOST")]
    public bool DisableGrappleBoost
    {
        get => _disableGrappleBoost;
        set
        {
            if (_disableGrappleBoost !=value)
            {
                _disableGrappleBoost = value;
                DisableGrappleBoostChanged?.Invoke(_disableGrappleBoost);
            }
        }
    }

    private bool _disableGrappleBoost=false;

    [SettingName("SETTINGS_SHORT_DISTANCE_GRAPPLE_BOOST")]
    public bool ShortDistanceGrappleBoost
    {
        get => _shortDistanceGrappleBoost;
        set
        {
            if (_shortDistanceGrappleBoost != value)
            {
                _shortDistanceGrappleBoost = value;
                ShortDistanceGrappleBoostChanged?.Invoke(_shortDistanceGrappleBoost);
            }
        }
    }
    private bool _shortDistanceGrappleBoost = false;

    [SettingName("SETTINGS_UNGRAPPLE_16_DIRECTION")]
    public bool Ungrapple16Direction
    {
        get => _ungrapple16dir;
        set
        {
            if (_ungrapple16dir != value)
            {
                _ungrapple16dir = value;
                Ungrapple16DirectionChanged?.Invoke(_ungrapple16dir);
            }
        }
    }

    private bool _ungrapple16dir = false;

    [SettingName("SETTINGS_THROW_HOOK_MODE")]
    public ShotHookModes ThrowHookMode { get; set; }

    [SettingName("SETTINGS_DEFAULT_SHOT_DIRECTION")]
    public DefaultShotDirections DefaultShotDirection { get; set; } = DefaultShotDirections.Up;

    [SettingName("SETTINGS_INDICATOR_TYPE")]
    public GrappleIndicatorType IndicatorSetting { get; set; } = GrappleIndicatorType.None;

    [SettingName("SETTINGS_SHOW_AUTO_GRAB_ICON")]
    public bool ShowAutoGrabIcon { get; set; } = true;

    public void CreateResetHookSettingsEntry(TextMenu menu, bool inGame)
    {
        menu.Add(new TextMenu.Button(Dialog.Get("SETTINGS_RESET_HOOK_PARAMETERS")).Pressed(OnResetHookSettings));
    }

    private void OnResetHookSettings()
    {
        HookSettings.Reset();
    }
}