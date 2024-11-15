using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unidirectional Dash Block")]
    [Tracked(true)]
    public class UnidirectionalDashBlock : DashBlock
    {
        public UnidirectionalDashBlock(Vector2 position, char tiletype, float width, float height, bool blendIn, bool permanent, bool canDash, EntityID id) 
            : base(position, tiletype, width, height, blendIn, permanent, canDash, id)
        {
            OnDashCollide = OnDashAttacked;
            _texNotchSet = GFX.Game["tilesets/notches/notch_set"];
        }

        public UnidirectionalDashBlock(EntityData data, Vector2 offset, EntityID id) 
            : base(data, offset, id)
        {
            OnDashCollide = OnDashAttacked;
            switch (data.Attr("direction"))
            {
                case "Left":
                    _specificDashDirection = -Vector2.UnitX;
                    break;
                case "Right":
                    _specificDashDirection = Vector2.UnitX;
                    break;
                case "Up":
                    _specificDashDirection = -Vector2.UnitY;
                    break;
                case "Down":
                    _specificDashDirection = Vector2.UnitY;
                    break;
            }
        }

        private DashCollisionResults OnDashAttacked(Player player, Vector2 direction)
        {
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
            {
                return DashCollisionResults.NormalCollision;
            }

            if (AquaMaths.IsApproximateZero(_specificDashDirection) || Vector2.Dot(direction, _specificDashDirection) > 0.0f)
            {
                Break(player.Center, direction, true, true);
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        public override void Render()
        {
            //if (_texNotchSet != null)
            //{
            //    _texNotchSet.Draw(Position, Vector2.Zero, Color.White, new Vector2(1.0f, Height / 8.0f), 0.0f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
            //}
            //TileGrid tileGrid = Get<TileGrid>();
            //tileGrid.Position = new Vector2(8.0f, 0.0f);
            base.Render();
        }

        private Vector2 _specificDashDirection = Vector2.Zero;

        private MTexture _texNotchSet;
    }
}
