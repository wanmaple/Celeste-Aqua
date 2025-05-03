namespace Celeste.Mod.Aqua.Core
{
    public static class IceBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.IceBlock.Added += IceBlock_Added;
        }

        public static void Uninitialize()
        {
            On.Celeste.IceBlock.Added -= IceBlock_Added;
        }

        private static void IceBlock_Added(On.Celeste.IceBlock.orig_Added orig, IceBlock self, Monocle.Scene scene)
        {
            orig(self, scene);
            self.solid.SetHookable(false);
        }
    }
}
