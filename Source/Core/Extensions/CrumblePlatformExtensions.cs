using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class CrumblePlatformExtensions
    {
        public static void Initialize()
        {
            _ilSequence = new ILHook(_methodSequence, CrumblePlatform_ILSequence);
        }

        public static void Uninitialize()
        {
            _ilSequence?.Dispose();
            _ilSequence = null;
        }

        private static void CrumblePlatform_ILSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            MethodInfo methodPlayerClimbing = typeof(Solid).GetMethod("GetPlayerClimbing", BindingFlags.Instance | BindingFlags.Public);
            if (methodPlayerClimbing != null)
            {
                if (cursor.TryGotoNext(MoveType.After, ins => ins.MatchLdloc1(), ins => ins.MatchCallvirt(methodPlayerClimbing)))
                {
                    cursor.EmitLdloc1();
                    cursor.EmitDelegate(WillCrumble);

                    ILLabel label = null;
                    while (cursor.TryGotoNext(MoveType.After, ins => ins.MatchLdloc1(), ins => ins.MatchCallvirt(out MethodReference nouse), ins => ins.MatchBrtrue(out label))) ;
                    cursor.EmitLdloc1();
                    cursor.EmitDelegate(WillRespawn);
                    cursor.EmitBrtrue(label);
                }
            }
        }

        private static bool WillCrumble(Player player, CrumblePlatform self)
        {
            return player == null && self.IsHookAttached();
        }

        private static bool WillRespawn(CrumblePlatform self)
        {
            return self.CollideCheck<GrapplingHook>() || self.IntersectsWithRope();
        }

        private static MethodInfo _methodSequence = typeof(CrumblePlatform).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        private static ILHook _ilSequence;
    }
}
