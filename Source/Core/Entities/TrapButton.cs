using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Trap Button")]
    [Tracked(false)]
    public class TrapButton : Entity
    {
        public enum Directions
        {
            Up,
            Down,
            Left,
            Right,
        }

        public Directions Direction { get; set; }
        public bool Pressed { get; private set; }
        public string Flag { get; private set; }
        public Color Color { get; private set; }

        public TrapButton(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Flag = data.Attr("flag");
            Color = data.HexColor("color");
            switch (data.Attr("direction"))
            {
                case "Down":
                    Direction = Directions.Down;
                    Collider = new Hitbox(14.0f, 4.0f, -7.0f, -4.0f);
                    _rotation = MathF.PI;
                    _pedestalDirection = -Vector2.UnitY;
                    break;
                case "Left":
                    Direction = Directions.Left;
                    Collider = new Hitbox(4.0f, 14.0f, 0.0f, -7.0f);
                    _rotation = -MathF.PI * 0.5f;
                    _pedestalDirection = Vector2.UnitX;
                    break;
                case "Right":
                    Direction = Directions.Right;
                    Collider = new Hitbox(4.0f, 14.0f, -4.0f, -7.0f);
                    _rotation = MathF.PI * 0.5f;
                    _pedestalDirection = -Vector2.UnitX;
                    break;
                case "Up":
                default:
                    Direction = Directions.Up;
                    Collider = new Hitbox(14.0f, 4.0f, -7.0f, 0.0f);
                    _rotation = 0.0f;
                    _pedestalDirection = Vector2.UnitY;
                    break;
            }
            _texIdle = GFX.Game["objects/trap_button/button00"];
            _texPressed = GFX.Game["objects/trap_button/button01"];
            _texPedestal = GFX.Game["objects/trap_button/button_pedestal"];
            Depth = -10000;
            this.SetHookable(true);
            var staticMover = new StaticMover();
            staticMover.SolidChecker = solid => CollideCheckOutside(solid, Position + _pedestalDirection);
            staticMover.JumpThruChecker = jumpthru => CollideCheck(jumpthru, Position + Vector2.UnitY);
            staticMover.OnShake = OnShake;
            staticMover.OnEnable = OnEnable;
            staticMover.OnDisable = OnDisable;
            staticMover.OnDestroy = OnDestroy;
            Add(staticMover);
        }

        public override void Update()
        {
            base.Update();
            Entity collideEntity = CollideFirst<Solid>();
            if (collideEntity == null)
                collideEntity = CollideFirst<Actor>();
            if (collideEntity == null)
                collideEntity = CollideFirst<GrapplingHook>();
            if (collideEntity == null)
            {
                // jumpthrus.
                switch (Direction)
                {
                    case Directions.Down:
                        collideEntity = this.CollideFirst(typeof(JumpThru), ModInterop.DownsideJumpthruTypes);
                        break;
                    case Directions.Up:
                        var downsideTypes = ModInterop.DownsideJumpthruTypes;
                        foreach (Type downsideType in downsideTypes)
                        {
                            collideEntity = this.CollideFirst(downsideType);
                            if (collideEntity != null)
                                break;
                        }
                        break;
                    case Directions.Left:
                    case Directions.Right:
                        var sidewaysTypes = ModInterop.SidewaysJumpthruTypes;
                        for (int i = 0; i < sidewaysTypes.Count; i++)
                        {
                            Type sidewaysType = sidewaysTypes[i];
                            var sideJumpthrus = Scene.Tracker.Entities[sidewaysType];
                            if (sideJumpthrus.Count > 0)
                            {
                                FieldInfo fieldLeft2Right = sidewaysType.GetField("AllowLeftToRight", BindingFlags.Instance | BindingFlags.Public);
                                if (fieldLeft2Right != null)
                                {
                                    foreach (Entity jumpthru in sideJumpthrus)
                                    {
                                        bool left2right = (bool)fieldLeft2Right.GetValue(jumpthru);
                                        if ((left2right && Direction == Directions.Left) || (!left2right && Direction == Directions.Right))
                                        {
                                            if (CollideCheck(jumpthru))
                                            {
                                                collideEntity = jumpthru;
                                                break;
                                            }
                                        }
                                        if (collideEntity != null)
                                            break;
                                    }
                                }
                            }
                            if (collideEntity != null)
                                break;
                        }
                        break;
                }
            }
            if (collideEntity != null)
            {
                Pressed = true;
                List<Entity> allBtns = Scene.Tracker.GetEntities<TrapButton>();
                bool allPressed = true;
                foreach (TrapButton btn in allBtns)
                {
                    if (btn.Flag == Flag && !btn.Pressed)
                    {
                        allPressed = false;
                        break;
                    }
                }
                if (allPressed)
                {
                    SceneAs<Level>().Session.SetFlag(Flag, true);
                }
            }
            else
            {
                Pressed = false;
                SceneAs<Level>().Session.SetFlag(Flag, false);
            }
        }

        public override void Render()
        {
            base.Render();
            float depth = 0.0f;
            Vector2 justify = new Vector2(0.5f, 0.0f);
            if (Pressed)
            {
                Draw.SpriteBatch.Draw(_texPressed.Texture.Texture_Safe, Position + _pedestalDirection + _imageOffset, _texPressed.ClipRect, Color, _rotation, new Vector2(_texPressed.Width * justify.X, _texPressed.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
            }
            else
            {
                Draw.SpriteBatch.Draw(_texIdle.Texture.Texture_Safe, Position + _imageOffset, _texIdle.ClipRect, Color, _rotation, new Vector2(_texIdle.Width * justify.X, _texIdle.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
            }
            Draw.SpriteBatch.Draw(_texPedestal.Texture.Texture_Safe, Position + _pedestalDirection * 3.0f + _imageOffset, _texPedestal.ClipRect, Color.White, _rotation, new Vector2(_texPedestal.Width * justify.X, _texPedestal.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
        }

        private void OnShake(Vector2 amount)
        {
            _imageOffset += amount;
        }

        private void OnEnable()
        {
            Collidable = Visible = true;
        }

        private void OnDisable()
        {
            Collidable = Visible = false;
        }

        private void OnDestroy()
        {
            Collidable = Visible = false;
        }

        private float _rotation;
        private Vector2 _pedestalDirection;
        private MTexture _texIdle;
        private MTexture _texPressed;
        private MTexture _texPedestal;
        private Vector2 _imageOffset = Vector2.Zero;
    }
}
