using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module
{
    public static class ModInterop
    {
        public static GravityHelperInterop GravityHelper => _interopGravityHelper;
        public static MaxHelpingHandInterop MaxHelpingHand => _interopMaxHelpingHand;
        public static FactoryHelperInterop FactoryHelper => _interopFactoryHelper;
        public static ExtendedVariantModeInterop ExtendedVariants => _interopExtendedVariants;
        public static IReadOnlyList<Type> SidewaysJumpthruTypes
        {
            get
            {
                if (_sidewaysJumpthruTypes == null)
                    CacheModTypes();
                return _sidewaysJumpthruTypes;
            }
        }
        public static IReadOnlyList<Type> DownsideJumpthruTypes
        {
            get
            {
                if (_downsideJumpthruTypes == null)
                    CacheModTypes();
                return _downsideJumpthruTypes;
            }
        }
        public static Type ConveyorType
        {
            get
            {
                if (_conveyorType == null)
                    CacheModTypes();
                return _conveyorType;
            }
        }

        public static void Initialize()
        {
            typeof(AquaExports).ModInterop();
            _interopGravityHelper.Load();
            _interopExtendedVariants.Load();
        }

        public static void Uninitialize()
        {
        }

        private static void CacheModTypes()
        {
            Type downsideJumpthru1 = MaxHelpingHand.GetType("Celeste.Mod.MaxHelpingHand.Entities.UpsideDownJumpThru");
            Type downsideJumpthru2 = GravityHelper.GetType("Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru");
            var downsideJumpthruTypes = new List<Type>(2);
            if (downsideJumpthru1 != null)
                downsideJumpthruTypes.Add(downsideJumpthru1);
            if (downsideJumpthru2 != null)
                downsideJumpthruTypes.Add(downsideJumpthru2);
            _downsideJumpthruTypes = downsideJumpthruTypes.ToArray();
            Type sidewayJumpthru1 = MaxHelpingHand.GetType("Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru");
            var sidewaysJumpthruTypes = new List<Type>(1);
            if (sidewayJumpthru1 != null)
                sidewaysJumpthruTypes.Add(sidewayJumpthru1);
            _sidewaysJumpthruTypes = sidewaysJumpthruTypes.ToArray();
            _conveyorType = FactoryHelper.GetType("FactoryHelper.Entities.Conveyor");
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
        private static MaxHelpingHandInterop _interopMaxHelpingHand = new MaxHelpingHandInterop();
        private static FactoryHelperInterop _interopFactoryHelper = new FactoryHelperInterop();
        private static ExtendedVariantModeInterop _interopExtendedVariants = new ExtendedVariantModeInterop();
        private static Type[] _downsideJumpthruTypes;
        private static Type[] _sidewaysJumpthruTypes;
        private static Type _conveyorType;
    }
}
