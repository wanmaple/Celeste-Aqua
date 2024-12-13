using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Rendering
{
    public class CustomDrawable : GameComponent, IDrawable
    {
        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public int DrawOrder { get; set; } = 0;
        public bool Visible { get; set; } = true;

        public CustomDrawable()
            : base(Engine.Instance)
        {
            _device = Engine.Instance.GraphicsDevice;
            _fx = FXCenter.Instance.GetFX("test");
            _vbo = new VertexBuffer(Engine.Instance.GraphicsDevice, typeof(VertexPositionColorTexture), _cube.Vertices.Length, BufferUsage.None);
            _vbo.SetData(_cube.Vertices);
            //MTexture tex = GFX.Game["objects/booster_orange/booster00"];
        }

        public override void Update(GameTime gameTime)
        {
            _time += Engine.DeltaTime;
            _rotation += Engine.DeltaTime * MathF.PI * 0.5f;
            if (_rotation >= MathF.PI * 2.0f)
            {
                _rotation -= MathF.PI * 2.0f;
            }
        }

        public void Draw(GameTime gameTime)
        {
            //_device.SetRenderTarget(GameplayBuffers.Level);
            _device.BlendState = BlendState.AlphaBlend;
            _device.SamplerStates[0] = SamplerState.PointClamp;
            _device.DepthStencilState = new DepthStencilState
            {
                DepthBufferEnable = false,
                DepthBufferWriteEnable = true,
            };
            _device.RasterizerState = RasterizerState.CullClockwise;
            _device.SetVertexBuffer(_vbo);
            Viewport viewport = _device.Viewport;
            Matrix model = AquaMaths.TRS(Vector3.UnitY * 1.0f, Quaternion.CreateFromAxisAngle(Vector3.UnitY, _rotation), Vector3.One * 1.0f);
            Matrix view = Matrix.CreateLookAt(Vector3.UnitZ * 2.5f, Vector3.Zero, Vector3.Up);
            Matrix proj = Matrix.CreatePerspectiveFieldOfView(MathF.PI * 2.0f / 3.0f, (float)viewport.Width / viewport.Height, 0.1f, 1000.0f);
            Matrix transform = model * view * proj;
            _fx.Parameters["MatrixTransform"].SetValue(transform);
            //_fx.Parameters["Albedo"].SetValue(GFX.Game["objects/booster_orange/booster00"].Texture.Texture_Safe);
            //_fx.Parameters["Time"].SetValue(_time);
            //_fx.Parameters["Resolution"].SetValue(new Vector2(viewport.Width, viewport.Height));
            foreach (EffectPass pass in _fx.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _cube.Vertices.Length / 3);
            }
        }

        Cube _cube = new Cube();
        VertexBuffer _vbo;
        Effect _fx;
        GraphicsDevice _device;
        float _rotation = 0.0f;
        float _time = 0.0f;
    }
}
