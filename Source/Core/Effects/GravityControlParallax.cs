using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class GravityControlParallax : Parallax
    {
        public Color NormalColor { get; private set; }
        public Color InvertedColor { get; private set; }

        public GravityControlParallax(MTexture texture, Color normalColor, Color invertedColor)
            : base(texture)
        {
            NormalColor = normalColor;
            InvertedColor = invertedColor;
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            float sign = ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
            _timer = Calc.Clamp(_timer + Engine.DeltaTime * 4.0f * -sign, 0.0f, 1.0f);
            _lerpColor = Color.Lerp(NormalColor, InvertedColor, Ease.CubeInOut(_timer));
        }

        public override void Render(Scene scene)
        {
            Color origColor = Color;
            Color = new Color(origColor.ToVector4() * _lerpColor.ToVector4());
            base.Render(scene);
            Color = origColor;
        }

        private float _timer = 0.0f;
        private Color _lerpColor;
    }
}
