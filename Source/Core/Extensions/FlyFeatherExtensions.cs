using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class FlyFeatherExtensions
    {
        public static void Initialize()
        {
            On.Celeste.FlyFeather.Respawn += FlyFeather_Respawn;
            On.Celeste.FlyFeather.Update += FlyFeather_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.FlyFeather.Respawn -= FlyFeather_Respawn;
            On.Celeste.FlyFeather.Update -= FlyFeather_Update;
        }

        private static void FlyFeather_Respawn(On.Celeste.FlyFeather.orig_Respawn orig, FlyFeather self)
        {
            if (DataContainer.For(self).RespawnPosition is Vector2 respawnPosition)
            {
                self.Position = respawnPosition;
                DataContainer.For(self).RespawnPosition = null;
            }
            orig(self);
        }

        private static void FlyFeather_Update(On.Celeste.FlyFeather.orig_Update orig, FlyFeather self)
        {
            orig(self);
            if (self.IsHookable() && DataContainer.For(self).RespawnPosition is Vector2 respawnPosition)
            {
                self.outline.Position = respawnPosition - self.Position;
            }
        }
    }
}
