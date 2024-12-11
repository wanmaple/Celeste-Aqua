using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Aqua Spring")]
    [Tracked(false)]
    public class AquaSpring : Spring
    {
        public enum SpringOrientations
        {
            Up,
            Down,
            Left,
            Right,
        }

        public AquaSpring(EntityData data, Vector2 offset)
            : base(data, offset, Orientations.Floor)
        {
            PufferCollider pufferCollider = Get<PufferCollider>();
            switch (data.Attr("orientation"))
            {
                case "Down":
                    _orientation = SpringOrientations.Down;
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitY);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY);
                    Collider = new Hitbox(16f, 6f, -8f, 0f);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, 0f);
                    sprite.Rotation = MathF.PI;
                    break;
                case "Left":
                    _orientation = SpringOrientations.Left;
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitX);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX);
                    Collider = new Hitbox(6f, 16f, -6f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, -12f, -8f);
                    sprite.Rotation = -MathF.PI / 2f;
                    break;
                case "Right":
                    _orientation = SpringOrientations.Right;
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitX);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX);
                    base.Collider = new Hitbox(6f, 16f, 0f, -8f);
                    pufferCollider.Collider = new Hitbox(12f, 16f, 0f, -8f);
                    sprite.Rotation = MathF.PI / 2f;
                    break;
                case "Up":
                default:
                    _orientation = SpringOrientations.Up;
                    staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitY);
                    staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY);
                    Collider = new Hitbox(16f, 6f, -8f, -6f);
                    pufferCollider.Collider = new Hitbox(16f, 10f, -8f, -10f);
                    break;
            }

            PlayerCollider playerCollider = Get<PlayerCollider>();
            playerCollider.OnCollide = OnCollideEx;
            HookInteractable hookInteractable = Get<HookInteractable>();
            hookInteractable.Interaction = OnInteractHookEx;
        }

        private void OnCollideEx(Player player)
        {
            if (player.StateMachine.State == 9 || !playerCanUse)
            {
                return;
            }

            switch (_orientation)
            {
                case SpringOrientations.Up:
                    if (player.Speed.Y >= 0f)
                    {
                        BounceAnimate();
                        player.SuperBounce(Top);
                    }
                    break;
                case SpringOrientations.Down:
                    if (player.Speed.Y <= 0f)
                    {
                        BounceAnimate();
                        player.BounceDown(Bottom);
                    }
                    break;
                case SpringOrientations.Left:
                    if (player.SideBounce(-1, Left, CenterY))
                    {
                        BounceAnimate();
                    }
                    break;
                case SpringOrientations.Right:
                    if (player.SideBounce(1, Right, CenterY))
                    {
                        BounceAnimate();
                    }
                    break;
            }
        }

        private bool OnInteractHookEx(GrapplingHook hook, Vector2 at)
        {
            if (hook.Bounce(GetBounceDirectionEx(), GrapplingHook.BOUNCE_SPEED_ADDITION))
            {
                BounceAnimate();
                return true;
            }
            return false;
        }

        private Vector2 GetBounceDirectionEx()
        {
            Vector2 direction = Vector2.Zero;
            switch (_orientation)
            {
                case SpringOrientations.Up:
                    direction = -Vector2.UnitY;
                    break;
                case SpringOrientations.Down:
                    direction = Vector2.UnitY;
                    break;
                case SpringOrientations.Left:
                    direction = -Vector2.UnitX;
                    break;
                case SpringOrientations.Right:
                    direction = Vector2.UnitX;
                    break;
                default:
                    break;
            }
            return direction;
        }

        private SpringOrientations _orientation;
    }
}
