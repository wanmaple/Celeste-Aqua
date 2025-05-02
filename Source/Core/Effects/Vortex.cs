using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class VortexParameters
    {
        public Color Color1 { get; set; }
        public Color Color2 { get; set; }
        public float Duration { get; set; }

        public VortexParameters()
        {
            Color1 = Color.Red;
            Color2 = Color.Blue;
            Duration = 4.0f;
        }
    }

    public class Vortex : ShaderBackdrop
    {
        public VortexParameters Arguments { get; private set; }

        public Vortex(VortexParameters args)
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
                _fx = FXCenter.Instance.GetFX("vortex");
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Effect eff = GetEffect();
            if (eff != null)
            {
                eff.Parameters["Color1"].SetValue(Arguments.Color1.ToVector3());
                eff.Parameters["Color2"].SetValue(Arguments.Color2.ToVector3());
                eff.Parameters["Duration"].SetValue(Arguments.Duration);
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
