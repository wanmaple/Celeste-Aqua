using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module
{
    public static class ModPatches
    {
        public static readonly HashSet<string> ConnectionEntityPatches = new HashSet<string>
        {
            "Celeste.Mod.CommunalHelper.Entities.DreamFloatySpaceBlock"
        };
    }
}
