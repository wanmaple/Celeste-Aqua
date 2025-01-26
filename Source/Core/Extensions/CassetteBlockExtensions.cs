using Celeste.Mod.Entities;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class CassetteBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.CassetteBlock.BlockedCheck += CassetteBlock_BlockedCheck;
            //On.Celeste.CassetteBlock.SetActivatedSilently += CassetteBlock_SetActivatedSilently;
            //On.Celeste.CassetteBlock.WillToggle += CassetteBlock_WillToggle;
            //On.Celeste.CassetteBlockManager.SetActiveIndex += CassetteBlockManager_SetActiveIndex;
        }

        public static void Uninitialize()
        {
            On.Celeste.CassetteBlock.BlockedCheck -= CassetteBlock_BlockedCheck;
            //On.Celeste.CassetteBlock.SetActivatedSilently -= CassetteBlock_SetActivatedSilently;
            //On.Celeste.CassetteBlock.WillToggle -= CassetteBlock_WillToggle;
            //On.Celeste.CassetteBlockManager.SetActiveIndex -= CassetteBlockManager_SetActiveIndex;
        }

        private static bool CassetteBlock_BlockedCheck(On.Celeste.CassetteBlock.orig_BlockedCheck orig, CassetteBlock self)
        {
            if (orig(self))
                return true;
            if (self.CollideFirst<GrapplingHook>() != null)
                return true;
            if (self.IntersectsWithRope())
                return true;
            return false;
        }

        private static void CassetteBlock_SetActivatedSilently(On.Celeste.CassetteBlock.orig_SetActivatedSilently orig, CassetteBlock self, bool activated)
        {
            orig(self, activated);
            self.SetCassetteActive(activated);
        }

        private static void CassetteBlock_WillToggle(On.Celeste.CassetteBlock.orig_WillToggle orig, CassetteBlock self)
        {
            var state = self.SceneAs<Level>().GetState();
            if (state != null && state.FeatureEnabled)
            {
                self.ShiftSize(self.IsCassetteActive() ? 1 : (-1));
                self.UpdateVisualState();
            }
            else
            {
                orig(self);
            }
        }

        private static void CassetteBlockManager_SetActiveIndex(On.Celeste.CassetteBlockManager.orig_SetActiveIndex orig, CassetteBlockManager self, int index)
        {
            var state = self.SceneAs<Level>().GetState();
            if (state != null && state.FeatureEnabled)
            {
                foreach (CassetteBlock entity in self.Scene.Tracker.GetEntities<CassetteBlock>())
                {
                    entity.Activated = entity.Index == index;
                    entity.SetCassetteActive(entity.Index == index);
                }

                foreach (CassetteListener component in self.Scene.Tracker.GetComponents<CassetteListener>())
                {
                    component.SetActivated(component.Index == index);
                }
            }
            else
            {
                orig(self, index);
            }
        }

        public static bool IsCassetteActive(this CassetteBlock self)
        {
            return DynamicData.For(self).Get<bool>("cassette_active");
        }

        private static void SetCassetteActive(this CassetteBlock self, bool active)
        {
            DynamicData.For(self).Set("cassette_active", active);
        }
    }
}
