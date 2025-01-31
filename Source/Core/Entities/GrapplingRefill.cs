using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grappling Refill")]
    [Tracked(false)]
    public class GrapplingRefill : CustomRefill
    {
        public int Cap { get; private set; }
        public bool Capped { get; private set; }
        public bool AlwaysRefill { get; private set; }

        public GrapplingRefill(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            RefillCount = data.Bool("chargeTwo", false) ? 2 : 1;
            Cap = data.Int("cap", -1);
            Capped = data.Bool("capped", false);
            AlwaysRefill = data.Bool("always_refill", false);
        }

        protected override void SetupSprite()
        {
            if (UseDefaultSprite || !GFX.SpriteBank.Has(RefillSprite) || !GFX.SpriteBank.Has(FlashSprite) || !GFX.Game.Has(OutlineTexture))
            {
                string outlinePath = "objects/refills/refill_Hook/outline";
                string spriteName = "Aqua_HookRefill";
                string spriteFlashName = "Aqua_HookRefillFlash";
                if (RefillCount == 2)
                {
                    outlinePath = "objects/refills/refillTwo_Hook/outline";
                    spriteName = "Aqua_HookRefillTwo";
                    spriteFlashName = "Aqua_HookRefillTwoFlash";
                }
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
                if (RefillCount == 2)
                {
                    ParticleColor1 = Calc.HexToColor("909cb0");
                    ParticleColor2 = Calc.HexToColor("515672");
                }
                else
                {
                    ParticleColor1 = Calc.HexToColor("d0bee9");
                    ParticleColor2 = Calc.HexToColor("8e7ca6");
                }
                base.SetupParticles();
            }
            else
            {
                base.SetupParticles();
            }
        }

        protected override bool UseRefill(Player player)
        {
            Level level = SceneAs<Level>();
            var state = level.GetState();
            if (state != null && state.GameplayMode == GrapplingHook.GameplayMode.ShootCounter)
            {
                if (RefillCondition(player))
                {
                    int defaultMax = state.MaxShootCount < 0 ? int.MaxValue : state.MaxShootCount;
                    int max = Cap < 0 ? defaultMax : Cap;
                    if (!Capped)
                    {
                        state.RestShootCount += RefillCount;
                    }
                    else
                    {
                        state.RestShootCount = Math.Min(state.RestShootCount + RefillCount, max);
                    }
                    return true;
                }
            }
            return false;
        }

        protected override bool RefillCondition(Player player)
        {
            Level level = SceneAs<Level>();
            var state = level.GetState();
            if (state == null)
                return false;
            if (AlwaysRefill)
                return true;
            int defaultMax = state.MaxShootCount < 0 ? int.MaxValue : state.MaxShootCount;
            int max = Cap < 0 ? defaultMax : Cap;
            return state.RestShootCount < max;
        }
    }
}
