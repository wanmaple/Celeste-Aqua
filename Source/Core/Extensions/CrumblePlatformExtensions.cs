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
            _ilOrigAdded = new ILHook(_methodOrigAdded, CrumblePlatform_ILOrigAdded);
        }

        public static void Uninitialize()
        {
            _ilSequence?.Dispose();
            _ilSequence = null;
            _ilOrigAdded?.Dispose();
            _ilOrigAdded = null;
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

        private static void CrumblePlatform_ILOrigAdded(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, ins => ins.MatchLdstr("objects/crumbleBlock/outline")))
            {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(GetOutlineTexture);
            }
        }

        private static string GetOutlineTexture(string defaultTexture, CrumblePlatform self)
        {
            if (self is AquaCrumblePlatform platform)
            {
                if (GFX.Game.Has(platform.OutlineTexture))
                {
                    return platform.OutlineTexture;
                }
            }
            return defaultTexture;
        }

        private static bool WillCrumble(Player player, CrumblePlatform self)
        {
            return player != null || self.IsHookAttached();
        }

        private static bool WillRespawn(CrumblePlatform self)
        {
            return self.CollideCheck<GrapplingHook>() || self.IntersectsWithRope();
        }

        private static MethodInfo _methodSequence = typeof(CrumblePlatform).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        private static MethodInfo _methodOrigAdded = typeof(CrumblePlatform).GetMethod("orig_Added");
        private static ILHook _ilSequence;
        private static ILHook _ilOrigAdded;
    }
}
