using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core
{
    public struct LevelExtras
    {
        public int HookMaterial;
        public bool FeatureEnabled;
        public HookSettings HookSettings;
        public int GameplayMode;
        public int InitialShootCount;

        public LevelExtras()
        {
            HookMaterial = 0;
            FeatureEnabled = false;
            HookSettings = AquaModule.Settings.HookSettings.Clone();
            GameplayMode = (int)GrapplingHook.GameplayMode.Default;
            InitialShootCount = 1;
        }
    }
}
