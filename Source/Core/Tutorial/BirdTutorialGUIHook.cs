using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class BirdTutorialGUIHook
    {
        public static void Initialize()
        {
            IL.Celeste.BirdTutorialGui.Render += BirdTutorialGui_ILRender;
        }

        public static void Uninitialize()
        {
            IL.Celeste.BirdTutorialGui.Render += BirdTutorialGui_ILRender;
        }

        private static void BirdTutorialGui_ILRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdloc0(), ins => ins.MatchCallvirt(out var method)))
            {
                cursor.Index += 3;
                ILLabel label = cursor.DefineLabel();
                cursor.MarkLabel(label);
                cursor.Index -= 3;
                cursor.EmitBr(label);
            }
        }
    }
}
