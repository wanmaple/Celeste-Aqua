using Monocle;

namespace Celeste.Mod.Aqua.Rendering
{
    public class CustomForegroundRenderer : Renderer
    {
        public override void Render(Scene scene)
        {
            scene.Entities.RenderOnly(RenderTags.CustomForeground);
        }
    }
}
