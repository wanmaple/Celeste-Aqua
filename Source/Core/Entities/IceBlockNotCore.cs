using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Ice Block Not Core")]
    public class IceBlockNotCore : IceBlock
    {
        public IceBlockNotCore(EntityData data, Vector2 offset)
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
