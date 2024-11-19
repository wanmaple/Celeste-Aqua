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
        }

        public static void Uninitialize()
        {
            IL.Celeste.LevelLoader.ctor -= LevelLoader_Construct;
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

        private static void ILCreateFramesMetadata(ILCursor cursor, string spriteName)
        {
            cursor.EmitLdstr(spriteName);
            cursor.EmitCall(_methodCreateFramesMetadata);
        }

        private static void GFX_LoadData2(On.Celeste.GFX.orig_LoadData orig)
        {
            if (!GFX.DataLoaded)
            {
                PlayerSprite.CreateFramesMetadata("Madeline");
                PlayerSprite.CreateFramesMetadata("MadelineNoBackpack");
                PlayerSprite.CreateFramesMetadata("MadelinePlayback");
                PlayerSprite.CreateFramesMetadata("AquaBadeline");
                PlayerSprite.CreateFramesMetadata("MadelineAsBadeline");
            }
            orig();
        }

        private static MethodInfo _methodCreateFramesMetadata;
    }
}
