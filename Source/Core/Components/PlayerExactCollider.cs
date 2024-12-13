using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
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
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null && player.CollideCheck(Entity))
            {
                OnCollide?.Invoke(player);
            }
        }
    }
}
