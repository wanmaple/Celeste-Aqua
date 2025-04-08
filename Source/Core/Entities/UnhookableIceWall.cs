using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Ice Wall")]
    [Tracked(false)]
    public class UnhookableIceWall : WallBooster
    {
        public bool AttachToSolid { get; private set; }

        public UnhookableIceWall(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Height, data.Bool("left"), true)
        {
            AttachToSolid = data.Bool("attach_to_solid", false);
            if (AttachToSolid)
            {
                Remove(Get<StaticMover>());
                Add(new StaticMover
                {
                    OnShake = OnShake,
                    SolidChecker = CheckSolidCollision,
                    OnEnable = OnEnable,
                    OnDisable = OnDisable,
                    OnDestroy = OnDestroy,
                });
            }
            Add(new HookInteractable(OnGrappleInteract));
        }

        public override void Render()
        {
            Vector2 pos = Position;
            Position += _shake;
            base.Render();
            Position = pos;
        }

        private bool OnGrappleInteract(GrapplingHook grapple, Vector2 at)
        {
            Vector2 dir = grapple.ShootDirection;
            if (AquaMaths.BlockDirection(dir, this, grapple, false, false, Facing != Facings.Left, Facing == Facings.Left))
            {
                Audio.Play("event:/char/madeline/unhookable", grapple.Position);
                grapple.Revoke();
                return true;
            }
            return false;
        }

        private void OnDisable()
        {
            SetColor(Color.Gray);
            Collidable = false;
        }

        private void OnEnable()
        {
            SetColor(Color.White);
            Collidable = Visible = true;
        }

        private void OnDestroy()
        {
            Collidable = Visible = false;
        }

        private bool CheckSolidCollision(Solid solid)
        {
            bool result = Facing switch
            {
                Facings.Right => CollideCheckOutside(solid, Position + Vector2.UnitX),
                Facings.Left => CollideCheckOutside(solid, Position - Vector2.UnitX),
                _ => false,
            };
            return result;
        }

        private void OnShake(Vector2 amount)
        {
            _shake += amount;
        }

        private void SetColor(Color color)
        {
            foreach (Sprite tile in tiles)
            {
                tile.Color = color;
            }
        }

        private Vector2 _shake;
    }
}
