using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public class HookRope : Component
    {
        public HookRope() : base(false, false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
        }

        public override void Removed(Entity entity)
        {
            base.Removed(entity);
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
        }

        public override void EntityRemoved(Scene scene)
        {
            base.EntityRemoved(scene);

            _pivots.Clear();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
        }

        public override void EntityAwake()
        {
            base.EntityAwake();
        }

        public override void Update()
        {
            GrapplingHook hook = Entity as GrapplingHook;
            if (_pivots.Count <= 0)
            {
                _pivots.Add(hook.Position);
            }
            else
            {
                _pivots[0] = hook.Position;
            }
            if (hook.State == GrapplingHook.HookStates.Fixed)
            {
            }
        }

        public override void Render()
        {
            Color lineColor = Color.White;
            for (int i = 0; i < _pivots.Count; i++)
            {
                if (i == _pivots.Count - 1)
                {
                    Player player = Scene.Tracker.GetEntity<Player>();
                    Draw.Line(_pivots[i], player.Center, lineColor);
                }
                else
                {
                    Draw.Line(_pivots[i], _pivots[i + 1], lineColor);
                }
            }
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
        }

        public override void HandleGraphicsCreate()
        {
            base.HandleGraphicsCreate();
        }

        private List<Vector2> _pivots = new List<Vector2>(8);
    }
}
