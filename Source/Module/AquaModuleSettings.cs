using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSettings : EverestModuleSettings
{
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.V)]
    public ButtonBinding ThrowHook { get; set; }

    public bool FeatureEnabled { get; set; } = true;

    public HookSettings HookSettings { get; set; } = new HookSettings();

    public bool ResetHookSettings { get; set; }

    public void CreateResetHookSettingsEntry(TextMenu menu, bool inGame)
    {
        menu.Add(new TextMenu.Button("Reset Hook Settings").Pressed(OnResetHookSettings));
    }

    private void OnResetHookSettings()
    {
        HookSettings.Reset();
    }
}