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
            On.Monocle.Entity.Added += Entity_Added;
        }

        public static void Uninitialize()
        {
            On.Monocle.Entity.ctor_Vector2 -= Entity_Construct;
            On.Monocle.Entity.Awake -= Entity_Awake;
            On.Monocle.Entity.Added -= Entity_Added;
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
            DynamicData.For(self).Set("accelerate_state", AccelerationArea.AccelerateState.None);
            DynamicData.For(self).Set("reversed", false);
            DynamicData.For(self).Set("post_move_patch", null);
        }

        private static void Entity_Added(On.Monocle.Entity.orig_Added orig, Entity self, Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("prev_position", self.Position);
            self.WorkWithCardinalBumper();
            self.WorkWithSidewaysJumpThrough();
            self.WorkWithElectricTypes();
        }

        private static void Entity_Awake(On.Monocle.Entity.orig_Awake orig, Entity self, Scene scene)
        {
            orig(self, scene);
            DynamicData.For(self).Set("prev_position", self.Position);
        }

        public static Entity CollideFirst(this Entity self, Type type, IReadOnlyList<Type> excludeTypes, params Entity[] ignoreList)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (Array.IndexOf(ignoreList, entity) >= 0)
                        continue;
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

        public static Entity CollideFirst(this Entity self, Type type, params Entity[] ignoreList)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (Array.IndexOf(ignoreList, entity) >= 0)
                        continue;
                    if (Collide.Check(self, entity))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static Entity CollideFirstWithout<T>(this Entity self, Vector2 at, params Entity[] ignoreList) where T : Entity
        {
            Entity ret = null;
            var entities = self.Scene.Tracker.GetEntities<T>();
            foreach (Entity entity in entities)
            {
                if (Array.IndexOf(ignoreList, entity) >= 0)
                    continue;
                Vector2 position = self.Position;
                self.Position = at;
                if (Collide.Check(self, entity))
                {
                    ret = entity;
                }
                self.Position = position;
                if (ret != null)
                    break;
            }
            return ret;
        }

        public static Entity CollideFirstOutside(this Entity self, Type type, Vector2 at, IReadOnlyList<Type> excludeTypes, params Entity[] ignoreList)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (Array.IndexOf(ignoreList, entity) >= 0)
                        continue;
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

        public static Entity CollideFirstOutside(this Entity self, Type type, Vector2 at, params Entity[] ignoreList)
        {
            if (self.Scene.Tracker.Entities.TryGetValue(type, out var entities))
            {
                foreach (Entity entity in entities)
                {
                    if (Array.IndexOf(ignoreList, entity) >= 0)
                        continue;
                    if (!Collide.Check(self, entity) && Collide.Check(self, entity, at))
                    {
                        return entity;
                    }
                }
            }
            return null;
        }

        public static Entity CollideFirstOutsideWithout<T>(this Entity self, Vector2 at, params Entity[] ignoreList) where T : Entity
        {
            var entities = self.Scene.Tracker.GetEntities<T>();
            foreach (Entity entity in entities)
            {
                if (Array.IndexOf(ignoreList, entity) >= 0)
                    continue;
                if (!Collide.Check(self, entity) && Collide.Check(self, entity, at))
                {
                    return entity;
                }
            }
            return null;
        }

        public static bool CheckColliderChanged(this Entity self, GrapplingHook grapple, Vector2 direction)
        {
            if (Collide.Check(grapple, self, grapple.Position + direction))
                return true;
            return false;
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
            if (hook.IsShooting)
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
                        if (container != null && ModInterop.HoldableContainerType != null && container.GetType().IsAssignableTo(ModInterop.HoldableContainerType))
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

        public static void SetHookAttached(this Entity self, bool attached, GrapplingHook grapple)
        {
            bool isAttached = self.IsHookAttached();
            if (isAttached != attached)
            {
                DynamicData.For(self).Set("hook_attached", attached);
                if (self.Scene != null)
                {
                    if (attached)
                        self.AttachCallback(grapple);
                    else
                        self.DetachCallback(grapple);
                }
            }
        }

        public static AccelerationArea.AccelerateState GetAccelerateState(this Entity self)
        {
            return DynamicData.For(self).Get<AccelerationArea.AccelerateState>("accelerate_state");
        }

        public static void SetAccelerateState(this Entity self, AccelerationArea.AccelerateState state)
        {
            DynamicData.For(self).Set("accelerate_state", state);
        }

        public static bool IsReversed(this Entity self)
        {
            return DynamicData.For(self).Get<bool>("reversed");
        }

        public static void SetReversed(this Entity self, bool reversed)
        {
            DynamicData.For(self).Set("reversed", reversed);
        }

        public static bool IntersectsWithRope(this Entity self)
        {
            return self.IntersectsWithRopeAndReturnGrapple() != null;
        }

        public static GrapplingHook IntersectsWithRopeAndReturnGrapple(this Entity self)
        {
            var grapples = self.Scene.Tracker.GetEntities<GrapplingHook>();
            foreach (GrapplingHook grapple in grapples)
            {
                if (grapple.IsRopeIntersectsWith(self))
                {
                    return grapple;
                }
            }
            return null;
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

        public static void PostMovePatch(this Entity self, Vector2 movement)
        {
            Action<Vector2> action = DynamicData.For(self).Get<Action<Vector2>>("post_move_patch");
            action?.Invoke(movement);
        }

        public static IEnumerator UndraggableRoutine(this Entity self, Sprite sprite, Vector2 direction, float duration, float distance)
        {
            float elapsed = 0.0f;
            Vector2 movement = direction * distance;
            Vector2 origin = self.Position;
            Vector2 oldRenderPos = sprite.RenderPosition;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                float t = elapsed / duration;
                t = MathF.Sqrt(t);
                Vector2 offset = AquaMaths.Lerp(Vector2.Zero, movement, t);
                sprite.RenderPosition = oldRenderPos + offset;
                float currentDistance = Vector2.Distance(sprite.RenderPosition, origin);
                if (currentDistance > distance)
                {
                    sprite.RenderPosition = origin + Calc.SafeNormalize(sprite.RenderPosition - origin) * distance;
                    break;
                }
                yield return null;
            }

            duration = 0.2f;
            elapsed = 0.0f;
            Vector2 begin = sprite.RenderPosition;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                float t = elapsed / duration;
                sprite.RenderPosition = AquaMaths.Lerp(begin, origin, t);
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

        public static Action<GrapplingHook> GetAttachCallback(this Entity self)
        {
            return DynamicData.For(self).Get<Action<GrapplingHook>>("on_attach_callback");
        }

        public static Action<GrapplingHook> GetDetachCallback(this Entity self)
        {
            return DynamicData.For(self).Get<Action<GrapplingHook>>("on_detach_callback");
        }

        public static void SetAttachCallbacks(this Entity self, Action<GrapplingHook> onAttach, Action<GrapplingHook> onDetach)
        {
            DynamicData.For(self).Set("on_attach_callback", onAttach);
            DynamicData.For(self).Set("on_detach_callback", onDetach);
        }

        public static void MakeGrappleFollowMe(this Entity self, Vector2 exactMovement, Vector2 pixelMovement)
        {
            if (exactMovement == Vector2.Zero)
                return;
            if (self.Scene == null)
                return;
            var grapples = self.Scene.Tracker.GetEntities<GrapplingHook>();
            foreach (GrapplingHook grapple in grapples)
            {
                grapple.PivotsFollowAttachment(self, exactMovement, pixelMovement);
            }
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

        public static bool CheckCollidePlatformsAtXDirection(this Entity self, float movement, out Entity collideEntity, params Entity[] ignoreList)
        {
            collideEntity = null;
            int sign = MathF.Sign(movement);
            if (sign == 0)
                return false;
            if ((collideEntity = self.CollideFirstWithout<Solid>(self.Position + Vector2.UnitX * movement, ignoreList)) != null)
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
                            if (Array.IndexOf(ignoreList, jumpthru) >= 0)
                                continue;
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
            var wallBoosters = self.Scene.Tracker.GetEntities<WallBooster>();
            foreach (WallBooster entity in wallBoosters)
            {
                if (Array.IndexOf(ignoreList, entity) >= 0)
                    continue;
                // why the facing of wall booster confuses me so much?
                if ((entity.Facing == Facings.Left && sign < 0)||(entity.Facing == Facings.Right && sign > 0))
                {
                    if (Collide.Check(self, entity, self.Position + Vector2.UnitX * movement))
                    {
                        collideEntity = entity;
                        return true;
                    }
                }
            }
            if (self.Get<Holdable>() != null)
            {
                if (ModInterop.HoldableBarrierType != null)
                {
                    if (self.Scene.Tracker.Entities.TryGetValue(ModInterop.HoldableBarrierType, out var barriers))
                    {
                        foreach (var barrier in barriers)
                        {
                            if (Array.IndexOf(ignoreList, barrier) >= 0)
                                continue;
                            barrier.Collidable = true;
                            if (Collide.Check(self, barrier, self.Position + Vector2.UnitX * movement))
                            {
                                barrier.Collidable = false;
                                collideEntity = barrier;
                                return true;
                            }
                            barrier.Collidable = false;
                        }
                    }
                }
            }
            return false;
        }

        public static bool CheckCollidePlatformsAtYDirection(this Entity self, float movement, out Entity collideEntity, params Entity[] ignoreList)
        {
            collideEntity = null;
            int sign = MathF.Sign(movement);
            if (sign == 0)
                return false;
            if ((collideEntity = self.CollideFirstWithout<Solid>(self.Position + Vector2.UnitY * movement, ignoreList)) != null)
                return true;
            if (sign > 0)
            {
                collideEntity = self.CollideFirstOutside(typeof(JumpThru), self.Position + Vector2.UnitY * movement, ModInterop.DownsideJumpthruTypes, ignoreList) as Platform;
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
                    collideEntity = self.CollideFirstOutside(downsideType, self.Position + Vector2.UnitY * movement, ignoreList) as Platform;
                    if (collideEntity != null)
                    {
                        return true;
                    }
                }
            }
            if (self.Get<Holdable>() != null)
            {
                if (ModInterop.HoldableBarrierType != null)
                {
                    if (self.Scene.Tracker.Entities.TryGetValue(ModInterop.HoldableBarrierType, out var barriers))
                    {
                        foreach (var barrier in barriers)
                        {
                            if (Array.IndexOf(ignoreList, barrier) >= 0)
                                continue;
                            barrier.Collidable = true;
                            if (Collide.Check(self, barrier, self.Position + Vector2.UnitY * movement))
                            {
                                barrier.Collidable = false;
                                collideEntity = barrier;
                                return true;
                            }
                            barrier.Collidable = false;
                        }
                    }
                }
                if (movement > 0.0f && ModInterop.HoldableBarrierJumpThruType != null)
                {
                    if (self.Scene.Tracker.Entities.TryGetValue(ModInterop.HoldableBarrierJumpThruType, out var barriers))
                    {
                        foreach (var barrier in barriers)
                        {
                            if (Array.IndexOf(ignoreList, barrier) >= 0)
                                continue;
                            barrier.Collidable = true;
                            if (!Collide.Check(self, barrier) && Collide.Check(self, barrier, self.Position + Vector2.UnitY * movement))
                            {
                                barrier.Collidable = false;
                                collideEntity = barrier;
                                return true;
                            }
                            barrier.Collidable = false;
                        }
                    }
                }
            }
            return false;
        }

        private static void WorkWithSidewaysJumpThrough(this Entity self)
        {
            // sideways jump throughs might be attached to solids which might do movement, we have to add its movement to grapples.
            if (ModInterop.SidewaysJumpthruTypes == null)
                return;
            foreach (Type sidewaysType in ModInterop.SidewaysJumpthruTypes)
            {
                if (self.GetType().IsAssignableTo(sidewaysType))
                {
                    foreach (Component com in self.Components)
                    {
                        if (com is StaticMover mover)
                        {
                            Action<Vector2> onMove = mover.OnMove;
                            mover.OnMove = move =>
                            {
                                Vector2 oldPosition = self.Position;
                                onMove?.Invoke(move);
                                Vector2 newPosition = self.Position;
                                Vector2 movement = newPosition - oldPosition;
                                self.MakeGrappleFollowMe(movement, movement);
                            };
                            break;
                        }
                    }
                    break;
                }
            }
        }

        private static void WorkWithElectricTypes(this Entity self)
        {
            if (ModInterop.ElectricEntityTypes != null)
            {
                foreach (Type type in ModInterop.ElectricEntityTypes)
                {
                    if (self.GetType().IsAssignableTo(type))
                    {
                        self.Add(new HookInOut(self.OnHookEnterElectricity, self.OnHookOutElectricity));
                        self.Add(new ElectricShockInLightning());
                    }
                }
            }
        }

        internal static void OnHookEnterElectricity(this Entity self, GrapplingHook hook)
        {
            hook.ElectricShocking = true;
            if (hook.Material == GrapplingHook.RopeMaterial.Metal && hook.State != GrapplingHook.HookStates.Revoking)
            {
                Player player = hook.Owner;
                player.StateMachine.ForceState((int)AquaStates.StElectricShock);
            }
        }

        internal static void OnHookOutElectricity(this Entity self, GrapplingHook hook)
        {
            if (hook != null)
            {
                hook.ElectricShocking = false;
            }
        }

        internal static void CheckRopeIntersectionOfElectricity(this Entity self)
        {
            GrapplingHook grapple = null;
            if ((grapple = self.IntersectsWithRopeAndReturnGrapple()) != null)
            {
                if (grapple.Material == GrapplingHook.RopeMaterial.Metal && grapple.State != GrapplingHook.HookStates.Revoking)
                {
                    Player player = grapple.Owner;
                    if (player.StateMachine.State != (int)AquaStates.StElectricShock)
                    {
                        player.StateMachine.ForceState((int)AquaStates.StElectricShock);
                    }
                }
            }
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
