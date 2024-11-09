using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSettings : EverestModuleSettings
{
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.T)]
    public ButtonBinding ThrowHook { get; set; }

    public HookSettings HookSettings { get; set; }

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