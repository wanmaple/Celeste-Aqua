using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    public static class CustomNPCExtensions
    {
        public static IEnumerator DummyWalkTo(this CustomNPC self, float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false)
        {
            if (Math.Abs(self.X - x) > 4f)
            {
                if (walkBackwards)
                {
                    self.Sprite.Rate = -1f;
                    self.Sprite.Scale.X = Math.Sign(self.X - x);
                }
                else
                {
                    self.Sprite.Scale.X = Math.Sign(x - self.X);
                }

                while (Math.Abs(x - self.X) > 4f && self.Scene != null && (keepWalkingIntoWalls || !self.CollideCheck<Solid>(self.Position + Vector2.UnitX * Math.Sign(x - self.X))))
                {
                    self.Sprite.Play("walk");
                    self.X += MathF.Sign(x - self.X) * 64.0f * speedMultiplier * Engine.DeltaTime;
                    yield return null;
                }

                self.Sprite.Rate = 1f;
                self.Sprite.Play("idle");
            }
        }
    }
}
