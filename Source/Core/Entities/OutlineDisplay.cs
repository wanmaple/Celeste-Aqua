using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Outline Display")]
    public class OutlineDisplay : Entity
    {
        public MTexture Texture { get; private set; }
        public Color Color { get; private set; }

        public OutlineDisplay(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            string path = data.Attr("texture_path", "objects/acceleration_area/area");
            if (!GFX.Game.Has(path))
            {
                path = "objects/acceleration_area/area";
            }
            Texture = GFX.Game[path];
            Color = data.HexColor("color", Color.White);
            var image = new Image9Slice(Texture, data.Width, data.Height, Image9Slice.RenderMode.Border);
            image.SetColor(Color);
            Add(image);
            Depth = Depths.SolidsBelow;
            Active = false;
        }
    }
}
