using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Module;
using MonoMod.Cil;

namespace Celeste.Mod.Aqua.Core
{
    public static class InputExtensions
    {
        public static void Initialize()
        {
            IL.Celeste.Input.GetAimVector += Input_ILGetAimVector;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Input.GetAimVector -= Input_ILGetAimVector;
        }

        private static void Input_ILGetAimVector(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdloc1(), ins => ins.MatchLdcR4(0.0f), ins => ins.MatchBlt(out ILLabel label)))
            {
                // Since the dash deadzone of analog is not symmetric along x-axis, we need to invert the range if the gravity is inverted.
                cursor.Index += 1;
                cursor.EmitDelegate(GetGravityCoefficient);
                cursor.EmitMul();
            }
        }

        private static int GetGravityCoefficient()
        {
            return ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1 : 1;
        }
    }
}
