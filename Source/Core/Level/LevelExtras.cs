using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core
{
    public struct LevelExtras
    {
        public int HookMaterial;
        public bool FeatureEnabled;
        public HookSettings HookSettings;

        public LevelExtras()
        {
            HookMaterial = 0;
            FeatureEnabled = true;
            HookSettings = AquaModule.Settings.HookSettings.Clone();
        }
    }
}
