using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using KeyNames = Celeste.Mod.Aqua.Miscellaneous.DataContainer.KeyNames;

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
            if (DataContainer.For(self).Has(KeyNames.RespawnPosition))
            {
                self.Position = DataContainer.For(self).Get<Vector2>(KeyNames.RespawnPosition);
                DataContainer.For(self).Remove(KeyNames.RespawnPosition);
            }
            orig(self);
        }

        private static void FlyFeather_Update(On.Celeste.FlyFeather.orig_Update orig, FlyFeather self)
        {
            orig(self);
            if (self.IsHookable() && DataContainer.For(self).Has(KeyNames.RespawnPosition))
            {
                self.outline.Position = DataContainer.For(self).Get<Vector2>(KeyNames.RespawnPosition) - self.Position;
            }
        }
    }
}
