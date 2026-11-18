using Celeste.Mod.Aqua.Miscellaneous;
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
        }

        private static void Refill_Update(On.Celeste.Refill.orig_Update orig, Refill self)
        {
            orig(self);
            if (self.IsHookable() && HasRespawnPosition(self))
            {
                self.outline.Position = self.GetRespawnPosition() - self.Position;
            }
        }

        private static void Refill_Respawn(On.Celeste.Refill.orig_Respawn orig, Refill self)
        {
            if (HasRespawnPosition(self))
            {
                self.Position = self.GetRespawnPosition();
                DataContainer.For(self).RespawnPosition = null;
            }
            orig(self);
            if (self is CustomRefill refill && refill.SyncHoldableContainer)
            {
                var container = refill.GetHoldableContainer();
                if (container != null)
                {
                    container.Position = refill.Position + refill.HoldableContainerOffset;
                    container.Active = true;
                }
            }
        }

        public static bool HasRespawnPosition(this Refill self)
        {
            return DataContainer.For(self).RespawnPosition is not null;
        }

        public static Vector2 GetRespawnPosition(this Refill self)
        {
            return DataContainer.For(self).RespawnPosition ?? Vector2.Zero;
        }
    }
}
