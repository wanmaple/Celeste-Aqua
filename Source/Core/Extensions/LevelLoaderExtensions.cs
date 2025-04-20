using MonoMod.Cil;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelLoaderExtensions
    {
        public static void Initialize()
        {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Uninitialize()
        {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            self.Level.Add(new BarrierRenderer());
            self.Level.Add(new RodEntityManager());
            self.Level.Add(new UnhookableTileCenter());
        }
    }
}
