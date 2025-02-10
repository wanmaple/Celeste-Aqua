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
        public static EeveeHelperInterop EeveeHelper => _interopEeveeHelper;
        public static JackalHelperInterop JackalHelper => _interopJackalHelper;
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

        public static Type HoldableType
        {
            get
            {
                if (_holdableContainerType == null)
                    CacheModTypes();
                return _holdableContainerType;
            }
        }

        public static Type CardinalBumperType
        {
            get
            {
                if (_cardinalBumperType == null)
                    CacheModTypes();
                return _cardinalBumperType;
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
            _holdableContainerType = EeveeHelper.GetType("Celeste.Mod.EeveeHelper.Entities.HoldableContainer");
            _cardinalBumperType = JackalHelper.GetType("Celeste.Mod.JackalHelper.Entities.CardinalBumper");
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
        private static MaxHelpingHandInterop _interopMaxHelpingHand = new MaxHelpingHandInterop();
        private static FactoryHelperInterop _interopFactoryHelper = new FactoryHelperInterop();
        private static ExtendedVariantModeInterop _interopExtendedVariants = new ExtendedVariantModeInterop();
        private static EeveeHelperInterop _interopEeveeHelper = new EeveeHelperInterop();
        private static JackalHelperInterop _interopJackalHelper = new JackalHelperInterop();
        private static Type[] _downsideJumpthruTypes;
        private static Type[] _sidewaysJumpthruTypes;
        private static Type _conveyorType;
        private static Type _holdableContainerType;
        private static Type _cardinalBumperType;
    }
}
