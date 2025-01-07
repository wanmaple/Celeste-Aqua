using MonoMod.ModInterop;

namespace Celeste.Mod.Aqua.Module
{
    public static class ModInterop
    {
        public static GravityHelperInterop GravityHelper => _interopGravityHelper;
        public static MaxHelpingHandInterop MaxHelpingHand => _interopMaxHelpingHand;

        public static void Initialize()
        {
            typeof(AquaExports).ModInterop();
            _interopGravityHelper.Load();
        }

        public static void Uninitialize()
        {
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
        private static MaxHelpingHandInterop _interopMaxHelpingHand = new MaxHelpingHandInterop();
    }
}
