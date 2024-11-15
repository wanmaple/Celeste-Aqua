using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public class PlayerInOut : Component
    {
        public PlayerInOut(Action<Player> playerIn, Action<Player> playerOut)
            : base(true, false)
        {
            _playerIn = playerIn;
            _playerOut = playerOut;
        }

        public override void Update()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            bool currentIn = Entity.CollideCheck(player);
            if (_lastIn != currentIn)
            {
                if (currentIn)
                {
                    _playerIn?.Invoke(player);
                }
                else
                {
                    _playerOut?.Invoke(player);
                }
            }
            _lastIn = currentIn;
        }

        private bool _lastIn = false;
        private Action<Player> _playerIn;
        private Action<Player> _playerOut;
    }
}
