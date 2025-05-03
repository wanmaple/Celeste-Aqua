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
        public static CommunalHelperInterop CommunalHelper => _interopCommunalHelper;
        public static FrostHelperInterop FrostHelper => _interopFrostHelper;
        public static FlaglinesAndSuchInterop FlaglinesAndSuch => _interopFlaglines;
        public static VivHelperInterop VivHelper => _interopVivHelper;
        public static VortexHelperInterop VortexHelper => _interopVortexHelper;
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
        public static IReadOnlyList<Type> CurvedBoosterTypes
        {
            get
            {
                if (_curvedBoosterTypes == null)
                    CacheModTypes();
                return _curvedBoosterTypes;
            }
        }
        public static IReadOnlyList<Type> SpringTypes
        {
            get
            {
                if (_springTypes == null)
                    CacheModTypes();
                return _springTypes;
            }
        }

        public static IReadOnlyList<Type> ElectricEntityTypes
        {
            get
            {
                if (_electricEntityTypes == null)
                    CacheModTypes();
                return _electricEntityTypes;
            }
        }

        public static IReadOnlyList<Type> BumperTypes
        {
            get
            {
                if (_bumperTypes == null)
                    CacheModTypes();
                return _bumperTypes;
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

        public static Type HoldableContainerType
        {
            get
            {
                if (_holdableContainerType == null)
                    CacheModTypes();
                return _holdableContainerType;
            }
        }

        public static Type ContainerRefType
        {
            get
            {
                if (_containerRefType == null)
                    CacheModTypes();
                return _containerRefType;
            }
        }

        public static Type ContainerMoverType
        {
            get
            {
                if (_containerMoverType == null)
                    CacheModTypes();
                return _containerMoverType;
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

        public static Type PlatformJellyType
        {
            get
            {
                if (_platformJellyType == null)
                    CacheModTypes();
                return _platformJellyType;
            }
        }

        public static Type HoldableBarrierType
        {
            get
            {
                if (_holdableBarrierType == null)
                    CacheModTypes();
                return _holdableBarrierType;
            }
        }

        public static Type HoldableBarrierJumpThruType
        {
            get
            {
                if (_holdableBarrierJumpThruType == null)
                    CacheModTypes();
                return _holdableBarrierJumpThruType;
            }
        }

        public static Type AttachedJumpThroughType
        {
            get
            {
                if (_attachJumpThruType == null)
                    CacheModTypes();
                return _attachJumpThruType;
            }
        }

        public static Type VortexBumperType
        {
            get
            {
                if (_vortexBumperType == null)
                    CacheModTypes();
                return _vortexBumperType;
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
            var curvedBoosterTypes = new List<Type>(3);
            Type curvedBooster = CommunalHelper.GetType("Celeste.Mod.CommunalHelper.Entities.CurvedBooster");
            Type spiralBooster = CommunalHelper.GetType("Celeste.Mod.CommunalHelper.Entities.Boosters.SpiralBooster");
            Type dreamBooster = CommunalHelper.GetType("Celeste.Mod.CommunalHelper.Entities.DreamBooster");
            if (curvedBooster != null)
                curvedBoosterTypes.Add(curvedBooster);
            if (spiralBooster != null)
                curvedBoosterTypes.Add(spiralBooster);
            if (dreamBooster != null)
                curvedBoosterTypes.Add(dreamBooster);
            _curvedBoosterTypes = curvedBoosterTypes.ToArray();
            var springTypes = new List<Type>(4);
            Type customSpring = FrostHelper.GetType("FrostHelper.CustomSpring");
            Type gravitySpring = GravityHelper.GetType("Celeste.Mod.GravityHelper.Entities.GravitySpring");
            Type momentumSpring = GravityHelper.GetType("Celeste.Mod.GravityHelper.Entities.MomentumSpring");
            Type noDashSpring = MaxHelpingHand.GetType("Celeste.Mod.MaxHelpingHand.Entities.NoDashRefillSpring");
            if (customSpring != null)
                springTypes.Add(customSpring);
            if (gravitySpring != null)
                springTypes.Add(gravitySpring);
            if (momentumSpring != null)
                springTypes.Add(momentumSpring);
            if (noDashSpring != null)
                springTypes.Add(noDashSpring);
            _springTypes = springTypes.ToArray();
            var elecEntityTypes = new List<Type>(4);
            Type elecSpike = FactoryHelper.GetType("FactoryHelper.Entities.ElectrifiedWall");
            if (elecSpike != null)
                elecEntityTypes.Add(elecSpike);
            _electricEntityTypes = elecEntityTypes.ToArray();
            var bumperTypes = new List<Type>(4);
            Type staticBumper = FrostHelper.GetType("FrostHelper.StaticBumper");
            _vortexBumperType = VortexHelper.GetType("Celeste.Mod.VortexHelper.Entities.VortexBumper");
            Type dashBumper = VivHelper.GetType("VivHelper.Entities.DashBumper");
            if (staticBumper != null)
                bumperTypes.Add(staticBumper);
            if (_vortexBumperType != null)
                bumperTypes.Add(_vortexBumperType);
            if (dashBumper != null)
                bumperTypes.Add(dashBumper);
            _bumperTypes = bumperTypes.ToArray();
            _conveyorType = FactoryHelper.GetType("FactoryHelper.Entities.Conveyor");
            _holdableContainerType = EeveeHelper.GetType("Celeste.Mod.EeveeHelper.Entities.HoldableContainer");
            _containerRefType = EeveeHelper.GetType("Celeste.Mod.EeveeHelper.Components.ContainerRefComponent");
            _containerMoverType = EeveeHelper.GetType("Celeste.Mod.EeveeHelper.Components.EntityContainerMover");
            _cardinalBumperType = JackalHelper.GetType("Celeste.Mod.JackalHelper.Entities.CardinalBumper");
            _platformJellyType = FlaglinesAndSuch.GetType("FlaglinesAndSuch.PlatformJelly");
            _holdableBarrierType = VivHelper.GetType("VivHelper.Entities.HoldableBarrier");
            _holdableBarrierJumpThruType = VivHelper.GetType("VivHelper.Entities.HoldableBarrierJumpThru");
            _attachJumpThruType = VortexHelper.GetType("Celeste.Mod.VortexHelper.Entities.AttachedJumpThru");
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
        private static MaxHelpingHandInterop _interopMaxHelpingHand = new MaxHelpingHandInterop();
        private static FactoryHelperInterop _interopFactoryHelper = new FactoryHelperInterop();
        private static ExtendedVariantModeInterop _interopExtendedVariants = new ExtendedVariantModeInterop();
        private static EeveeHelperInterop _interopEeveeHelper = new EeveeHelperInterop();
        private static JackalHelperInterop _interopJackalHelper = new JackalHelperInterop();
        private static CommunalHelperInterop _interopCommunalHelper = new CommunalHelperInterop();
        private static FrostHelperInterop _interopFrostHelper = new FrostHelperInterop();
        private static FlaglinesAndSuchInterop _interopFlaglines = new FlaglinesAndSuchInterop();
        private static VivHelperInterop _interopVivHelper = new VivHelperInterop();
        private static VortexHelperInterop _interopVortexHelper = new VortexHelperInterop();
        private static Type[] _downsideJumpthruTypes;
        private static Type[] _sidewaysJumpthruTypes;
        private static Type[] _curvedBoosterTypes;
        private static Type[] _springTypes;
        private static Type[] _electricEntityTypes;
        private static Type[] _bumperTypes;
        private static Type _conveyorType;
        private static Type _holdableContainerType;
        private static Type _containerRefType;
        private static Type _containerMoverType;
        private static Type _cardinalBumperType;
        private static Type _platformJellyType;
        private static Type _holdableBarrierType;
        private static Type _holdableBarrierJumpThruType;
        private static Type _attachJumpThruType;
        private static Type _vortexBumperType;
    }
}
