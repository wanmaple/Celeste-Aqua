using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Grapple Attack Kevin")]
    public class GrappleAttackCrushBlock : CrushBlock
    {
        public bool ActiveTop { get; private set; }
        public bool ActiveBottom { get; private set; }
        public bool ActiveLeft { get; private set; }
        public bool ActiveRight { get; private set; }
        public bool GrappleTrigger { get; private set; }
        public bool DashTrigger { get; private set; }
        public Color EdgeColor { get; private set; }

        public GrappleAttackCrushBlock(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            ActiveTop = data.Bool("top", true);
            ActiveBottom = data.Bool("bottom", true);
            ActiveLeft = data.Bool("left", true);
            ActiveRight = data.Bool("right", true);
            GrappleTrigger = data.Bool("grapple_trigger", true);
            DashTrigger = data.Bool("dash_trigger", true);
            EdgeColor = data.HexColor("edge_color", Calc.HexToColor("0efefe"));
            RebuildImages();
            if (GrappleTrigger)
                Add(new HookInteractable(OnGrappleInteract));
            OnDashCollide = OnCustomDashed;
        }

        private void RebuildImages()
        {
            foreach (Image img in idleImages)
            {
                Remove(img);
            }
            foreach (Image img in activeLeftImages)
            {
                Remove(img);
            }
            foreach (Image img in activeRightImages)
            {
                Remove(img);
            }
            foreach (Image img in activeTopImages)
            {
                Remove(img);
            }
            foreach (Image img in activeBottomImages)
            {
                Remove(img);
            }
            MTexture idle = GFX.Game["objects/grapple_kevin/block"];
            MTexture idleTop = GFX.Game["objects/grapple_kevin/block_top"];
            MTexture idleBottom = GFX.Game["objects/grapple_kevin/block_bottom"];
            MTexture idleLeft = GFX.Game["objects/grapple_kevin/block_left"];
            MTexture idleRight = GFX.Game["objects/grapple_kevin/block_right"];
            int w = (int)(Width / 8.0f) - 1;
            int h = (int)(Height / 8.0f) - 1;
            AddIdleLayer(idle, w, h);
            int num = idleImages.Count;
            if (ActiveTop)
                AddIdleLayer(idleTop, w, h);
            if (ActiveBottom)
                AddIdleLayer(idleBottom, w, h);
            if (ActiveLeft)
                AddIdleLayer(idleLeft, w, h);
            if (ActiveRight)
                AddIdleLayer(idleRight, w, h);
            MTexture litTop = GFX.Game["objects/grapple_kevin/lit_top"];
            MTexture litBottom = GFX.Game["objects/grapple_kevin/lit_bottom"];
            MTexture litLeft = GFX.Game["objects/grapple_kevin/lit_left"];
            MTexture litRight = GFX.Game["objects/grapple_kevin/lit_right"];
            if (ActiveTop)
            {
                AddLitImage(litTop, 0, 0, 0, 0, activeTopImages);
                AddLitImage(litTop, w, 0, 3, 0, activeTopImages);
                for (int i = 1; i < w; i++)
                {
                    AddLitImage(litTop, i, 0, Calc.Random.Choose(1, 2), 0, activeTopImages);
                }
            }
            if (ActiveBottom)
            {
                AddLitImage(litBottom, 0, h, 0, 0, activeBottomImages);
                AddLitImage(litBottom, w, h, 3, 0, activeBottomImages);
                for (int i = 1; i < w; i++)
                {
                    AddLitImage(litBottom, i, h, Calc.Random.Choose(1, 2), 0, activeBottomImages);
                }
            }
            if (ActiveLeft)
            {
                AddLitImage(litLeft, 0, 0, 0, 0, activeLeftImages);
                AddLitImage(litLeft, 0, h, 0, 3, activeLeftImages);
                for (int i = 1; i < h; i++)
                {
                    AddLitImage(litLeft, 0, i, 0, Calc.Random.Choose(1, 2), activeLeftImages);
                }
            }
            if (ActiveRight)
            {
                AddLitImage(litRight, w, 0, 0, 0, activeRightImages);
                AddLitImage(litRight, w, h, 0, 3, activeRightImages);
                for (int i = 1; i < h; i++)
                {
                    AddLitImage(litRight, w, i, 0, Calc.Random.Choose(1, 2), activeRightImages);
                }
            }
            int idx = 0;
            foreach (Image image in idleImages)
            {
                if (idx >= num)
                    image.SetColor(EdgeColor);
                ++idx;
            }
        }

        private void AddIdleLayer(MTexture texture, int width, int height)
        {
            AddImage(texture, 0, 0, 0, 0);
            AddImage(texture, width, 0, 3, 0);
            AddImage(texture, 0, height, 0, 3);
            AddImage(texture, width, height, 3, 3);
            for (int i = 1; i < width; i++)
            {
                AddImage(texture, i, 0, Calc.Random.Choose(1, 2), 0);
                AddImage(texture, i, height, Calc.Random.Choose(1, 2), 3);
            }

            for (int j = 1; j < height; j++)
            {
                AddImage(texture, 0, j, 0, Calc.Random.Choose(1, 2));
                AddImage(texture, width, j, 3, Calc.Random.Choose(1, 2));
            }
        }

        private void AddLitImage(MTexture texture, int x, int y, int tx, int ty, IList<Image> container)
        {
            MTexture subtexture = texture.GetSubtexture(tx * 8, ty * 8, 8, 8);
            Vector2 pos = new Vector2(x * 8, y * 8);
            var image = new Image(subtexture);
            container.Add(image);
            image.Position = pos;
            image.Visible = false;
            image.SetColor(EdgeColor);
            Add(image);
        }

        private DashCollisionResults OnCustomDashed(Player player, Vector2 direction)
        {
            if (DashTrigger && CanActivateKevin(-direction))
            {
                Attack(-direction);
                return DashCollisionResults.Rebound;
            }

            return DashCollisionResults.NormalCollision;
        }

        private bool OnGrappleInteract(GrapplingHook grapple, Vector2 at)
        {
            Vector2 direction = ConvertToHitDirection(grapple, grapple.ShootDirection);
            if (GrappleTrigger && CanActivateKevin(-direction))
            {
                grapple.Revoke();
                Attack(-direction);
                return true;
            }
            return false;
        }

        private Vector2 ConvertToHitDirection(GrapplingHook grapple, Vector2 direction)
        {
            if (grapple.CollideCheck(this, grapple.Position + Vector2.UnitX))
            {
                return Vector2.UnitX;
            }
            if (grapple.CollideCheck(this, grapple.Position - Vector2.UnitX))
            {
                return -Vector2.UnitX;
            }
            if (grapple.CollideCheck(this, grapple.Position + Vector2.UnitY))
            {
                return Vector2.UnitY;
            }
            if (grapple.CollideCheck(this, grapple.Position - Vector2.UnitY))
            {
                return -Vector2.UnitY;
            }
            return direction;
        }

        private bool CanActivateKevin(Vector2 direction)
        {
            if (giant && direction.X <= 0f)
            {
                return false;
            }

            if (canActivate && crushDir != direction)
            {
                if (direction.X < 0.0f && !ActiveLeft)
                    return false;
                if (direction.X > 0.0f && !ActiveRight)
                    return false;
                if (direction.Y < 0.0f && !ActiveTop)
                    return false;
                if (direction.Y > 0.0f && !ActiveBottom)
                    return false;
                return true;
            }

            return false;
        }
    }
}
