using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Unhookable Area")]
    [Tracked(false)]
    public class UnhookableArea : Entity
    {
        public bool BlockUp { get; private set; }
        public bool BlockDown { get; private set; }
        public bool BlockLeft { get; private set; }
        public bool BlockRight { get; private set; }

        public UnhookableArea(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            BlockUp = data.Bool("block_up", true);
            BlockDown = data.Bool("block_down", true);
            BlockLeft = data.Bool("block_left", true);
            BlockRight = data.Bool("block_right", true);
            this.SetHookable(true);
            Add(new HookInteractable(OnInteractHook));
            if (data.Bool("attachToSolid"))
            {
                var staticMover = new StaticMover();
                staticMover.SolidChecker = solid => CollideCheck(solid);
                staticMover.OnEnable = OnEnable;
                staticMover.OnDisable = OnDisable;
                staticMover.OnDestroy = OnDestroy;
                Add(staticMover);
            }
        }

        private bool OnInteractHook(GrapplingHook grapple, Vector2 at)
        {
            Vector2 hookDir = grapple.ShootDirection;
            if (AquaMaths.BlockDirection(hookDir, this, grapple, BlockUp, BlockDown, BlockLeft, BlockRight))
            {
                Audio.Play("event:/char/madeline/unhookable", grapple.Position);
                grapple.Revoke();
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            Collidable = true;
        }

        private void OnDisable()
        {
            Collidable = false;
        }

        private void OnDestroy()
        {
            Collidable = false;
        }
    }
}
