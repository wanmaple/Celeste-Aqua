using Celeste.Mod.Aqua.Debug;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelExtrasLoader
    {
        public static void Initialize()
        {
            On.Celeste.AreaData.Load += AreaData_Load;
        }

        public static void Uninitialize()
        {
            On.Celeste.AreaData.Load -= AreaData_Load;
        }

        private static void AreaData_Load(On.Celeste.AreaData.orig_Load orig)
        {
            orig();
            foreach (AreaData map in AreaData.Areas)
            {
                string extraMetaPath = "Maps/" + map.Mode[0].Path + ".extras.meta";
                if (!Everest.Content.TryGet(extraMetaPath, out ModAsset metadata) || !metadata.TryDeserialize(out LevelExtras meta))
                {
                    map.SetExtraMeta(new LevelExtras());
                    continue;
                }
                AquaDebugger.LogInfo("Extra meta of map {0} is found.", map.Mode[0].Path);
                map.SetExtraMeta(meta);
            }
        }

        public static LevelExtras GetExtraMeta(this AreaData self)
        {
            return DynamicData.For(self).Get<LevelExtras>("extra_meta");
        }

        public static void SetExtraMeta(this AreaData self, LevelExtras extras)
        {
            DynamicData.For(self).Set("extra_meta", extras);
        }
    }
}
