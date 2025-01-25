using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class RefillExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Refill.ctor_Vector2_bool_bool += Refill_Construct;
            On.Celeste.Refill.Update += Refill_Update;
            On.Celeste.Refill.Respawn += Refill_Respawn;
        }

        public static void Uninitialize()
        {
            On.Celeste.Refill.ctor_Vector2_bool_bool -= Refill_Construct;
            On.Celeste.Refill.Update -= Refill_Update;
            On.Celeste.Refill.Respawn -= Refill_Respawn;
        }

        private static void Refill_Construct(On.Celeste.Refill.orig_ctor_Vector2_bool_bool orig, Refill self, Vector2 position, bool twoDashes, bool oneUse)
        {
            orig(self, position, twoDashes, oneUse);
            self.SetRespawnPosition(self.Position);
        }

        private static void Refill_Update(On.Celeste.Refill.orig_Update orig, Refill self)
        {
            orig(self);
            if (self.IsHookable())
            {
                self.outline.Position = self.GetRespawnPosition() - self.Position;
            }
        }

        private static void Refill_Respawn(On.Celeste.Refill.orig_Respawn orig, Refill self)
        {
            self.Position = self.GetRespawnPosition();
            orig(self);
        }

        public static Vector2 GetRespawnPosition(this Refill self)
        {
            return DynamicData.For(self).Get<Vector2>("respawn_position");
        }

        public static void SetRespawnPosition(this Refill self, Vector2 pos)
        {
            DynamicData.For(self).Set("respawn_position", pos);
        }
    }
}
