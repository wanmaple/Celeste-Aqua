using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.Aqua.Module
{
    public class GravityHelperInterop
    {
        public bool IsLoaded => Everest.Loader.DependencyLoaded(_meta);

        [ModExportName("GravityHelper")]
        public static class GravityHelperImports
        {
            public static Func<bool> IsPlayerInverted;
        }

        public GravityHelperInterop()
        {
            _meta = new EverestModuleMetadata
            {
                Name = "GravityHelper",
                Version = new System.Version(1, 2, 21),
            };
        }

        public void Load()
        {
            typeof(GravityHelperImports).ModInterop();
        }

        public bool IsPlayerGravityInverted()
        {
            if (IsLoaded && GravityHelperImports.IsPlayerInverted != null)
                return GravityHelperImports.IsPlayerInverted.Invoke();
            return false;
        }

        private EverestModuleMetadata _meta;
    }
}
