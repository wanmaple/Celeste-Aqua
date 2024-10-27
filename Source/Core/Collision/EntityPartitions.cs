using Celeste.Mod.Aqua.Module;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityPartitions
    {
        public static void Initialize()
        {
            AquaModule.Instance.HookCenter.BeforeLoadLevel += BeforeLoadLevel;
            AquaModule.Instance.HookCenter.AfterLevelEnd += AfterLevelEnd;
            On.Monocle.Tracker.EntityAdded += Tracker_EntityAdded;
            On.Monocle.Tracker.EntityRemoved += Tracker_EntityRemoved;
            On.Monocle.EntityList.ClearEntities += EntityList_ClearEntities;
        }

        public static void Uninitialize()
        {
            AquaModule.Instance.HookCenter.BeforeLoadLevel -= BeforeLoadLevel;
            AquaModule.Instance.HookCenter.AfterLevelEnd -= AfterLevelEnd;
            On.Monocle.Tracker.EntityAdded -= Tracker_EntityAdded;
            On.Monocle.Tracker.EntityRemoved -= Tracker_EntityRemoved;
            On.Monocle.EntityList.ClearEntities -= EntityList_ClearEntities;
        }

        private static void BeforeLoadLevel(Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (_space.IsValid)
            {
                _space.Clear();
            }
            _space.Bounds = self.Bounds;
            _duringLevel = true;
        }

        private static void AfterLevelEnd(Level self)
        {
            _space.Clear();
            _duringLevel = false;
        }

        private static void Tracker_EntityAdded(On.Monocle.Tracker.orig_EntityAdded orig, Monocle.Tracker self, Monocle.Entity entity)
        {
            orig(self, entity);
            if (_duringLevel)
            {
                _space.AddEntity(entity);
            }
        }

        private static void Tracker_EntityRemoved(On.Monocle.Tracker.orig_EntityRemoved orig, Monocle.Tracker self, Monocle.Entity entity)
        {
            orig(self, entity);
            if (_duringLevel)
            {
                _space.RemoveEntity(entity);
            }
        }

        private static void EntityList_ClearEntities(On.Monocle.EntityList.orig_ClearEntities orig, EntityList self)
        {
            if (_duringLevel)
            {
                foreach (Entity e in self.entities)
                {
                    _space.RemoveEntity(e);
                }
            }
            orig(self);
        }

        private static QuadTree _space = new QuadTree();
        private static bool _duringLevel = false;
    }
}
