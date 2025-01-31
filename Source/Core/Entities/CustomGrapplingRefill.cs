using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Custom Grappling Refill")]
    [Tracked(false)]
    public class CustomGrapplingRefill : GrapplingRefill
    {
        public CustomGrapplingRefill(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            RefillCount = data.Int("refill_count", 3);
        }

        public override void Render()
        {
            base.Render();
            if (Collidable)
            {
                string numDisplay = RefillCount.ToString();
                int len = numDisplay.Length;
                for (int i = 0; i < len; i++)
                {
                    MTexture texNum = NumberAtlas.Instance.GetNumberTextureForCharacter(numDisplay[i]);
                    float offset = len % 2 == 0 ? -len * 2.0f + 2.0f + i * 4.0f : (len / 2) * -4.0f + i * 4.0f;
                    texNum.DrawOutlineCentered(new Vector2(Position.X + outline.Position.X + 4.0f + offset, Position.Y + outline.Position.Y + 4.0f + sine.Value * 2.0f), Color.White, Vector2.One);
                }
            }
        }

        protected override void SetupSprite()
        {
            if (UseDefaultSprite || !GFX.SpriteBank.Has(RefillSprite) || !GFX.SpriteBank.Has(FlashSprite) || !GFX.Game.Has(OutlineTexture))
            {
                string outlinePath = "objects/refills/refill_Hook/outline";
                string spriteName = "Aqua_HookRefill";
                string spriteFlashName = "Aqua_HookRefillFlash";
                outline.Texture = GFX.Game[outlinePath];
                GFX.SpriteBank.CreateOn(sprite, spriteName);
                GFX.SpriteBank.CreateOn(flash, spriteFlashName);
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
                ParticleColor1 = Calc.HexToColor("d0bee9");
                ParticleColor2 = Calc.HexToColor("8e7ca6");
                base.SetupParticles();
            }
            else
            {
                base.SetupParticles();
            }
        }
    }
}
