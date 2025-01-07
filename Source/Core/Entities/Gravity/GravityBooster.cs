using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Gravity Booster")]
    public class GravityBooster : AquaBooster
    {
        public int GravityType { get; private set; }

        public string OverlayAnimation => GravityType switch
        {
            1 => "overlay_invert",
            2 => "overlay_toggle",
            _ => "overlay_normal",
        };

        public GravityBooster(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            GravityType = ModInterop.GravityHelper.GravityTypeToInt(data.Attr("gravity_type"));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            UpdateSprites();
        }

        public override void Update()
        {
            base.Update();
            UpdateSprites();
        }

        private void UpdateSprites()
        {
            const float RIPPLE_OFFSET = 5.0f;
            if (ModInterop.GravityHelper.IsLoaded && _rippleSprite == null)
            {
                Add(_overlaySprite = new Sprite());
                GFX.SpriteBank.CreateOn(_overlaySprite, "gravityBooster");
                _overlaySprite.Play(OverlayAnimation);
                Add(_rippleSprite = GFX.SpriteBank.Create("gravityRipple"));
                _rippleSprite.Color = ModInterop.GravityHelper.HighlightColor(GravityType);
                _rippleSprite.Play("loop");
            }
            if (_rippleSprite == null)
                return;
            int currentGravity = ModInterop.GravityHelper.GetPlayerGravity();
            if (GravityType == GravityHelperInterop.GRAVITY_INVERTED || (GravityType == GravityHelperInterop.GRAVITY_TOGGLE && currentGravity == GravityHelperInterop.GRAVITY_NORMAL))
            {
                _rippleSprite.Y = -RIPPLE_OFFSET;
                _rippleSprite.Scale.Y = 1.0f;
            }
            else if (GravityType == GravityHelperInterop.GRAVITY_NORMAL || (GravityType == GravityHelperInterop.GRAVITY_TOGGLE && currentGravity == GravityHelperInterop.GRAVITY_INVERTED))
            {
                _rippleSprite.Y = RIPPLE_OFFSET;
                _rippleSprite.Scale.Y = -1.0f;
            }

            if (GravityType == GravityHelperInterop.GRAVITY_TOGGLE)
            {
                _overlaySprite.Scale.Y = currentGravity == GravityHelperInterop.GRAVITY_NORMAL ? -1.0f : 1.0f;
            }

            _rippleSprite.Visible = _overlaySprite.Visible = sprite.CurrentAnimationID.StartsWith("loop");
        }

        protected override void BeginBoosting()
        {
            ModInterop.GravityHelper.SetPlayerGravity(GravityType);
        }

        private Sprite _overlaySprite;
        private Sprite _rippleSprite;
    }
}
