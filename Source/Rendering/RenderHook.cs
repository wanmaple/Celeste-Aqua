using Celeste.Mod.Aqua.Debug;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.Aqua.Rendering
{
    public static class RenderHook
    {
        static RenderHook()
        {
            _methodRender = typeof(Renderer).GetMethod("Render", BindingFlags.Instance | BindingFlags.Public);
        }

        public static void Initialize()
        {
            IL.Celeste.Level.Render += Level_ILRender;
            IL.Monocle.EntityList.RenderExcept += EntityList_ILRenderExcept;
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
            On.Celeste.Tags.Initialize += Tags_Initialize;
            On.Monocle.Scene.ctor += Scene_Construct;
            On.Monocle.Scene.Update += Scene_Update;
        }

        public static void Uninitialize()
        {
            IL.Celeste.Level.Render -= Level_ILRender;
            IL.Monocle.EntityList.RenderExcept -= EntityList_ILRenderExcept;
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
            On.Celeste.Tags.Initialize -= Tags_Initialize;
            On.Monocle.Scene.ctor -= Scene_Construct;
            On.Monocle.Scene.Update -= Scene_Update;
        }

        private static void Level_ILRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchCallvirt(_methodRender)))
            {
                cursor.Index -= 3;
                cursor.EmitLdarg0();
                cursor.EmitDelegate(RenderBackground);
                // 有很多限制，由于GameplayRenderer并没有写深度，我只能靠后画了，因此可能盖掉其他非Custom的Entity。
                //cursor.Index += 4;
                //cursor.EmitLdarg0();
                //cursor.EmitDelegate(RenderCustomEntities);
            }
        }

        private static void EntityList_ILRenderExcept(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdloc1(), ins => ins.MatchLdarg1()))
            {
                cursor.Index += 2;
                cursor.EmitDelegate(ExtraExcludeTags);
                cursor.EmitOr();
            }
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            var bgRender = new CustomBackgroundRenderer();
            DynamicData.For(self.Level).Set("custom_bg_renderer", bgRender);
            self.Level.Add(bgRender);
        }

        private static void Tags_Initialize(On.Celeste.Tags.orig_Initialize orig)
        {
            orig();
            RenderTags.CustomBackground = new BitTag("CustomBackground");
            RenderTags.CustomPostProcessing = new BitTag("CustomPostProcessing");
            RenderTags.All = RenderTags.CustomBackground | RenderTags.CustomPostProcessing;
        }

        private static void Scene_Construct(On.Monocle.Scene.orig_ctor orig, Scene self)
        {
            orig(self);
            DynamicData.For(self).Set("time", 0.0f);
        }

        private static void Scene_Update(On.Monocle.Scene.orig_Update orig, Scene self)
        {
            orig(self);
            float time = self.GetTime();
            time += Engine.DeltaTime;
            DynamicData.For(self).Set("time", time);
        }

        private static void RenderBackground(this Level self)
        {
            CustomBackgroundRenderer bgRenderer = DynamicData.For(self).Get<CustomBackgroundRenderer>("custom_bg_renderer");
            if (bgRenderer != null)
            {
                bgRenderer.Render(self);
            }
        }

        private static int ExtraExcludeTags()
        {
            return RenderTags.All;
        }

        public static float GetTime(this Scene self)
        {
            return DynamicData.For(self).Get<float>("time");
        }

        private static MethodInfo _methodRender;
    }
}
