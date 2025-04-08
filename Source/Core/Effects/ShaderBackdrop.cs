using Celeste.Mod.Aqua.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public abstract class ShaderBackdrop : Backdrop
    {
        protected ShaderBackdrop()
            : base()
        {
            _device = Engine.Instance.GraphicsDevice;
            UseSpritebatch = false;
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

        public override void Ended(Scene scene)
        {
            if (_vbo != null)
            {
                _vbo.Dispose();
                _vbo = null;
            }
        }

        public override void BeforeRender(Scene scene)
        {
            if (_vbo == null)
            {
                _vbo = new VertexBuffer(_device, typeof(VertexPositionColorTexture), GetVertices().Length, BufferUsage.None);
                _vbo.SetData(GetVertices());
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            Effect eff = GetEffect();
            if (eff != null)
            {
                eff.Parameters["Time"].SetValue(scene.GetTime());
                eff.Parameters["Resolution"].SetValue(new Vector2(320.0f, 180.0f));
            }
        }

        public override void Render(Scene scene)
        {
            Effect fx = GetEffect();
            if (fx != null && !fx.IsDisposed)
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
