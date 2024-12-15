using Monocle;

namespace Celeste.Mod.Aqua.Rendering
{
    public class CustomBackgroundRenderer : Renderer
    {
        public override void Render(Scene scene)
        {
            scene.Entities.RenderOnly(RenderTags.CustomBackground);
        }
    }
}
