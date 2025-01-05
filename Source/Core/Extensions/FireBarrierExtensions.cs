namespace Celeste.Mod.Aqua.Core
{
    public static class FireBarrierExtensions
    {
        public static void Initialize()
        {
            On.Celeste.FireBarrier.OnChangeMode += FireBarrier_OnChangeMode;
        }

        public static void Uninitialize()
        {
            On.Celeste.FireBarrier.OnChangeMode -= FireBarrier_OnChangeMode;
        }

        private static void FireBarrier_OnChangeMode(On.Celeste.FireBarrier.orig_OnChangeMode orig, FireBarrier self, Session.CoreModes mode)
        {
            orig(self, mode);
            self.solid.SetHookable(mode == Session.CoreModes.Cold);
        }
    }
}
