using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Module
{ 
    public abstract class Interop
    {
        public bool IsLoaded => Everest.Loader.DependencyLoaded(_meta);

        protected Interop(string modName, int major, int minor, int build)
        {
            _meta = new EverestModuleMetadata
            {
                Name = modName,
                Version = new Version(major, minor, build),
            };
        }

        public Type GetType(string typeName)
        {
            if (_meta == null)
                return null;
            if (Everest.Loader.TryGetDependency(_meta, out EverestModule module))
            {
                Assembly asm = module.GetType().Assembly;
                Type type = asm.GetType(typeName);
                return type;
            }
            return null;
        }

        protected EverestModuleMetadata _meta;
    }
}
