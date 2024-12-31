using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Decal Assigner")]
    public class DecalAssigner : Entity
    {
        public string Assign { get; private set; }

        public DecalAssigner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Assign = data.Attr("command", "Floaty");
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Entity entity in scene.Entities)
            {
                if (entity is Decal decal)
                {
                    float w = decal.textures[0].Width * decal.scale.X;
                    float h = decal.textures[0].Height * decal.scale.Y;
                    Rectangle rect = new Rectangle((int)(decal.X - w * 0.5f), (int)(decal.Y - h * 0.5f), (int)w, (int)h);
                    if (Collide.CheckRect(this, rect))
                    {
                        switch (Assign)
                        {
                            case "Floaty":
                                decal.MakeFloaty();
                                break;
                            case "StaticMover":
                                decal.MakeStaticMover((int)(-w * 0.5f), (int)(-h * 0.5f), rect.Width, rect.Height + 8);
                                break;
                        }
                    }
                }
            }
        }
    }
}
