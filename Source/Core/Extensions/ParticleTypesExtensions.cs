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
            AquaBooster.P_BurstOrange = new ParticleType(Booster.P_Burst)
            {
                Color = Calc.HexToColor("bc630e"),
            };
            AquaBooster.P_BurstPurple = new ParticleType(Booster.P_BurstRed)
            {
                Color = Calc.HexToColor("760ebc"),
            };
            AquaBooster.P_AppearOrange = new ParticleType(Booster.P_Appear)
            {
                Color = Calc.HexToColor("bc630e"),
            };
            AquaBooster.P_AppearPurple = new ParticleType(Booster.P_RedAppear)
            {
                Color = Calc.HexToColor("760ebc"),
            };
            AquaRefill.P_ShatterHookable1 = new ParticleType(Refill.P_Shatter)
            {
                Color = Calc.HexToColor("f19310"),
                Color2 = Calc.HexToColor("824c00"),
            };
            AquaRefill.P_GlowHookable1 = new ParticleType(Refill.P_Glow)
            {
                Color = Calc.HexToColor("f19310"),
                Color2 = Calc.HexToColor("824c00"),
            };
            AquaRefill.P_RegenHookable1 = new ParticleType(Refill.P_Regen)
            {
                Color = Calc.HexToColor("f19310"),
                Color2 = Calc.HexToColor("824c00"),
            };
            AquaRefill.P_ShatterHookable2 = new ParticleType(Refill.P_ShatterTwo)
            {
                Color = Calc.HexToColor("912ed4"),
                Color2 = Calc.HexToColor("4b1680"),
            };
            AquaRefill.P_GlowHookable2 = new ParticleType(Refill.P_GlowTwo)
            {
                Color = Calc.HexToColor("912ed4"),
                Color2 = Calc.HexToColor("4b1680"),
            };
            AquaRefill.P_RegenHookable2 = new ParticleType(Refill.P_Regen)
            {
                Color = Calc.HexToColor("912ed4"),
                Color2 = Calc.HexToColor("4b1680"),
            };
            GrapplingRefill.P_Shatter = new ParticleType(Refill.P_Shatter)
            {
                Color = Calc.HexToColor("909cb0"),
                Color2 = Calc.HexToColor("515672"),
            };
            GrapplingRefill.P_Glow = new ParticleType(Refill.P_Glow)
            {
                Color = Calc.HexToColor("909cb0"),
                Color2 = Calc.HexToColor("515672"),
            };
            GrapplingRefill.P_Regen = new ParticleType(Refill.P_Regen)
            {
                Color = Calc.HexToColor("909cb0"),
                Color2 = Calc.HexToColor("515672"),
            };
            GrapplingRefill.P_Shatter2 = new ParticleType(Refill.P_Shatter)
            {
                Color = Calc.HexToColor("d0bee9"),
                Color2 = Calc.HexToColor("8e7ca6"),
            };
            GrapplingRefill.P_Glow2 = new ParticleType(Refill.P_Glow)
            {
                Color = Calc.HexToColor("d0bee9"),
                Color2 = Calc.HexToColor("8e7ca6"),
            };
            GrapplingRefill.P_Regen2 = new ParticleType(Refill.P_Regen)
            {
                Color = Calc.HexToColor("d0bee9"),
                Color2 = Calc.HexToColor("8e7ca6"),
            };
        }
    }
}
