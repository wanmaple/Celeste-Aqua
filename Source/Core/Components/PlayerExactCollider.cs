using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class PlayerExactCollider : Component
    {
        public Action<Player> OnCollide;

        public PlayerExactCollider(Action<Player> onCollide)
            : base(true, false)
        {
            OnCollide = onCollide;
        }

        public override void Update()
        {
            var players = Scene.Tracker.GetEntities<Player>();
            if (players != null)
            {
                foreach (Player player in players)
                {
                    if (player.CollideCheck(Entity))
                        OnCollide?.Invoke(player);
                }
            }
        }
    }
}
