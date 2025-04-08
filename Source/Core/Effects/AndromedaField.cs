using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class AndromedaFieldParameters
    {
        public float TimeOffset { get; set; }
        public Color BaseColor { get; set; }
        public Color OffsetColor { get; set; }
        public float Speed { get; set; }
        public float Angle { get; set; }
        public int LayerCount { get; set; }

        public AndromedaFieldParameters()
        {
            TimeOffset = Calc.Random.Range(100.0f, 1000.0f);
            BaseColor = new Color(1.0f, 0.5f, 1.0f);
            OffsetColor = Color.Red;
            Speed = 3.0f;
            Angle = 135.0f;
            LayerCount = 4;
        }
    }

    public class AndromedaField : ShaderBackdrop
    {
        public AndromedaFieldParameters Arguments { get; private set; }

        public AndromedaField(AndromedaFieldParameters args)
            : base()
        {
            Arguments = args;
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            if (_fx != null)
            {
                _fx.Dispose();
                _fx = null;
            }
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            if (_fx == null)
                _fx = FXCenter.Instance.GetFX("andromeda_field");
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Effect eff = GetEffect();
            if (eff != null)
            {
                eff.Parameters["TimeOffset"].SetValue(Arguments.TimeOffset);
                eff.Parameters["BaseColor"].SetValue(Arguments.BaseColor.ToVector3());
                eff.Parameters["OffsetColor"].SetValue(Arguments.OffsetColor.ToVector3());
                eff.Parameters["Speed"].SetValue(Arguments.Speed);
                eff.Parameters["Angle"].SetValue(Calc.DegToRad * Arguments.Angle);
                eff.Parameters["LayerCount"].SetValue((float)Arguments.LayerCount);
            }
        }

        public override Effect GetEffect()
        {
            return _fx;
        }

        protected override VertexPositionColorTexture[] GetVertices()
        {
            return _quad.Vertices;
        }

        private Effect _fx;
        private Quad _quad = new Quad();
    }
}
