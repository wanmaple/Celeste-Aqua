using Celeste.Mod.Aqua.Rendering;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Slidable Ice Block")]
    public class SlidableIceBlock : SlidableSolid, ICustomRenderEntity
    {
        public SlidableIceBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            _fxHighlight = FXCenter.Instance.GetFX("ice_highlight");
            if (_fxHighlight != null)
            {
                _fxHighlight.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
            }
            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            _rtEff = new RenderTarget2D(Draw.SpriteBatch.GraphicsDevice, (int)Width, (int)Height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _rtEff.Dispose();
            _fxHighlight.Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            _rtEff.Dispose();
            _fxHighlight.Dispose();
        }

        public override void Update()
        {
            base.Update();
            if (_fxHighlight != null)
            {
                _fxHighlight.Parameters["Time"].SetValue(Scene.GetTime());
            }
        }

        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(_rtEff, Position, Color.White);
        }

        private void BeforeRender()
        {
            if (_fxHighlight != null)
            {
                var device = Draw.SpriteBatch.GraphicsDevice;
                device.SetRenderTarget(_rtEff);
                device.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _fxHighlight, Matrix.Identity);
                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, new Rectangle(0, 0, _rtEff.Width, _rtEff.Height), Color.White);
                Draw.SpriteBatch.End();
            }
        }

        public Effect GetEffect()
        {
            return _fxHighlight;
        }

        public void OnReload()
        {
            _fxHighlight = FXCenter.Instance.GetFX("ice_highlight");
            if (_fxHighlight != null)
            {
                _fxHighlight.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
            }
        }

        private Effect _fxHighlight;
        private RenderTarget2D _rtEff;
    }
}
