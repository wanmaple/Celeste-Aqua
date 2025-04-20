using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Die Trigger")]
    public class DieTrigger : Trigger
    {
        public DieTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            player.Die(Vector2.Zero);
        }
    }
}
