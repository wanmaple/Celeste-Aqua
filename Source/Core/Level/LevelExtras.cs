namespace Celeste.Mod.Aqua.Core
{
    public struct LevelExtras
    {
        public int HookMaterial;
        public bool FeatureEnabled;
        public bool BreakSpeedLimits;

        public LevelExtras()
        {
            HookMaterial = 0;
            FeatureEnabled = true;
            BreakSpeedLimits = false;
        }
    }
}
