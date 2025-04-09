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
            public static Action<string, int, bool> TriggerIntegerVariant;
            public static Action<string, bool, bool> TriggerBooleanVariant;
            public static Action<string, float, bool> TriggerFloatVariant;
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

        public float GetCurrentMaxFallMultiplier()
        {
            object value = GetCurrentVariantValue("FallSpeed");
            if (value != null)
                return Convert.ToSingle(value);
            return 1.0f;
        }

        public void TriggerMaxFallMultiplier(float multiplier)
        {
            TriggerFloatVariant("FallSpeed", multiplier, true);
        }

        public object GetCurrentVariantValue(string variantName)
        {
            if (IsLoaded && ExtendedVariantModeImports.GetCurrentVariantValue != null)
                return ExtendedVariantModeImports.GetCurrentVariantValue.Invoke(variantName);
            return null;
        }

        public void TriggerIntegerVariant(string variantName, int value, bool revertOnDeath)
        {
            if (IsLoaded && ExtendedVariantModeImports.TriggerIntegerVariant != null)
                ExtendedVariantModeImports.TriggerIntegerVariant.Invoke(variantName, value, revertOnDeath);
        }

        public void TriggerBooleanVariant(string variantName, bool value, bool revertOnDeath)
        {
            if (IsLoaded && ExtendedVariantModeImports.TriggerBooleanVariant != null)
                ExtendedVariantModeImports.TriggerBooleanVariant.Invoke(variantName, value, revertOnDeath);
        }

        public void TriggerFloatVariant(string variantName, float value, bool revertOnDeath)
        {
            if (IsLoaded && ExtendedVariantModeImports.TriggerFloatVariant != null)
                ExtendedVariantModeImports.TriggerFloatVariant.Invoke(variantName, value, revertOnDeath);
        }
    }
}
