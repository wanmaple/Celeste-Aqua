namespace Celeste.Mod.Aqua.Core
{
    public static class FireBarrierExtensions
    {
        public static void Initialize()
        {
            On.Celeste.FireBarrier.Added += FireBarrier_Added;
        }

        public static void Uninitialize()
        {
            On.Celeste.FireBarrier.Added -= FireBarrier_Added;
        }

        private static void FireBarrier_Added(On.Celeste.FireBarrier.orig_Added orig, FireBarrier self, Monocle.Scene scene)
        {
            orig(self, scene);
            self.solid.SetHookable(false);
        }
    }
}
