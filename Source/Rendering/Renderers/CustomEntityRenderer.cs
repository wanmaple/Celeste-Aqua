using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.Aqua.Rendering
{
    public class CustomEntityRenderer : Renderer
    {
        public override void Render(Scene scene)
        {
            // 本来想搞批处理的，但是大部分Shader都有Uniform参数，又不想写Instancing，想了想Draw call多一些就多一些吧 别滥用就好(未来如果有精力的话(TODO))
            _customEntities.Clear();
            foreach (Entity entity in scene.Entities)
            {
                if (entity.Visible && entity.TagCheck(RenderTags.CustomEntity) && entity is ICustomRenderEntity e)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, e.GetEffect(), GameplayRenderer.instance.Camera.Matrix);
                    entity.Render();
                    Draw.SpriteBatch.End();
                }
            }
        }

        private List<Entity> _customEntities = new List<Entity>();
    }
}
