using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Custom NPC")]
    [Tracked(false)]
    public class AquaNPC : CustomNPC
    {
        public string Name { get; private set; }
        public string Animation { get; private set; }

        public AquaNPC(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset, id)
        {
            Name = data.Attr("spriteName");
            Animation = data.Attr("animationName");
            Add(Sprite = new Sprite());
            GFX.SpriteBank.CreateOn(Sprite, Name);
            Sprite.Justify = new Vector2(0.5f, 1.0f);
            Sprite.Play(Animation);
            Vector2 scale = Vector2.One;
            if (data.Bool("flipX"))
            {
                scale.X = -1f;
            }
            if (data.Bool("flipY"))
            {
                scale.Y = -1f;
            }
            Sprite.Scale = scale;
        }

        public IEnumerator DummyWalkTo(float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false)
        {
            if (Math.Abs(X - x) > 4f)
            {
                if (walkBackwards)
                {
                    Sprite.Rate = -1f;
                    Sprite.Scale.X = Math.Sign(X - x);
                }
                else
                {
                    Sprite.Scale.X = Math.Sign(x - X);
                }

                while (Math.Abs(x - X) > 4f && Scene != null && (keepWalkingIntoWalls || !CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(x - X))))
                {
                    if (Sprite.Has("walk"))
                        Sprite.Play("walk");
                    else if (Sprite.Has("move"))
                        Sprite.Play("move");
                    else
                        Sprite.Play("idle");
                    X += MathF.Sign(x - X) * 64.0f * speedMultiplier * Engine.DeltaTime;
                    yield return null;
                }

                Sprite.Rate = 1f;
                Sprite.Play("idle");
            }
        }

        public override void Render()
        {
            Components.Render();
        }
    }
}
