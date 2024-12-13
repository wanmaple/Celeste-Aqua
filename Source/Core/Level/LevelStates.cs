using Celeste.Mod.Aqua.Module;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelStates
    {
        public class LevelState
        {
            public bool AutoGrabHookRope { get; set; }
            public bool FeatureEnabled { get; set; }
            public GrapplingHook.RopeMaterial RopeMaterial { get; set; }

            public LevelState(AreaData areaData)
            {
                Reset(areaData);
            }

            public void Reset(AreaData areaData)
            {
                AutoGrabHookRope = AquaModule.Settings.AutoGrabRopeIfPossible;
                FeatureEnabled = areaData.GetExtraMeta().FeatureEnabled;
                RopeMaterial = (GrapplingHook.RopeMaterial)areaData.GetExtraMeta().HookMaterial;
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
            return DynamicData.For(self).Get<LevelState>("state");
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            LevelState state = self.GetState();
            if (state == null)
            {
                AreaData areaData = AreaData.Get(self.Session.Area);
                state = new LevelState(areaData);
                DynamicData.For(self).Set("state", state);
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
