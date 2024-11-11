namespace Celeste.Mod.Aqua.Core
{
    public static class FallingBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.FallingBlock.PlayerFallCheck += FallingBlock_PlayerFallCheck;
        }

        public static void Uninitialize()
        {
            On.Celeste.FallingBlock.PlayerFallCheck -= FallingBlock_PlayerFallCheck;
        }

        private static bool FallingBlock_PlayerFallCheck(On.Celeste.FallingBlock.orig_PlayerFallCheck orig, FallingBlock self)
        {
            return orig(self) || (self.climbFall && self.IsHookAttached());
        }
    }
}
