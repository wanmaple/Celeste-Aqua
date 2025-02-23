using Celeste.Mod.Aqua.Miscellaneous;
using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class EntityExtensions
    {
        public static void Initialize()
        {
            On.Monocle.Entity.ctor_Vector2 += Entity_Construct;
            On.Monocle.Entity.Awake += Entity_Awake;
        }

        public static void Uninitialize()
        {
            On.Monocle.Entity.ctor_Vector2 -= Entity_Construct;
            On.Monocle.Entity.Awake -= Entity_Awake;
        }

        private static void Entity_Construct(On.Monocle.Entity.orig_ctor_Vector2 orig, Entity self, Vector2 position)
        {
            orig(self, position);
            self.SetHookable(false);
            self.SetAttachCallbacks(null, null);
            DynamicData.For(self).Set("hook_attached", false);
            DynamicData.For(self).Set("can_collide_method", null);
            DynamicData.For(self).Set("unique_id", AUTO_ID++);
            DynamicData.For(self).Set("prev_position", self.Position);
            self.WorkWithCardinalBumper();
        }

        private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("prev_position", self.Position);
        }

        public static Entity CollideFirst(this Entity self, Type type, IReadOnlyList<Type> excludeTypes)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    bool excluded = false;
                    foreach (Type excludeType in excludeTypes)
                    {
                        if (entity.GetType().IsAssignableTo(excludeType))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded) continue;
                    if (Collide.Check(self, entity))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static Entity CollideFirst(this Entity self, Type type)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (Collide.Check(self, entity))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static Entity CollideFirstOutside(this Entity self, Type type, Vector2 at, IReadOnlyList<Type> excludeTypes)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    bool excluded = false;
                    foreach (Type excludeType in excludeTypes)
                    {
                        if (entity.GetType().IsAssignableTo(excludeType))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded) continue;
                    if (!Collide.Check(self, entity) && Collide.Check(self, entity, at))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static Entity CollideFirstOutside(this Entity self, Type type, Vector2 at)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (!Collide.Check(self, entity) && Collide.Check(self, entity, at))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static void MakeSelfEnableGrabToPlayer(this Entity self)
        {
            self.SetHookable(true);
            var moveToward = new MoveToward(null, true);
            moveToward.Active = false;
            self.Add(moveToward);
            DynamicData.For(self).Set("move_toward", moveToward);
            HookInteractable com = self.Get<HookInteractable>();
            if (com != null)
            {
                com.Interaction = self.OnSimpleHookToPlayer;
            }
            else
            {
                self.Add(new HookInteractable(self.OnSimpleHookToPlayer));
            }
        }

        private static bool OnSimpleHookToPlayer(this Entity self, GrapplingHook hook, Vector2 at)
        {
            if (hook.State == GrapplingHook.HookStates.Emitting || hook.State == GrapplingHook.HookStates.Bouncing)
            {
                hook.Revoke();
                MoveToward moveToward = DynamicData.For(self).Get<MoveToward>("move_toward");
                moveToward.Target = hook;
                moveToward.Active = true;
                return true;
            }
            return false;
        }

        public static ulong GetUniqueID(this Entity self)
        {
            return DynamicData.For(self).Get<ulong>("unique_id");
        }

        public static Entity GetHoldableContainer(this Entity self)
        {
            Component containerRef = null;
            if (ModInterop.ContainerRefType != null && (containerRef = self.GetComponent(ModInterop.ContainerRefType)) != null)
            {
                FieldInfo fieldContainers = ModInterop.ContainerRefType.FindField(BindingFlags.Public | BindingFlags.Instance, "containers");
                if (fieldContainers != null)
                {
                    IEnumerable containers = fieldContainers.GetValue(containerRef) as IEnumerable;
                    if (containers != null)
                    {
                        object container = null;
                        foreach (object c in containers)
                        {
                            container = c;
                            break;
                        }
                        if (container != null && ModInterop.HoldableType != null && container.GetType().IsAssignableTo(ModInterop.HoldableType))
                        {
                            return container as Entity;
                        }
                    }
                }
            }
            return null;
        }

        public static Vector2 GetPreviousPosition(this Entity self)
        {
            Vector2 prevPos = DynamicData.For(self).Get<Vector2>("prev_position");
            return prevPos;
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
            bool isAttached = self.IsHookAttached();
            if (isAttached != attached)
            {
                DynamicData.For(self).Set("hook_attached", attached);
                if (self.Scene != null)
                {
                    GrapplingHook grapple = self.Scene.Tracker.GetEntity<GrapplingHook>();
                    if (attached)
                        self.AttachCallback(grapple);
                    else
                        self.DetachCallback(grapple);
                }
            }
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

        public static Component GetComponent(this Entity self, Type comType)
        {
            foreach (Component com in self.Components)
            {
                if (com.GetType().IsAssignableTo(comType))
                    return com;
            }
            return null;
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

        public static void AttachCallback(this Entity self, GrapplingHook grapple)
        {
            Action<GrapplingHook> callback = DynamicData.For(self).Get<Action<GrapplingHook>>("on_attach_callback");
            callback?.Invoke(grapple);
        }

        public static void DetachCallback(this Entity self, GrapplingHook grapple)
        {
            Action<GrapplingHook> callback = DynamicData.For(self).Get<Action<GrapplingHook>>("on_detach_callback");
            callback?.Invoke(grapple);
        }

        public static void SetAttachCallbacks(this Entity self, Action<GrapplingHook> onAttach, Action<GrapplingHook> onDetach)
        {
            DynamicData.For(self).Set("on_attach_callback", onAttach);
            DynamicData.For(self).Set("on_detach_callback", onDetach);
        }

        public static void MakeExtraCollideCondition(this Entity self)
        {
            MethodInfo method = self.GetType().GetMethod("CanCollide", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            DynamicData.For(self).Set("can_collide_method", method);
        }

        public static void MakeExtraCollideCondition(this Entity self, MethodInfo method)
        {
            DynamicData.For(self).Set("can_collide_method", method);
        }

        public static bool CheckCollidePlatformsAtXDirection(this Entity self, float movement, out Entity collideEntity)
        {
            collideEntity = null;
            int sign = MathF.Sign(movement);
            if (sign == 0)
                return false;
            if ((collideEntity = self.CollideFirst<Solid>(self.Position + Vector2.UnitX * movement)) != null)
                return true;
            var sidewaysTypes = ModInterop.SidewaysJumpthruTypes;
            for (int j = 0; j < sidewaysTypes.Count; j++)
            {
                Type sidewaysType = sidewaysTypes[j];
                var sideJumpthrus = self.Scene.Tracker.Entities[sidewaysType];
                if (sideJumpthrus.Count > 0)
                {
                    FieldInfo fieldLeft2Right = sidewaysType.GetField("AllowLeftToRight", BindingFlags.Instance | BindingFlags.Public);
                    if (fieldLeft2Right != null)
                    {
                        foreach (Entity jumpthru in sideJumpthrus)
                        {
                            bool left2right = (bool)fieldLeft2Right.GetValue(jumpthru);
                            if ((!left2right && sign > 0) || (left2right && sign < 0))
                            {
                                jumpthru.SetHookable(true);
                                if (!Collide.Check(self, jumpthru) && Collide.Check(self, jumpthru, self.Position + Vector2.UnitX * movement))
                                {
                                    collideEntity = jumpthru;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckCollidePlatformsAtYDirection(this Entity self, float movement, out Entity collideEntity)
        {
            collideEntity = null;
            int sign = MathF.Sign(movement);
            if (sign == 0)
                return false;
            if ((collideEntity = self.CollideFirst<Solid>(self.Position + Vector2.UnitY * movement)) != null)
                return true;
            if (sign > 0)
            {
                collideEntity = self.CollideFirstOutside(typeof(JumpThru), self.Position + Vector2.UnitY * movement, ModInterop.DownsideJumpthruTypes) as Platform;
                if (collideEntity != null)
                {
                    return true;
                }
            }
            else
            {
                var downsideTypes = ModInterop.DownsideJumpthruTypes;
                for (int i = 0; i < downsideTypes.Count; i++)
                {
                    Type downsideType = downsideTypes[i];
                    collideEntity = self.CollideFirstOutside(downsideType, self.Position + Vector2.UnitY * movement) as Platform;
                    if (collideEntity != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void WorkWithCardinalBumper(this Entity self)
        {
            if (ModInterop.CardinalBumperType == null)
                return;
            if (self.GetType().IsAssignableTo(ModInterop.CardinalBumperType))
            {
                self.SetHookable(true);
                self.Add(new HookInteractable(self.OnInteractCardinalBumper));
            }
        }

        private static bool OnInteractCardinalBumper(this Entity self, GrapplingHook grapple, Vector2 at)
        {
            Vector2 direction = Calc.SafeNormalize(at - self.Center);
            direction = AquaMaths.TurnToDirection4(direction);
            grapple.BounceTo(direction);
            self.CardinalBumperHit(direction);
            return true;
        }

        private static void CardinalBumperHit(this Entity self, Vector2 direction)
        {
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Audio.Play("event:/game/06_reflection/pinballbumper_hit", self.Center);
            FieldInfo fieldSprite = ModInterop.CardinalBumperType.GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldSprite != null)
            {
                Sprite sprite = fieldSprite.GetValue(self) as Sprite;
                if (sprite != null)
                {
                    sprite.Play("hit", true);
                }
            }
            FieldInfo fieldRespawnTimer = ModInterop.CardinalBumperType.GetField("respawnTimer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fieldRespawnTimer != null)
            {
                fieldRespawnTimer.SetValue(self, 0.7f);
            }
            SlashFx.Burst(self.Center, direction.Angle());
            MethodInfo methodNextNode = ModInterop.CardinalBumperType.GetMethod("NextNode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (methodNextNode != null)
            {
                FieldInfo fieldLinked = ModInterop.CardinalBumperType.GetField("Linked", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldLinked != null)
                {
                    bool linked = (bool)fieldLinked.GetValue(self);
                    if (linked)
                    {
                        foreach (Entity entity in self.Scene.Tracker.Entities[ModInterop.CardinalBumperType])
                        {
                            if ((bool)fieldLinked.GetValue(entity))
                            {
                                methodNextNode.Invoke(entity, null);
                            }
                        }
                        return;
                    }
                }
                methodNextNode.Invoke(self, null);
            }
        }

        private static ulong AUTO_ID = 0u;
    }
}
