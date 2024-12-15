using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Rendering
{
    [CustomEntity("Aqua/Custom Background")]
    public class CustomBackground : CustomShaderEntity
    {
        public string FXName { get; private set; }

        public CustomBackground(string fxName)
            : base()
        {
            FXName = fxName;
            _fx = FXCenter.Instance.GetFX(FXName);
            AddTag(RenderTags.CustomBackground);
            AddTag(Tags.Global);
        }

        public CustomBackground(EntityData data, Vector2 offset)
            : this(data.Attr("fx"))
        {
        }

        public override void OnReload()
        {
            _fx = FXCenter.Instance.GetFX(FXName);
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

        private Effect _fx;
        private Quad _quad = new Quad();
    }
}
