using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.Aqua.Module
{
    public class ExtendedVariantModeInterop : Interop
    {
        [ModImportName("ExtendedVariantMode")]
        public static class ExtendedVariantModeImports
        {
            public static Func<string, object> GetCurrentVariantValue;
        }

        public ExtendedVariantModeInterop()
            : base("ExtendedVariantMode", 0, 40, 1)
        {
        }

        public void Load()
        {
            typeof(ExtendedVariantModeImports).ModInterop();
        }

        public float GetCurrentGravityMultiplier()
        {
            object value = GetCurrentVariantValue("Gravity");
            if (value != null)
                return Convert.ToSingle(value);
            return 1.0f;
        }

        public object GetCurrentVariantValue(string variantName)
        {
            if (IsLoaded && ExtendedVariantModeImports.GetCurrentVariantValue != null)
                return ExtendedVariantModeImports.GetCurrentVariantValue.Invoke(variantName);
            return null;
        }
    }
}
