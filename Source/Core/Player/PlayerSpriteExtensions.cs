using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlayerSpriteExtensions
    {
        public static void Initialize()
        {
            On.Celeste.PlayerSprite.CreateFramesMetadata += PlayerSprite_CreateFramesMetadata;
        }

        public static void Uninitialize()
        {
            On.Celeste.PlayerSprite.CreateFramesMetadata -= PlayerSprite_CreateFramesMetadata;
        }

        private static void PlayerSprite_CreateFramesMetadata(On.Celeste.PlayerSprite.orig_CreateFramesMetadata orig, string sprite)
        {
            orig(sprite);
            if (CUSTOM_PLAYER_SPRITE_MAPPING.TryGetValue(sprite, out string customSprite))
            {
                PlayerSprite.CreateFramesMetadata(customSprite);
                var existingAnims = GFX.SpriteBank.SpriteData[sprite].Sprite.Animations;
                var customAnims = GFX.SpriteBank.SpriteData[customSprite].Sprite.Animations;
                foreach (var pair in customAnims)
                {
                    string animId = pair.Key;
                    if (!existingAnims.ContainsKey(animId))
                    {
                        existingAnims.Add(animId, pair.Value);
                    }
                }
            }
        }

        public static void PlayFlipOnIce(this PlayerSprite self)
        {
            if (!self.animations.TryGetValue("fliponice", out Sprite.Animation anim))
            {
                anim = new Sprite.Animation();
                anim.Delay = 0.04f;
                anim.Frames = self.animations["flip"].Frames;
                anim.Goto = null;
                self.animations.Add("fliponice", anim);
            }
            self.Play("fliponice");
        }

        private static Dictionary<string, string> CUSTOM_PLAYER_SPRITE_MAPPING = new Dictionary<string, string>
        {
            { "player", "Aqua_Madeline" },
            { "player_no_backpack", "Aqua_MadelineNoBackpack" },
            { "badeline", "Aqua_Badeline" },
            { "player_badeline", "Aqua_MadelineAsBadeline" },
            { "player_playback", "Aqua_MadelinePlayback" },
        };
    }
}
