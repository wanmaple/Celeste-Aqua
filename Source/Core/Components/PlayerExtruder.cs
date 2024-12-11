using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class PlayerExtruder : Component
    {
        public PlayerExtruder()
            : base(true, false)
        {
        }

        public override void Update()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            Vector2 speed = player.Speed;
            if (!AquaMaths.IsApproximateZero(speed.X))
            {
                if (player.Right < Entity.Right && player.Right > Entity.Left)
                {
                    
                }
            }
        }
    }
}
