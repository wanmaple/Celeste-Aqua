using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Rendering;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public struct LevelExtras
    {
        public int HookMaterial;
        public bool FeatureEnabled;
        public HookSettings HookSettings;
        public int GameplayMode;
        public int InitialShootCount;
        public List<BackgroundData> Backgrounds;

        public LevelExtras()
        {
            HookMaterial = 0;
            FeatureEnabled = false;
            HookSettings = AquaModule.Settings.HookSettings.Clone();
            GameplayMode = (int)GrapplingHook.GameplayMode.Default;
            InitialShootCount = 1;
            Backgrounds = new List<BackgroundData>(4);
        }
    }
}
