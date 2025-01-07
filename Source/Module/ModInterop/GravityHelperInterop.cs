using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.Aqua.Module
{
    public class GravityHelperInterop : Interop
    {
        public const int GRAVITY_NORMAL = 0;
        public const int GRAVITY_INVERTED = 1;
        public const int GRAVITY_TOGGLE = 2;

        [ModImportName("GravityHelper")]
        public static class GravityHelperImports
        {
            public static Func<int> GetPlayerGravity;
            public static Func<bool> IsPlayerInverted;
            public static Func<string, int> GravityTypeToInt;
            public static Action<int, float> SetPlayerGravity;
        }

        public GravityHelperInterop()
            : base("GravityHelper", 1, 2, 21)
        {
        }

        public void Load()
        {
            typeof(GravityHelperImports).ModInterop();
        }

        public int GetPlayerGravity()
        {
            if (IsLoaded && GravityHelperImports.GetPlayerGravity != null)
                GravityHelperImports.GetPlayerGravity.Invoke();
            return GRAVITY_NORMAL;
        }

        public bool IsPlayerGravityInverted()
        {
            if (IsLoaded && GravityHelperImports.IsPlayerInverted != null)
                return GravityHelperImports.IsPlayerInverted.Invoke();
            return false;
        }

        public int GravityTypeToInt(string gravityType)
        {
            if (IsLoaded && GravityHelperImports.GravityTypeToInt != null)
                return GravityHelperImports.GravityTypeToInt.Invoke(gravityType);
            return GRAVITY_NORMAL;
        }

        public void SetPlayerGravity(int gravityType, float momentumMultiplier = 1.0f)
        {
            if (IsLoaded && GravityHelperImports.SetPlayerGravity != null)
                GravityHelperImports.SetPlayerGravity.Invoke(gravityType, momentumMultiplier);
        }

        public Color HighlightColor(int gravityType)
        {
            switch (gravityType)
            {
                case 0:
                    return Calc.HexToColor("007cff");
                case 1:
                    return Calc.HexToColor("dc1828");
                case 2:
                    return Calc.HexToColor("ca41f5");
                default:
                    return Color.White;
            }
        }
    }
}
