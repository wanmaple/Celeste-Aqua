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
            //IL.Celeste.LevelLoader.ctor += LevelLoader_Construct;
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        public static void Uninitialize()
        {
            //IL.Celeste.LevelLoader.ctor -= LevelLoader_Construct;
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void LevelLoader_Construct(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(ins => ins.MatchLdstr("player_playback")))
            {
                cursor.Index += 2;
                ILCreateFramesMetadata(cursor, "Aqua_Madeline");
                ILCreateFramesMetadata(cursor, "Aqua_MadelineNoBackpack");
                ILCreateFramesMetadata(cursor, "Aqua_MadelinePlayback");
                ILCreateFramesMetadata(cursor, "Aqua_AquaBadeline");
                ILCreateFramesMetadata(cursor, "Aqua_MadelineAsBadeline");
            }
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            self.Level.Add(new BarrierRenderer());
            self.Level.Add(new RodEntityManager());
        }

        private static void ILCreateFramesMetadata(ILCursor cursor, string spriteName)
        {
            cursor.EmitLdstr(spriteName);
            cursor.EmitCall(_methodCreateFramesMetadata);
        }

        private static MethodInfo _methodCreateFramesMetadata;
    }
}
