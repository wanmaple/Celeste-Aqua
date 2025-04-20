using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(false)]
    public class UnhookableTileCenter : Entity
    {
        public UnhookableTileCenter()
            : base()
        {
            AddTag(Tags.Global);
        }

        public void AddBlockTiletype(char tiletype)
        {
            _blockTiles.Add(tiletype);
        }

        public bool IsTileBlocked(Vector2 position)
        {
            SolidTiles tiles = Scene.Tracker.GetEntity<SolidTiles>();
            Vector2 relativePos = position - tiles.Position;
            Vector2 coord = AquaMaths.Floor(relativePos / 8.0f);
            Level level = SceneAs<Level>();
            char tiletype = level.SolidsData[(int)coord.X, (int)coord.Y];
            return _blockTiles.Contains(tiletype);
        }

        private HashSet<char> _blockTiles = new HashSet<char>();
    }
}
