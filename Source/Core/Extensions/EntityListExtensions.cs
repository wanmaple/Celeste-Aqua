using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityListExtensions
    {
        public static void Initialize()
        {
            On.Monocle.EntityList.Update += EntityList_Update;
        }

        public static void Uninitialize()
        {
            On.Monocle.EntityList.Update -= EntityList_Update;
        }

        private static void EntityList_Update(On.Monocle.EntityList.orig_Update orig, EntityList self)
        {
            orig(self);
            if (self.Scene != null)
            {
                var grapples = self.Scene.Tracker.GetEntities<GrapplingHook>();
                foreach (GrapplingHook grapple in grapples)
                {
                    grapple.UpdateLast();
                }
            }
        }
    }
}
