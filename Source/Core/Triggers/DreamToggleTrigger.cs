using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Dream Toggle Trigger")]
    public class DreamToggleTrigger : ActivateDreamBlocksTrigger
    {
        public bool Once { get; private set; }

        public DreamToggleTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Once = data.Bool("once");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (Once)
            {
                RemoveSelf();
            }
        }
    }
}
