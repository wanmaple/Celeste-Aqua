using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelStates
    {
        [Serializable]
        public class LevelState
        {
            public bool FeatureEnabled { get; set; }
            public GrapplingHook.RopeMaterial RopeMaterial { get; set; }
            public GrapplingHook.GameplayMode GameplayMode { get; set; }
            public int InitialShootCount { get; set; }
            public int RestShootCount { get; set; }
            public HookSettings HookSettings { get; set; }
            public List<BackgroundData> Backgrounds { get; private set; }

            public LevelState()
            {
            }

            public LevelState(AreaData areaData)
            {
                Reset(areaData);
            }

            public void Reset(AreaData areaData)
            {
                FeatureEnabled = areaData.GetExtraMeta().FeatureEnabled || AquaModule.Settings.FeatureEnabled;
                RopeMaterial = (GrapplingHook.RopeMaterial)areaData.GetExtraMeta().HookMaterial;
                GameplayMode = (GrapplingHook.GameplayMode)areaData.GetExtraMeta().GameplayMode;
                InitialShootCount = RestShootCount = areaData.GetExtraMeta().InitialShootCount;
                HookSettings = areaData.GetExtraMeta().HookSettings.Clone();
                Backgrounds = new List<BackgroundData>(areaData.GetExtraMeta().Backgrounds);
            }
        }

        public static void Initialize()
        {
            On.Celeste.Level.Begin += Level_Begin;
            On.Celeste.Level.End += Level_End;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            On.Celeste.Level.UnloadLevel += Level_UnloadLevel;
            On.Celeste.Level.Update += Level_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Level.Begin -= Level_Begin;
            On.Celeste.Level.End -= Level_End;
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            On.Celeste.Level.UnloadLevel -= Level_UnloadLevel;
            On.Celeste.Level.Update -= Level_Update;
        }

        public static LevelState GetState(this Level self)
        {
            return AquaModule.Session.levelState;
        }

        private static void Level_Begin(On.Celeste.Level.orig_Begin orig, Level self)
        {
            orig(self);
            AquaModule.Settings.FeatureEnableChanged += self.AquaSettings_FeatureEnableChanged;
            AquaModule.Settings.HookSettings.ParameterChanged += self. AquaHookSettings_ParameterChanged;
        }

        private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
            AquaModule.Settings.FeatureEnableChanged -= self.AquaSettings_FeatureEnableChanged;
            AquaModule.Settings.HookSettings.ParameterChanged -= self.AquaHookSettings_ParameterChanged;
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
            Player player = self.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.InitializeGrapplingHook(GrapplingHook.HOOK_SIZE, state.HookSettings.RopeLength, state.RopeMaterial, state.GameplayMode, state.InitialShootCount);
            }
            if (state.Backgrounds != null)
            {
                foreach (BackgroundData bgData in state.Backgrounds)
                {
                    if (BACKGROUND_HINTS.TryGetValue(bgData.EntityName, out var pair))
                    {
                        Type bgType = pair.Key;
                        if (self.Entities.FirstOrDefault(e => e.GetType() == bgType) == null)
                        {
                            Type paramType = pair.Value;
                            object args = Activator.CreateInstance(paramType);
                            var method = paramType.GetMethod("Parse", BindingFlags.Instance | BindingFlags.Public);
                            if (method != null)
                            {
                                method.Invoke(args, new object[] { bgData.Uniforms, });
                            }
                            var entity = Activator.CreateInstance(bgType, new object[] { args, }) as Entity;
                            self.Add(entity);
                        }
                    }
                }
            }
        }

        private static void Level_UnloadLevel(On.Celeste.Level.orig_UnloadLevel orig, Level self)
        {
            orig(self);
            RenderInfoStorage.Instance.Clear();
        }

        private static void AquaSettings_FeatureEnableChanged(this Level self, bool enabled)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras extras = areaData.GetExtraMeta();
            if (!extras.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    state.FeatureEnabled = enabled;
                }
            }
        }

        private static void AquaHookSettings_ParameterChanged(this Level self, string parameter, int value)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras extras = areaData.GetExtraMeta();
            if (!extras.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    switch (parameter)
                    {
                        case "RopeLength":
                            state.HookSettings.RopeLength = value;
                            Player player = self.Tracker.GetEntity<Player>();
                            if (player != null)
                            {
                                GrapplingHook hook = player.GetGrappleHook();
                                if (hook != null)
                                {
                                    hook.SetRopeLength(value);
                                }
                            }
                            break;
                        case "EmitSpeed":
                            state.HookSettings.EmitSpeed = value;
                            break;
                        case "MaxLineSpeed":
                            state.HookSettings.MaxLineSpeed = value;
                            break;
                        case "FlyTowardSpeed":
                            state.HookSettings.FlyTowardSpeed = value;
                            break;
                    }
                }
            }
        }

        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (!self.FrozenOrPaused && AquaModule.Settings.SwitchAutoGrab.Pressed)
            {
                AquaModule.Settings.AutoGrabRopeIfPossible = !AquaModule.Settings.AutoGrabRopeIfPossible;
            }
        }

        public static void SyncPropertyIfPossible(this Level self, Action<LevelState> action)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras extras = areaData.GetExtraMeta();
            LevelState state = self.GetState();
            if (!extras.DisableUserCustomParameters && state != null)
            {
                action?.Invoke(state);
            }
        }

        private static readonly Dictionary<string, KeyValuePair<Type, Type>> BACKGROUND_HINTS = new Dictionary<string, KeyValuePair<Type, Type>> {
            { "AndromedaField", KeyValuePair.Create(typeof(AndromedaField), typeof(AndromedaFieldParameters))},
            {"SelfCircuit", KeyValuePair.Create(typeof(SelfCircuit), typeof(SelfCircuitParameters))},
            };
    }
}
