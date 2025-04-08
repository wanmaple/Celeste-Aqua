using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Booster Filter")]
    public class BoosterFilter : Filter
    {
        public bool CanPassGreenBooster { get; private set; }
        public bool CanPassRedBooster { get; private set; }

        public BoosterFilter(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            switch (data.Attr("can_pass", "Both"))
            {
                case "Green":
                    CanPassGreenBooster = true;
                    break;
                case "Red":
                    CanPassRedBooster = true;
                    break;
                case "Both":
                default:
                    CanPassGreenBooster = CanPassRedBooster = true;
                    break;
            }
        }

        protected override bool CanCollide(Entity other)
        {
            if (other is Player player)
            {
                if (CanPassGreenBooster && (player.IsBoosterDash() || (player.StateMachine.State == (int)AquaStates.StBoost && player.CurrentBooster != null && !player.CurrentBooster.red)))
                    return false;
                if (CanPassRedBooster && (player.StateMachine.State == (int)AquaStates.StRedDash || (player.StateMachine.State == (int)AquaStates.StBoost && player.CurrentBooster != null && player.CurrentBooster.red)))
                    return false;
                return true;
            }
            else if (other is Platform)
            {
                return CollideSolids;
            }
            return true;
        }
    }
}
