using Monocle;

namespace Celeste.Mod.Aqua.Rendering
{
    public class CustomBackgroundRenderer : Renderer
    {
        public CustomBackgroundRenderer()
        {
            Visible = true;
        }

        public override void BeforeRender(Scene scene)
        {
        }

        public override void Render(Scene scene)
        {
            scene.Entities.RenderOnly(RenderTags.CustomBackground);
        }

        public override void AfterRender(Scene scene)
        {
        }
    }
}
