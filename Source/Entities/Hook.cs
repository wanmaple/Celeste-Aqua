using Monocle;

namespace Celeste.Mod.Aqua
{
    public class Hook : Entity
    {
        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }

        public override void HandleGraphicsCreate()
        {
            base.HandleGraphicsCreate();
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }

        public override void Render()
        {
            base.Render();
        }

        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
