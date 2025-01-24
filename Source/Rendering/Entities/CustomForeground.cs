using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.Aqua.Rendering
{
    [CustomEntity("Aqua/Custom Foreground")]
    public class CustomForeground : CustomShaderEntity
    {
        public string FXName { get; private set; }

        public CustomForeground(string fxName)
            : base()
        {
            FXName = fxName;
            _fx = FXCenter.Instance.GetFX(FXName);
            AddTag(RenderTags.CustomForeground);
            AddTag(Tags.Global);
        }

        public CustomForeground(EntityData data, Vector2 offset)
            : this(data.Attr("fx"))
        {
        }

        public override void OnReload()
        {
            _fx = FXCenter.Instance.GetFX(FXName);
            if (_fx != null)
            {
                UpdateUniforms();
            }
        }

        public override void Update()
        {
            if (_fx != null)
            {
                _fx.Parameters["Time"].SetValue(Scene.GetTime());
                Viewport viewport = _device.Viewport;
                _fx.Parameters["Resolution"].SetValue(new Vector2(viewport.Width, viewport.Height));
            }
        }

        protected override VertexPositionColorTexture[] GetVertices()
        {
            return _quad.Vertices;
        }

        public override Effect GetEffect()
        {
            return _fx;
        }

        protected virtual void UpdateUniforms()
        {
        }

        private Effect _fx;
        private Quad _quad = new Quad();
    }
}
