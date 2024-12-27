using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class NumberAtlas
    {
        public static NumberAtlas Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NumberAtlas();
                return _instance;
            }
        }

        private static NumberAtlas _instance = null;

        private NumberAtlas()
        {
            MTexture source = GFX.Game["pico8/font"];
            _numberTextures = new MTexture[10];
            int index = 0;
            int j = 104;
            while (index < 4)
            {
                _numberTextures[index++] = source.GetSubtexture(j, 0, 3, 5);
                j += 4;
            }
            int i = 0;
            while (index < 10)
            {
                _numberTextures[index++] = source.GetSubtexture(i, 6, 3, 5);
                i += 4;
            }
        }

        public MTexture GetNumberTextureForCharacter(char num)
        {
            int idx = Convert.ToInt32(num) - 48;
            return _numberTextures[idx];
        }

        private MTexture[] _numberTextures;
    }
}
