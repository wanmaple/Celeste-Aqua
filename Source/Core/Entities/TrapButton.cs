using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Trap Button")]
    [Tracked(false)]
    public class TrapButton : Entity
    {
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
                    Collider = new Hitbox(14.0f, 4.0f, -7.0f, -4.0f);
                    _rotation = MathF.PI;
                    _pedestalDirection = -Vector2.UnitY;
                    break;
                case "Left":
                    Collider = new Hitbox(4.0f, 14.0f, 0.0f, -7.0f);
                    _rotation = -MathF.PI * 0.5f;
                    _pedestalDirection = Vector2.UnitX;
                    break;
                case "Right":
                    Collider = new Hitbox(4.0f, 14.0f, -4.0f, -7.0f);
                    _rotation = MathF.PI * 0.5f;
                    _pedestalDirection = -Vector2.UnitX;
                    break;
                case "Up":
                default:
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
        }

        public override void Update()
        {
            base.Update();
            Entity collideEntity = CollideFirst<Solid>();
            if (collideEntity == null)
                collideEntity = CollideFirst<Player>();
            if (collideEntity == null)
                collideEntity = CollideFirst<GrapplingHook>();
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
                Draw.SpriteBatch.Draw(_texPressed.Texture.Texture_Safe, Position + _pedestalDirection, _texPressed.ClipRect, Color, _rotation, new Vector2(_texPressed.Width * justify.X, _texPressed.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
            }
            else
            {
                Draw.SpriteBatch.Draw(_texIdle.Texture.Texture_Safe, Position, _texIdle.ClipRect, Color, _rotation, new Vector2(_texIdle.Width * justify.X, _texIdle.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
            }
            Draw.SpriteBatch.Draw(_texPedestal.Texture.Texture_Safe, Position + _pedestalDirection * 3.0f, _texPedestal.ClipRect, Color.White, _rotation, new Vector2(_texPedestal.Width * justify.X, _texPedestal.Height * justify.Y), 1.0f, SpriteEffects.None, depth);
        }

        private float _rotation;
        private Vector2 _pedestalDirection;
        private MTexture _texIdle;
        private MTexture _texPressed;
        private MTexture _texPedestal;
    }
}
