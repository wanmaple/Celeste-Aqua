using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class PlayerHairExtensions
    {
        public static void Initialize()
        {
            On.Celeste.PlayerHair.AfterUpdate += PlayerHair_AfterUpdate;
            On.Celeste.PlayerHair.GetHairScale += PlayerHair_GetHairScale;
        }

        public static void Uninitialize()
        {
            On.Celeste.PlayerHair.AfterUpdate -= PlayerHair_AfterUpdate;
            On.Celeste.PlayerHair.GetHairScale -= PlayerHair_GetHairScale;
        }

        private static void PlayerHair_AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            PresentationPlayer player = self.Entity as PresentationPlayer;
            if (player != null)
            {
                float invert = MathF.Sign(player.PlayerSprite.Scale.Y);
                self.StepYSinePerSegment *= invert;
                self.StepPerSegment *= invert;
                self.wave *= invert;
                if (invert < 0.0f)
                {
                    Vector2 renderPos = player.PlayerSprite.RenderPosition;
                    renderPos.Y += 4.0f;
                    player.PlayerSprite.RenderPosition = renderPos;
                }
            }
            orig(self);
            if (player != null)
            {
                float invert = MathF.Sign(player.PlayerSprite.Scale.Y);
                if (invert < 0.0f)
                {
                    Vector2 renderPos = player.PlayerSprite.RenderPosition;
                    renderPos.Y -= 4.0f;
                    player.PlayerSprite.RenderPosition = renderPos;
                }
            }
        }

        private static Vector2 PlayerHair_GetHairScale(On.Celeste.PlayerHair.orig_GetHairScale orig, PlayerHair self, int index)
        {
            Vector2 scale = orig(self, index);
            if (self.Entity is PresentationPlayer player)
            {
                scale.Y *= MathF.Sign(player.PlayerSprite.Scale.Y);
            }
            return scale;
        }
    }
}
