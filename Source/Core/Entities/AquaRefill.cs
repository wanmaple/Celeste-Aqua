using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Refill")]
    [Tracked(false)]
    public class AquaRefill : CustomRefill
    {
        public AquaRefill(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            RefillCount = data.Bool("twoDash", false) ? 2 : 1;
        }

        protected override void SetupSprite()
        {
            if (UseDefaultSprite || !GFX.SpriteBank.Has(RefillSprite) || !GFX.SpriteBank.Has(FlashSprite) || !GFX.Game.Has(OutlineTexture))
            {
                if (Hookable)
                {
                    string dir = string.Empty;
                    string animID = string.Empty;
                    if (twoDashes)
                    {
                        dir = "objects/refills/refillTwo_Hookable/";
                        animID = "Aqua_RefillTwo";
                    }
                    else
                    {
                        dir = "objects/refills/refillOne_Hookable/";
                        animID = "Aqua_RefillOne";
                    }
                    outline.Texture = GFX.Game[dir + "outline"];
                    GFX.SpriteBank.CreateOn(sprite, animID);
                    GFX.SpriteBank.CreateOn(flash, animID + "Flash");
                }
            }
            else
            {
                    outline.Texture = GFX.Game[OutlineTexture];
                GFX.SpriteBank.CreateOn(sprite, RefillSprite);
                GFX.SpriteBank.CreateOn(flash, FlashSprite);
            }
        }

        protected override void SetupParticles()
        {
            if (UseDefaultSprite || !GFX.SpriteBank.Has(RefillSprite) || !GFX.SpriteBank.Has(FlashSprite))
            {
                if (Hookable)
                {
                    string dir = string.Empty;
                    string animID = string.Empty;
                    if (twoDashes)
                    {
                        ParticleColor1 = Calc.HexToColor("912ed4");
                        ParticleColor2 = Calc.HexToColor("4b1680");
                    }
                    else
                    {
                        ParticleColor1 = Calc.HexToColor("f19310");
                        ParticleColor2 = Calc.HexToColor("824c00");
                    }
                    base.SetupParticles();
                }
            }
            else
            {
                base.SetupParticles();
            }
        }

        protected override bool UseRefill(Player player)
        {
            return player.UseRefill(twoDashes);
        }

        protected override bool RefillCondition(Player player)
        {
            return player.Dashes < (twoDashes ? 2 : 1);
        }
    }
}
