using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.Aqua.Core
{
    public static class GrabbyIconExtends
    {
        public static void Initialize()
        {
            IL.Celeste.GrabbyIcon.Render += GrabbyIcon_ILRender;
            On.Celeste.GrabbyIcon.Render += GrabbyIcon_Render;
        }

        public static void Uninitialize()
        {
            IL.Celeste.GrabbyIcon.Render -= GrabbyIcon_ILRender;
            On.Celeste.GrabbyIcon.Render -= GrabbyIcon_Render;
        }

        private static void GrabbyIcon_ILRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdstr("util/glove")))
            {
                cursor.Index += 4;
                cursor.EmitLdarg0();
                cursor.EmitDelegate(CalculateOffset);
                cursor.EmitAdd();
            }
        }

        private static void GrabbyIcon_Render(On.Celeste.GrabbyIcon.orig_Render orig, GrabbyIcon self)
        {
            orig(self);
            LevelStates.LevelState state = self.SceneAs<Level>().GetState();
            int num = 0;
            if (state.FeatureEnabled && AquaModule.Settings.AutoGrabRopeIfPossible)
            {
                ++num;
            }
            if (state.FeatureEnabled && state.GameplayMode == GrapplingHook.GameplayMode.ShootCounter)
            {
                ++num;
            }
            if (num > 0)
            {
                Vector2 scale = Vector2.One * (1f + self.wiggler.Value * 0.2f);
                Player entity = self.Scene.Tracker.GetEntity<Player>();
                float yFlip = ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
                float yExtra = ModInterop.GravityHelper.IsPlayerGravityInverted() ? entity.Height : 0.0f;
                if (state.FeatureEnabled && AquaModule.Settings.AutoGrabRopeIfPossible)
                {
                    if (!self.SceneAs<Level>().InCutscene && entity != null && !entity.Dead)
                    {
                        string texture = AquaModule.Settings.AutoGrabRopeIfPossible ? "util/grab_rope" : "util/ungrab_rope";
                        float x = self.enabled ? (2 - num) * 6.0f : -6.0f * (num - 1);
                        if (AquaModule.Settings.AutoGrabRopeIfPossible)
                        {
                            GFX.Game[texture].DrawJustified(new Vector2(entity.X + x, entity.Y - 16.0f * yFlip + yExtra), new Vector2(0.5f, 1.0f), Color.White, scale);
                        }
                    }
                }
                if (state.FeatureEnabled && state.GameplayMode == GrapplingHook.GameplayMode.ShootCounter)
                {
                    if (!self.SceneAs<Level>().InCutscene && entity != null && !entity.Dead)
                    {
                        MTexture texture = GFX.Game["util/hook_count"];
                        float x = self.enabled ? 6.0f * num : 6.0f * (num - 1);
                        texture.DrawJustified(new Vector2(entity.X + x, entity.Y - 16.0f * yFlip + yExtra), new Vector2(0.5f, 1.0f), Color.White, scale);
                        int cnt = state.RestShootCount;
                        string numDisplay = cnt.ToString();
                        int len = numDisplay.Length;
                        for (int i = 0; i < len; i++)
                        {
                            MTexture texNum = NumberAtlas.Instance.GetNumberTextureForCharacter(numDisplay[i]);
                            float offset = len % 2 == 0 ? -len * 2.0f + 2.0f + i * 4.0f : (len / 2) * -4.0f + i * 4.0f;
                            texNum.DrawOutlineCentered(new Vector2(entity.X + x + 4.0f + offset, entity.Y - 20.0f * yFlip + yExtra), Color.White, Vector2.One);
                        }
                    }
                }
            }
        }

        private static float CalculateOffset(this GrabbyIcon self)
        {
            LevelStates.LevelState state = self.SceneAs<Level>().GetState();
            int num = 0;
            if (state.FeatureEnabled && AquaModule.Settings.AutoGrabRopeIfPossible)
            {
                ++num;
            }
            if (state.FeatureEnabled && state.GameplayMode == GrapplingHook.GameplayMode.ShootCounter)
            {
                ++num;
            }
            if (num > 0)
            {
                return -6.0f * num;
            }
            return 0.0f;
        }
    }
}
