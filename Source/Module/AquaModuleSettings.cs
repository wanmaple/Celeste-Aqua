using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSettings : EverestModuleSettings
{
    [DefaultButtonBinding(Buttons.LeftShoulder, Keys.T)]
    public ButtonBinding ThrowHook { get; set; }
}