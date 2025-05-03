using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Tile Controller")]
    public class UnhookableTileController : Entity
    {
        public char BlockTile1 { get; private set; }
        public char BlockTile2 { get; private set; }
        public char BlockTile3 { get; private set; }
        public char BlockTile4 { get; private set; }
        public bool Activate1 { get; private set; }
        public bool Activate2 { get; private set; }
        public bool Activate3 { get; private set; }
        public bool Activate4 { get; private set; }

        public UnhookableTileController(EntityData data, Vector2 offset)
            : base()
        {
            BlockTile1 = data.Char("block_tile1");
            BlockTile2 = data.Char("block_tile2");
            BlockTile3 = data.Char("block_tile3");
            BlockTile4 = data.Char("block_tile4");
            Activate1 = data.Bool("activate1");
            Activate2 = data.Bool("activate2");
            Activate3 = data.Bool("activate3");
            Activate4 = data.Bool("activate4");
        }

        public override void Update()
        {
            base.Update();
            var state = SceneAs<Level>().GetState();
            if (state != null)
            {
                if (Activate1 && BlockTile1 != '\0')
                    state.UnhookableTiletypes.Add(BlockTile1);
                if (Activate2 && BlockTile2 != '\0')
                    state.UnhookableTiletypes.Add(BlockTile2);
                if (Activate3 && BlockTile3 != '\0')
                    state.UnhookableTiletypes.Add(BlockTile3);
                if (Activate4 && BlockTile4 != '\0')
                    state.UnhookableTiletypes.Add(BlockTile4);
            }
            RemoveSelf();
        }
    }
}
