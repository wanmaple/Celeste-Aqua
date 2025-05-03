using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Fire Barrier Not Core")]
    public class FireBarrierNotCore : FireBarrier
    {
        public FireBarrierNotCore(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            CoreModeListener listener = Get<CoreModeListener>();
            Remove(listener);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Collidable = true;
        }
    }
}
