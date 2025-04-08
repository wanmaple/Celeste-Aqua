using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.Aqua.Core.GrapplingHook;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public enum GrappleIndicatorType
    {
        None,
        ForwardUp,
        ForwardDown,
        Both,
    }

    public class GrappleIndicator : Entity
    {
        public MTexture Texture { get; private set; }
        public Player Owner { get; private set; }

        public GrappleIndicator(MTexture texture, Player owner)
            : base()
        {
            Texture = texture;
            Owner = owner;
            float size = GrapplingHook.HOOK_SIZE;
            Collider = new Hitbox(size, size, -size * 0.5f, -size * 0.5f);
            AddTag(Tags.Global);
            Depth = Depths.Top;
            this.MakeExtraCollideCondition();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(_imgForwardUp = new Image(Texture));
            Add(_imgForwardDown = new Image(Texture));
            _imgForwardUp.JustifyOrigin(0.5f, 0.5f);
            _imgForwardDown.JustifyOrigin(0.5f, 0.5f);
        }

        public override void Update()
        {
            base.Update();
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.None)
            {
                return;
            }
            var state = Owner.level.GetState();
            if (state == null || !state.FeatureEnabled)
                return;
            if (!WillShow())
                return;
            GrapplingHook grapple = Owner.GetGrappleHook();
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.ForwardUp || AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.Both)
            {
                Position = AquaMaths.Round(Owner.ExactCenter());
                _movementCounter = Owner.ExactCenter() - Position;
                Vector2 movement = Vector2.Normalize(new Vector2((float)Owner.Facing, -1.0f)) * grapple.MaxLength;
                Vector2 preMove = Position;
                if (Move(movement))
                {
                    Vector2 renderPos = Position;
                    Position = preMove;
                    _imgForwardUp.RenderPosition = renderPos;
                    _imgForwardUp.Visible = true;
                }
                else
                {
                    _imgForwardUp.Visible = false;
                }
            }
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.ForwardDown || AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.Both)
            {
                Position = AquaMaths.Round(Owner.ExactCenter());
                _movementCounter = Owner.ExactCenter() - Position;
                Vector2 movement = Vector2.Normalize(new Vector2((float)Owner.Facing, 1.0f)) * grapple.MaxLength;
                Vector2 preMove = Position;
                if (Move(movement))
                {
                    Vector2 renderPos = Position;
                    Position = preMove;
                    _imgForwardDown.RenderPosition = renderPos;
                    _imgForwardDown.Visible = true;
                }
                else
                {
                    _imgForwardDown.Visible = false;
                }
            }
        }

        public override void Render()
        {
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.None)
            {
                return;
            }
            var state = Owner.level.GetState();
            if (state == null || !state.FeatureEnabled)
                return;
            if (!WillShow())
                return;
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.ForwardUp ||
                AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.Both)
            {
                if (_imgForwardUp.Visible)
                    _imgForwardUp.Render();
            }
            if (AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.ForwardDown ||
                AquaModule.Settings.IndicatorSetting == GrappleIndicatorType.Both)
            {
                if (_imgForwardDown.Visible)
                    _imgForwardDown.Render();
            }
        }

        public bool CanCollide(Entity other)
        {
            return other.IsHookable();
        }

        private bool WillShow()
        {
            bool playerCond = !Owner.Ducking && (Owner.StateMachine.State == (int)AquaStates.StNormal || Owner.StateMachine.State == (int)AquaStates.StDash || Owner.StateMachine.State == (int)AquaStates.StLaunch || (int)Owner.StateMachine.State == (int)AquaStates.StSwim);
            GrapplingHook grapple = Owner.GetGrappleHook();
            bool grappleCond = grapple != null && !grapple.Active;
            return playerCond && grappleCond;
        }

        private bool Move(Vector2 movement)
        {
            _movementCounter += movement;
            int dx = (int)MathF.Round(_movementCounter.X, MidpointRounding.ToEven);
            int dy = (int)MathF.Round(_movementCounter.Y, MidpointRounding.ToEven);
            if (dx != 0 || dy != 0)
            {
                int absY = Math.Abs(dy);
                int absX = Math.Abs(dx);
                bool swapped = false;
                if (absY > absX)
                {
                    int tmp = absX;
                    absX = absY;
                    absY = tmp;
                    swapped = true;
                }

                int stepX = MathF.Sign(dx);
                int stepY = MathF.Sign(dy);
                int error = absX - absY;
                int x = absX;
                do
                {
                    if ((!swapped && StepMoveH(stepX)) || (swapped && StepMoveV(stepY)))
                    {
                        return true;
                    }
                    else
                    {
                        if (!swapped)
                            X += stepX;
                        else
                            Y += stepY;
                    }

                    error -= absY;
                    if (error < 0)
                    {
                        if ((!swapped && StepMoveV(stepY)) || (swapped && StepMoveH(stepX)))
                        {
                            return true;
                        }
                        else
                        {
                            if (!swapped)
                                Y += stepY;
                            else
                                X += stepX;
                        }
                        error += absX;
                    }
                    x--;
                } while (x != 0);
            }
            return false;
        }

        private bool StepMoveH(int step)
        {
            Entity collideEntity = null;
            if (this.CheckCollidePlatformsAtXDirection(step, out collideEntity))
            {
                return true;
            }
            _movementCounter.X -= step;
            return false;
        }

        private bool StepMoveV(int step)
        {
            Entity collideEntity = null;
            if (this.CheckCollidePlatformsAtYDirection(step, out collideEntity))
            {
                return true;
            }
            _movementCounter.Y -= step;
            return false;
        }

        private Vector2 _movementCounter;
        private Image _imgForwardUp;
        private Image _imgForwardDown;
    }
}
