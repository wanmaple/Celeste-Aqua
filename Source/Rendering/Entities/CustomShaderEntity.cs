using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Aqua.Rendering
{
    [Tracked(true)]
    public abstract class CustomShaderEntity : Entity, ICustomRenderEntity
    {
        protected CustomShaderEntity()
            : base(Vector2.Zero)
        {
            _device = Engine.Instance.GraphicsDevice;
        }

        public virtual void OnReload()
        {
        }

        protected virtual void SetupRenderStates(GraphicsDevice device)
        {
            device.BlendState = BlendState.AlphaBlend;
            device.SamplerStates[0] = SamplerState.PointClamp;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullClockwise;
        }

        protected abstract VertexPositionColorTexture[] GetVertices();
        public abstract Effect GetEffect();

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (_vbo == null)
            {
                _vbo = new VertexBuffer(_device, typeof(VertexPositionColorTexture), GetVertices().Length, BufferUsage.None);
                _vbo.SetData(GetVertices());
            }
        }

        public override void Render()
        {
            Effect fx = GetEffect();
            if (fx != null)
            {
                SetupRenderStates(_device);
                _device.SetVertexBuffer(_vbo);
                foreach (EffectPass pass in fx.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    _device.DrawPrimitives(PrimitiveType.TriangleList, 0, GetVertices().Length / 3);
                }
            }
        }

        protected GraphicsDevice _device;
        protected VertexBuffer _vbo;
    }
}
