using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlayerSpriteExtensions
    {
        public static void Initialize()
        {
            On.Celeste.PlayerSprite.ctor += PlayerSprite_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.PlayerSprite.ctor -= PlayerSprite_Construct;
        }

        public static void SetHookMode(this PlayerSprite self, bool useHook, bool force = false)
        {
            bool isHookMode = DynamicData.For(self).Get<bool>("is_hook_mode");
            if (isHookMode == useHook && !force)
            {
                return;
            }

            string id = string.Empty;
            switch (self.Mode)
            {
                case PlayerSpriteMode.Madeline:
                    id = useHook ? "Madeline" : "player";
                    break;
                case PlayerSpriteMode.MadelineNoBackpack:
                    id = useHook ? "MadelineNoBackpack" : "player_no_backpack";
                    break;
                case PlayerSpriteMode.Badeline:
                    id = useHook ? "AquaBadeline" : "badeline";
                    break;
                case PlayerSpriteMode.MadelineAsBadeline:
                    id = useHook ? "MadelineAsBadeline" : "player_badeline";
                    break;
                case PlayerSpriteMode.Playback:
                    id = useHook ? "MadelinePlayback" : "player_playback";
                    break;
            }

            self.spriteName = id;
            GFX.SpriteBank.CreateOn(self, id);
            DynamicData.For(self).Set("is_hook_mode", useHook);
        }

        private static void PlayerSprite_Construct(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode)
        {
            orig(self, mode);
            DynamicData.For(self).Set("is_hook_mode", false);
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
    }
}
