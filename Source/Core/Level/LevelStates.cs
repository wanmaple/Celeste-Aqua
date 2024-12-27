using Celeste.Mod.Aqua.Module;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelStates
    {
        [Serializable]
        public class LevelState
        {
            public bool AutoGrabHookRope { get; set; }
            public bool FeatureEnabled { get; set; }
            public GrapplingHook.RopeMaterial RopeMaterial { get; set; }
            public GrapplingHook.GameplayMode GameplayMode { get; set; }
            public int InitialShootCount { get; set; }
            public int RestShootCount { get; set; }
            public HookSettings HookSettings { get; set; }

            public LevelState()
            {
            }

            public LevelState(AreaData areaData)
            {
                Reset(areaData);
            }

            public void Reset(AreaData areaData)
            {
                AutoGrabHookRope = AquaModule.Settings.AutoGrabRopeIfPossible;
                FeatureEnabled = areaData.GetExtraMeta().FeatureEnabled || AquaModule.Settings.FeatureEnabled;
                RopeMaterial = (GrapplingHook.RopeMaterial)areaData.GetExtraMeta().HookMaterial;
                GameplayMode = (GrapplingHook.GameplayMode)areaData.GetExtraMeta().GameplayMode;
                InitialShootCount = RestShootCount = areaData.GetExtraMeta().InitialShootCount;
                HookSettings = areaData.GetExtraMeta().HookSettings.Clone();
            }
        }

        public static void Initialize()
        {
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            On.Celeste.Level.Update += Level_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            On.Celeste.Level.Update -= Level_Update;
        }

        public static LevelState GetState(this Level self)
        {
            return AquaModule.Session.levelState;
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            LevelState state = AquaModule.Session.levelState;
            if (state == null)
            {
                AreaData areaData = AreaData.Get(self.Session.Area);
                state = new LevelState(areaData);
                AquaModule.Session.levelState = state;
            }
        }

        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (AquaModule.Settings.SwitchAutoGrab.Pressed)
            {
                var state = self.GetState();
                state.AutoGrabHookRope = !state.AutoGrabHookRope;
            }
        }
    }
}
