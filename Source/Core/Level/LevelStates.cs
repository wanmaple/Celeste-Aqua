﻿using Celeste.Mod.Aqua.Debug;
using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelStates
    {
        [Serializable]
        public class LevelState
        {
            public const char EMPTY_TILE = '0';

            public bool FeatureEnabled { get; set; }
            public GrapplingHook.RopeMaterial RopeMaterial { get; set; }
            public int HookStyle { get; set; }
            public GrapplingHook.GameplayMode GameplayMode { get; set; }
            public int InitialShootCount { get; set; }
            public int RestShootCount { get; set; }
            public int MaxShootCount { get; set; }
            public bool ResetCountInTransition { get; set; }
            public bool DisableGrappleBoost { get; set; }
            public bool ShortDistanceGrappleBoost { get; set; }
            public bool Ungrapple16Direction { get; set; }
            public HookSettings HookSettings { get; set; }

            public HashSet<char> UnhookableTiletypes { get; private set; } = new HashSet<char>();

            public LevelState()
            {
            }

            public LevelState(AreaData areaData)
            {
                Reset(areaData);
            }

            public void Reset(AreaData areaData)
            {
                var meta = areaData.GetExtraMeta();
                if (meta == null)
                    meta = new LevelExtras();
                var extraMeta = meta.Value;
                FeatureEnabled = extraMeta.FeatureEnabled || AquaModule.Settings.FeatureEnabled;
                RopeMaterial = (GrapplingHook.RopeMaterial)extraMeta.HookMaterial;
                HookStyle = extraMeta.HookStyle;
                GameplayMode = (GrapplingHook.GameplayMode)extraMeta.GameplayMode;
                InitialShootCount = RestShootCount = extraMeta.InitialShootCount;
                MaxShootCount = extraMeta.MaxShootCount;
                ResetCountInTransition = extraMeta.ResetCountInTransition;
                DisableGrappleBoost = extraMeta.DisableGrappleBoost;
                ShortDistanceGrappleBoost = extraMeta.ShortDistanceGrappleBoost;
                Ungrapple16Direction = extraMeta.Ungrapple16Direction;
                HookSettings = extraMeta.HookSettings.Clone();
            }

            public bool IsTileBlocked(Level level, Vector2 position, bool xDir)
            {
                SolidTiles tiles = level.Tracker.GetEntity<SolidTiles>();
                Vector2 relativePos = position - tiles.Position;
                int coordX = (int)MathF.Floor(relativePos.X / 8.0f);
                int coordY = (int)MathF.Floor(relativePos.Y / 8.0f);
                if (xDir)
                {
                    char tiletype = level.SolidsData[coordX, coordY];
                    if (tiletype == EMPTY_TILE)
                    {
                        if (UnhookableTiletypes.Contains(level.SolidsData[coordX, coordY - 1]) || UnhookableTiletypes.Contains(level.SolidsData[coordX, coordY + 1]))
                            return true;
                    }
                    else if (UnhookableTiletypes.Contains(tiletype))
                        return true;
                }
                else
                {
                    char tiletype = level.SolidsData[coordX, coordY];
                    if (tiletype == EMPTY_TILE)
                    {
                        if (UnhookableTiletypes.Contains(level.SolidsData[coordX - 1, coordY]) || UnhookableTiletypes.Contains(level.SolidsData[coordX + 1, coordY]))
                            return true;
                    }
                    else if (UnhookableTiletypes.Contains(tiletype))
                        return true;
                }
                return false;
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
            AquaModule.Settings.DisableGrappleBoostChanged += self.AquaSettings_DisableGrappleBoostChanged;
            AquaModule.Settings.ShortDistanceGrappleBoostChanged += self.AquaSettings_ShortDistanceGrappleBoostChanged;
            AquaModule.Settings.Ungrapple16DirectionChanged += self.AquaSettings_Ungrapple16DirectionChanged;
            AquaModule.Settings.HookSettings.ParameterChanged += self.AquaHookSettings_ParameterChanged;
        }

        private static void Level_End(On.Celeste.Level.orig_End orig, Level self)
        {
            orig(self);
            AquaModule.Settings.FeatureEnableChanged -= self.AquaSettings_FeatureEnableChanged;
            AquaModule.Settings.DisableGrappleBoostChanged -= self.AquaSettings_DisableGrappleBoostChanged;
            AquaModule.Settings.ShortDistanceGrappleBoostChanged -= self.AquaSettings_ShortDistanceGrappleBoostChanged;
            AquaModule.Settings.Ungrapple16DirectionChanged -= self.AquaSettings_Ungrapple16DirectionChanged;
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
            var players = self.Tracker.GetEntities<Player>();
            if (players != null)
            {
                foreach (Player player in players)
                {
                    player.InitializeGrapplingHook(GrapplingHook.HOOK_SIZE, state.HookSettings.RopeLength, state.RopeMaterial, state.GameplayMode, state.InitialShootCount, state.HookStyle);
                }
            }
            if (state.ResetCountInTransition)
            {
                state.RestShootCount = state.InitialShootCount;
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
            LevelExtras? extras = areaData.GetExtraMeta();
            if (extras == null || !extras.Value.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    state.FeatureEnabled = enabled;
                }
            }
        }

        private static void AquaSettings_DisableGrappleBoostChanged(this Level self, bool value)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras? extras = areaData.GetExtraMeta();
            if (extras == null || !extras.Value.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    state.DisableGrappleBoost = value;
                }
            }
        }

        private static void AquaSettings_ShortDistanceGrappleBoostChanged(this Level self, bool value)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras? extras = areaData.GetExtraMeta();
            if (extras == null || !extras.Value.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    state.ShortDistanceGrappleBoost = value;
                }
            }
        }

        private static void AquaSettings_Ungrapple16DirectionChanged(this Level self, bool value)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras? extras = areaData.GetExtraMeta();
            if (extras == null || !extras.Value.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    state.Ungrapple16Direction = value;
                }
            }
        }

        private static void AquaHookSettings_ParameterChanged(this Level self, string parameter, int value)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras? extras = areaData.GetExtraMeta();
            if (extras == null || !extras.Value.DisableUserCustomParameters)
            {
                LevelState state = self.GetState();
                if (state != null)
                {
                    switch (parameter)
                    {
                        case "RopeLength":
                            state.HookSettings.RopeLength = value;
                            var players = self.Tracker.GetEntities<Player>();
                            if (players != null)
                            {
                                foreach (Player player in players)
                                {
                                    GrapplingHook hook = player.GetGrappleHook();
                                    if (hook != null)
                                    {
                                        hook.SetRopeLength(value);
                                    }
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
                        case "ActorPullForce":
                            state.HookSettings.ActorPullForce = value;
                            break;
                        case "SwingJumpStaminaCost":
                            state.HookSettings.SwingJumpStaminaCost = value;
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
#if DEBUG
            if (MInput.Keyboard.Pressed(Keys.T))
            {
                string testYaml = "grapple_test";
                if (Everest.Content.TryGet(testYaml, out ModAsset asset) && asset.TryDeserialize(out GrappleUnitTest test))
                {
                    HookRope rope = new HookRope(90.0f, GrapplingHook.RopeMaterial.Default);
                    rope.DebugTest(test);
                }
            }
#endif
        }

        public static void SyncPropertyIfPossible(this Level self, Action<LevelState> action)
        {
            AreaData areaData = AreaData.Get(self.Session.Area);
            LevelExtras? extras = areaData.GetExtraMeta();
            LevelState state = self.GetState();
            if ((extras == null || !extras.Value.DisableUserCustomParameters) && state != null)
            {
                action?.Invoke(state);
            }
        }

        private static readonly Dictionary<string, KeyValuePair<Type, Type>> BACKGROUND_HINTS = new Dictionary<string, KeyValuePair<Type, Type>>
        {
        };
    }
}
