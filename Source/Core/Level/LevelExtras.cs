using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Rendering;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public struct LevelExtras
    {
        public int HookMaterial;
        public bool FeatureEnabled;
        public bool DisableUserCustomParameters;
        public HookSettings HookSettings;
        public int GameplayMode;
        public int InitialShootCount;
        public int MaxShootCount;
        public bool ResetCountInTransition;
        public List<BackgroundData> Backgrounds;

        public LevelExtras()
        {
            HookMaterial = 0;
            FeatureEnabled = false;
            DisableUserCustomParameters = false;
            HookSettings = AquaModule.Settings.HookSettings;
            GameplayMode = (int)GrapplingHook.GameplayMode.Default;
            InitialShootCount = 1;
            MaxShootCount = -1;
            ResetCountInTransition = true;
            Backgrounds = new List<BackgroundData>(4);
        }
    }
}
