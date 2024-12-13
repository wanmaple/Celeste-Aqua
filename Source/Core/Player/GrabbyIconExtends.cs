using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
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
            if (state.FeatureEnabled)
            {
                Vector2 scale = Vector2.One * (1f + self.wiggler.Value * 0.2f);
                Player entity = self.Scene.Tracker.GetEntity<Player>();
                if (!self.SceneAs<Level>().InCutscene && entity != null && !entity.Dead)
                {
                    string texture = state.AutoGrabHookRope ? "util/grab_rope" : "util/ungrab_rope";
                    float x = self.enabled ? 6.0f : 0.0f;
                    GFX.Game[texture].DrawJustified(new Vector2(entity.X + x, entity.Y - 16.0f), new Vector2(0.5f, 1.0f), Color.White, scale);
                }
            }
        }

        private static float CalculateOffset(this GrabbyIcon self)
        {
            LevelStates.LevelState state = self.SceneAs<Level>().GetState();
            if (state.FeatureEnabled && self.enabled)
            {
                return -6.0f;
            }
            return 0.0f;
        }
    }
}
