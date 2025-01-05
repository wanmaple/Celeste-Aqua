using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module
{
    public static class ModInterop
    {
        public static GravityHelperInterop GravityHelper => _interopGravityHelper;

        public static void Initialize()
        {
            _interopGravityHelper.Load();
        }

        public static void Uninitialize()
        {
        }

        private static GravityHelperInterop _interopGravityHelper = new GravityHelperInterop();
    }
}
