using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityExtensions
    {
        public static void Initialize()
        {
            On.Monocle.Entity.ctor_Vector2 += Entity_Construct;
        }

        public static void Uninitialize()
        {
            On.Monocle.Entity.ctor_Vector2 -= Entity_Construct;
        }

        private static void Entity_Construct(On.Monocle.Entity.orig_ctor_Vector2 orig, Entity self, Vector2 position)
        {
            orig(self, position);
            self.SetHookable(false);
            self.SetHookAttached(false);
        }

        public static bool IsHookable(this Entity self)
        {
            return DynamicData.For(self).Get<bool>("hookable");
        }

        public static void SetHookable(this Entity self, bool hookable)
        {
            DynamicData.For(self).Set("hookable", hookable);
        }

        public static bool IsHookAttached(this Entity self)
        {
            return DynamicData.For(self).Get<bool>("hook_attached");
        }

        public static void SetHookAttached(this Entity self, bool attached)
        {
            DynamicData.For(self).Set("hook_attached", attached);
        }

        public static bool IntersectsWithRope(this Entity self)
        {
            GrapplingHook hook = self.Scene.Tracker.GetEntity<GrapplingHook>();
            if (hook != null)
            {
                return hook.IsRopeIntersectsWith(self);
            }
            return false;
        }

        public static IEnumerator UndraggableRoutine(this Entity self, Sprite sprite, Vector2 direction, float duration, float distance)
        {
            float elapsed = 0.0f;
            Vector2 movement = direction * distance;
            Vector2 oldRenderPos = sprite.RenderPosition;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                float t = elapsed / duration;
                t = MathF.Sqrt(t);
                Vector2 offset = AquaMaths.Lerp(Vector2.Zero, movement, t);
                sprite.RenderPosition = oldRenderPos + offset;
                yield return null;
            }

            duration = 0.2f;
            elapsed = 0.0f;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                float t = elapsed / duration;
                Vector2 offset = AquaMaths.Lerp(movement, Vector2.Zero, t);
                sprite.RenderPosition = oldRenderPos + offset;
                yield return null;
            }
        }
    }
}
