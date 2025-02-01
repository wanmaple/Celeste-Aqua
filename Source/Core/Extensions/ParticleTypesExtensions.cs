using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class ParticleTypesExtensions
    {
        public static void Initialize()
        {
            On.Celeste.ParticleTypes.Load += ParticleTypes_Load;
        }

        public static void Uninitialize()
        {
            On.Celeste.ParticleTypes.Load -= ParticleTypes_Load;
        }

        private static void ParticleTypes_Load(On.Celeste.ParticleTypes.orig_Load orig)
        {
            orig();

            TrapGate.P_Behind = new ParticleType(SwitchGate.P_Behind);
            TrapGate.P_Dust = new ParticleType(SwitchGate.P_Dust);
        }
    }
}
