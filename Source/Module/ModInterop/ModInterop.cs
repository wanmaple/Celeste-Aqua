using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module
{
    public static class ModInterop
    {
        public static GravityHelperInterop GravityHelper => _interopGravityHelper;
        public static MaxHelpingHandInterop MaxHelpingHand => _interopMaxHelpingHand;
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

        public static void Initialize()
        {
            typeof(AquaExports).ModInterop();
            _interopGravityHelper.Load();
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
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
        private static MaxHelpingHandInterop _interopMaxHelpingHand = new MaxHelpingHandInterop();
        private static Type[] _downsideJumpthruTypes;
        private static Type[] _sidewaysJumpthruTypes;
    }
}
