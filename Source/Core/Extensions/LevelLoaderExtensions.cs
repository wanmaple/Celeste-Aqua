using MonoMod.Cil;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class LevelLoaderExtensions
    {
        static LevelLoaderExtensions()
        {
            _methodCreateFramesMetadata = typeof(PlayerSprite).GetMethod("CreateFramesMetadata", BindingFlags.Public | BindingFlags.Static);
        }

        public static void Initialize()
        {
            IL.Celeste.LevelLoader.ctor += LevelLoader_Construct;
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Uninitialize()
        {
            IL.Celeste.LevelLoader.ctor -= LevelLoader_Construct;
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void LevelLoader_Construct(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdstr("player_playback")))
            {
                cursor.Index += 2;
                ILCreateFramesMetadata(cursor, "Madeline");
                ILCreateFramesMetadata(cursor, "MadelineNoBackpack");
                ILCreateFramesMetadata(cursor, "MadelinePlayback");
                ILCreateFramesMetadata(cursor, "AquaBadeline");
                ILCreateFramesMetadata(cursor, "MadelineAsBadeline");
            }
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            self.Level.Add(new BarrierRenderer());
        }

        private static void ILCreateFramesMetadata(ILCursor cursor, string spriteName)
        {
            cursor.EmitLdstr(spriteName);
            cursor.EmitCall(_methodCreateFramesMetadata);
        }

        private static MethodInfo _methodCreateFramesMetadata;
    }
}
