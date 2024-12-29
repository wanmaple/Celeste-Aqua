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
            Add(new Image9Slice(GFX.Game["objects/ice_block/ice_9tile"], (int)Width, (int)Height, Image9Slice.RenderMode.Fill));
            Add(new BeforeRenderHook(BeforeRender));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Effect fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "ice_highlight");
            if (fx != null)
            {
                fx.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
            }
            RenderInfoStorage.Instance.CreateSimpleRenderTarget(this.GetUniqueID(), (int)Width, (int)Height);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            RenderInfoStorage.Instance.ReleaseAll(this.GetUniqueID());
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            RenderInfoStorage.Instance.ReleaseAll(this.GetUniqueID());
        }

        public override void Update()
        {
            base.Update();
            PatchSpeedRunner();
            Effect fx = RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
            if (fx != null)
            {
                fx.Parameters["Time"].SetValue(Scene.GetTime());
            }
        }

        public override void Render()
        {
            base.Render();
            RenderTarget2D rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (rt != null)
            {
                Draw.SpriteBatch.Draw(rt, Position, Color.White);
            }
        }

        private void BeforeRender()
        {
            Effect fx = RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
            RenderTarget2D rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (fx != null && rt != null)
            {
                var device = Draw.SpriteBatch.GraphicsDevice;
                device.SetRenderTarget(rt);
                device.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, fx, Matrix.Identity);
                Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, new Rectangle(0, 0, rt.Width, rt.Height), Color.White);
                Draw.SpriteBatch.End();
            }
        }

        private void PatchSpeedRunner()
        {
            // This is a patch to SpeedRunner, IDK if there is a better solution though since loading only trigger the Removed not Added.
            Effect fx = RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
            if (fx == null)
            {
                fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "ice_highlight");
                if (fx != null)
                {
                    fx.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
                }
            }
            RenderTarget2D rt = RenderInfoStorage.Instance.GetSimpleRenderTarget(this.GetUniqueID());
            if (rt == null)
            {
                RenderInfoStorage.Instance.CreateSimpleRenderTarget(this.GetUniqueID(), (int)Width, (int)Height);
            }
        }

        public Effect GetEffect()
        {
            return RenderInfoStorage.Instance.GetEffect(this.GetUniqueID());
        }

        public void OnReload()
        {
            Effect fx = RenderInfoStorage.Instance.CreateEffect(this.GetUniqueID(), "ice_highlight");
            if (fx != null)
            {
                fx.Parameters["Resolution"].SetValue(new Vector2(Width, Height));
            }
        }
    }
}
