using Mono.Cecil;
using MonoMod.Cil;

namespace Celeste.Mod.Aqua.Core
{
    public static class BounceBlockExtensions
    {
        public static void Initialize()
        {
            IL.Celeste.BounceBlock.Update += BounceBlock_ILUpdate;
            On.Celeste.BounceBlock.WindUpPlayerCheck += BounceBlock_WindUpPlayerCheck;
        }

        public static void Uninitialize()
        {
            IL.Celeste.BounceBlock.Update -= BounceBlock_ILUpdate;
            On.Celeste.BounceBlock.WindUpPlayerCheck -= BounceBlock_WindUpPlayerCheck;
        }

        private static void BounceBlock_ILUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            ILLabel label = null;
            // Twice.
            for (int i = 0; i < 2; ++i)
            {
                cursor.TryGotoNext(MoveType.After, ins => ins.MatchLdarg0(), ins => ins.MatchCall(out MethodReference nouse), ins => ins.MatchBrtrue(out label));
            }
            cursor.EmitLdarg0();
            cursor.EmitDelegate(WillRespawn);
            cursor.EmitBrtrue(label);
        }

        private static Player BounceBlock_WindUpPlayerCheck(On.Celeste.BounceBlock.orig_WindUpPlayerCheck orig, BounceBlock self)
        {
            Player player = orig(self);
            if (player == null)
            {
                if (self.IsHookAttached())
                {
                    var grapples = self.Scene.Tracker.GetEntities<GrapplingHook>();
                    foreach (GrapplingHook grapple in grapples)
                    {
                        if (grapple.State == GrapplingHook.HookStates.Fixed && grapple.AttachedEntity == self)
                        {
                            return new Player(grapple.Position, PlayerSpriteMode.Madeline); // Just a temp player with a position which I need to use after.
                        }
                    }
                }
            }
            return player;
        }

        private static bool WillRespawn(BounceBlock self)
        {
            return self.CollideCheck<GrapplingHook>() || self.IntersectsWithRope();
        }
    }
}
