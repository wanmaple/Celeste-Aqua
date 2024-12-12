using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityExtensions
    {
        public static void Initialize()
        {
            On.Monocle.Entity.ctor_Vector2 += Entity_Construct;
            On.Monocle.Entity.Awake += Entity_Awake;
            On.Monocle.Entity.Update += Entity_Update;
        }

        public static void Uninitialize()
        {
            On.Monocle.Entity.ctor_Vector2 -= Entity_Construct;
            On.Monocle.Entity.Awake -= Entity_Awake;
            On.Monocle.Entity.Update -= Entity_Update;
        }

        private static void Entity_Construct(On.Monocle.Entity.orig_ctor_Vector2 orig, Entity self, Vector2 position)
        {
            orig(self, position);
            self.SetHookable(false);
            self.SetHookAttached(false);
            DynamicData.For(self).Set("can_collide_method", null);
        }

        private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("prev_position", self.Position);
        }

        private static void Entity_Update(On.Monocle.Entity.orig_Update orig, Entity self)
        {
            DynamicData.For(self).Set("prev_position", self.Position);
            orig(self);
        }

        public static Vector2 GetPreviousPosition(this Entity self)
        {
            return DynamicData.For(self).Get<Vector2>("prev_position");
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

        public static TimeTicker GetTimeTicker(this Entity self, string name)
        {
            return DynamicData.For(self).Get<TimeTicker>(name);
        }

        public static void SetTimeTicker(this Entity self, string name, float duration)
        {
            DynamicData.For(self).Set(name, new TimeTicker(duration));
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

        public static bool CanCollide(this Entity self, Entity other)
        {
            MethodInfo method = DynamicData.For(self).Get<MethodInfo>("can_collide_method");
            if (method == null)
                return true;
            return (bool)method.Invoke(self, new object[] { other });
        }

        public static void MakeExtraCollideCondition(this Entity self)
        {
            MethodInfo method = self.GetType().GetMethod("CanCollide", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            DynamicData.For(self).Set("can_collide_method", method);
        }
    }
}
