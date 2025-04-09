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
    public static class ActorExtensions
    {
        public struct MomentumResults
        {
            public Vector2 OwnerSpeed;
            public Vector2 OtherSpeed;
        }

        public static void Initialize()
        {
            On.Celeste.Actor.ctor += Actor_Construct;
            On.Celeste.Actor.Update += Actor_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Actor.ctor -= Actor_Construct;
            On.Celeste.Actor.Update -= Actor_Update;
        }

        private static void Actor_Construct(On.Celeste.Actor.orig_ctor orig, Actor self, Vector2 position)
        {
            orig(self, position);
            self.Add(new ActorExtraFields());
            self.SetMass(PlayerStates.MADELINE_MASS);
            self.SetHookable(true);
            DynamicData.For(self).Set("eevee_patched", false);
        }

        private static void Actor_Update(On.Celeste.Actor.orig_Update orig, Actor self)
        {
            self.PatchNonPlatformHoldableContainer();
            DynamicData.For(self).Set("prev_position", self.Position);
            orig(self);
        }

        public static float GetMass(this Actor self)
        {
            return self.Get<ActorExtraFields>().Mass;
        }

        public static void SetMass(this Actor self, float mass)
        {
            self.Get<ActorExtraFields>().Mass = mass;
        }

        public static float GetStaminaCost(this Actor self)
        {
            return self.Get<ActorExtraFields>().StaminaCost;
        }

        public static void SetStaminaCost(this Actor self, float cost)
        {
            self.Get<ActorExtraFields>().StaminaCost = cost;
        }

        public static float GetAgainstBoostCoefficient(this Actor self)
        {
            return self.Get<ActorExtraFields>().AgainstBoostCoefficient;
        }

        public static void SetAgainstBoostCoefficient(this Actor self, float coeff)
        {
            self.Get<ActorExtraFields>().AgainstBoostCoefficient = coeff;
        }

        public static MomentumResults HandleMomentumOfActor(this Actor self, Actor other, Vector2 mySpeed, Vector2 otherSpeed, Vector2 direction)
        {
            //Vector2 toActor = Calc.SafeNormalize(self.Center - other.Center, Vector2.UnitX, 1.0f);
            bool blocked = false;
            Vector2 toActor = direction;
            float mySaveSpeed = MathF.Max(Vector2.Dot(mySpeed, -toActor), 0.0f);
            float otherSaveSpeed = MathF.Max(Vector2.Dot(otherSpeed, toActor), 0.0f);
            mySaveSpeed = MathF.Round(mySaveSpeed / 20.0f) * 20.0f;
            otherSaveSpeed = MathF.Round(otherSaveSpeed / 20.0f) * 20.0f;
            float myMass = self.GetMass();
            float otherMass = other.GetMass();
            float myRatio = 0.0f, otherRatio = 0.0f;
            float totalSpeed = self.SceneAs<Level>().GetState().HookSettings.ActorPullForce;
            if (myMass == 0.0f && otherMass == 0.0f)
            {
                myRatio = otherRatio = 0.5f;
            }
            else if (myMass == float.PositiveInfinity && otherMass == float.PositiveInfinity)
            {
                myRatio = otherRatio = 0.0f;
            }
            else if (myMass == 0.0f)
            {
                myRatio = 1.0f;
                otherRatio = 0.0f;
            }
            else if (myMass == float.PositiveInfinity)
            {
                myRatio = 0.0f;
                otherRatio = 1.0f;
            }
            else if (otherMass == 0.0f)
            {
                myRatio = 0.0f;
                otherRatio = 1.0f;
            }
            else if (otherMass == float.PositiveInfinity)
            {
                myRatio = 1.0f;
                otherRatio = 0.0f;
            }
            else
            {
                // if the actor will be blocked by the platform, then Madeline will gain a defined 'against-wall boost'.
                Vector2 dir8 = AquaMaths.TurnToDirection8(-toActor);
                int signX = MathF.Sign(dir8.X);
                int signY = MathF.Sign(dir8.Y);
                for (int i = 1; i <= 4; ++i)
                {
                    if (signX != 0)
                    {
                        Entity collideEntity = null;
                        if (self.CheckCollidePlatformsAtXDirection(signX * i, out collideEntity))
                        {
                            blocked = true;
                            break;
                        }
                    }
                    if (signY != 0)
                    {
                        Entity collideEntity = null;
                        if (self.CheckCollidePlatformsAtYDirection(signY * i, out collideEntity))
                        {
                            blocked = true;
                            break;
                        }
                    }
                }
                if (blocked)
                {
                    myRatio = otherMass / (myMass + otherMass);
                    otherRatio = self.GetAgainstBoostCoefficient();
                }
                else
                {
                    myRatio = otherMass / (myMass + otherMass);
                    otherRatio = myMass / (myMass + otherMass);
                }
            }
            if (other is Player player)
            {
                // if player is at dash state, immediately cancel it.
                if (player.StateMachine.State == (int)AquaStates.StDash)
                {
                    player.StateMachine.State = (int)AquaStates.StNormal;
                }
            }
            var state = self.SceneAs<Level>().GetState();
            Vector2 ownerSpd = myRatio * totalSpeed * -toActor; // ignore addition speed for consistency.
            Vector2 otherSpd = (otherRatio * totalSpeed + otherSaveSpeed) * toActor;
            float extraCoeff = blocked ? MathF.Max(0.0f, self.GetAgainstBoostCoefficient() - myMass / (myMass + otherMass)) : 0.0f;
            float slowFallMax = Player.DashSpeed * (1.0f + extraCoeff);
            float fastFallMax = Player.DashSpeed * (1.0f + extraCoeff);
            if (self.Get<Holdable>().SlowFall && otherSpd.Y < -slowFallMax)
            {
                otherSpd.Y = -slowFallMax;
            }
            else if (!self.Get<Holdable>().SlowFall && otherSpd.Y < -fastFallMax)
            {
                otherSpd.Y = -fastFallMax;
            }
            return new MomentumResults
            {
                OwnerSpeed = ownerSpd,   // ignore addition speed for consistency.
                OtherSpeed = otherSpd,
            };
        }

        public static bool GeneralHoldableInteraction(this Actor self, GrapplingHook hook, Vector2 at)
        {
            Player player = hook.Owner;
            if (player != null)
            {
                FieldInfo fieldNoGravityTimer = self.GetType().FindField(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, "noGravityTimer", "_noGravityTimer");
                FieldInfo fieldSpeed = self.GetType().FindField(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, "Speed");
                if (fieldNoGravityTimer != null && fieldSpeed != null && fieldSpeed.FieldType == typeof(Vector2))
                {
                    if (self is Glider jelly)
                    {
                        if (jelly.bubble)
                        {
                            jelly.OnPickup();
                        }
                    }
                    else if (ModInterop.PlatformJellyType != null && self.GetType().IsAssignableTo(ModInterop.PlatformJellyType))
                    {
                        FieldInfo fieldBubble = self.GetType().FindField(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, "bubble");
                        MethodInfo methodOnPickup = self.GetType().GetMethod("OnPickup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (fieldBubble != null && methodOnPickup != null)
                        {
                            bool bubble = (bool)fieldBubble.GetValue(self);
                            if (bubble)
                            {
                                methodOnPickup.Invoke(self, null);
                            }
                        }
                    }
                    fieldNoGravityTimer.SetValue(self, 0.15f);
                    Vector2 entitySpeed = (Vector2)fieldSpeed.GetValue(self);
                    Vector2 direction = hook.ShootDirection;
                    if (hook.State == GrapplingHook.HookStates.Bouncing && Vector2.DistanceSquared(hook.Owner.Center, self.Center) > 256.0f)
                    {
                        direction = AquaMaths.TurnToDirection8(-hook.HookDirection);
                    }
                    var result = self.HandleMomentumOfActor(player, entitySpeed, player.Speed, direction);
                    fieldSpeed.SetValue(self, result.OwnerSpeed);
                    Vector2 oldSpeed = player.Speed;
                    player.Speed = result.OtherSpeed;
                    player.Stamina = MathF.Max(player.Stamina - self.GetStaminaCost(), 0.0f);
                    hook.Revoke();
                    Celeste.Freeze(0.05f);
                    Audio.Play("event:/char/madeline/jump_superslide", player.Center);
                    return true;
                }
                return false;
            }
            return false;
        }

        private static void PatchNonPlatformHoldableContainer(this Actor self)
        {
            if (DynamicData.For(self).Get<bool>("eevee_patched"))
                return;
            if (ModInterop.HoldableContainerType == null || !self.GetType().IsAssignableTo(ModInterop.HoldableContainerType))
            {
                DynamicData.For(self).Set("eevee_patched", true);
                return;
            }
            // the movement of non-platform holdables made by EeveeHelper can't be tracked.
            Component mover = null;
            if (ModInterop.ContainerMoverType == null || (mover = self.GetComponent(ModInterop.ContainerMoverType)) == null)
            {
                DynamicData.For(self).Set("eevee_patched", true);
                return;
            }
            FieldInfo fieldPreMove = mover.GetType().FindField(BindingFlags.Instance | BindingFlags.Public, "OnPreMove");
            FieldInfo fieldPostMove = mover.GetType().FindField(BindingFlags.Instance | BindingFlags.Public, "OnPostMove");
            MethodInfo methodGetEntities = mover.GetType().FindMethod("GetEntities");
            if (fieldPreMove == null || fieldPostMove == null || methodGetEntities == null)
            {
                DynamicData.For(self).Set("eevee_patched", true);
                return;
            }
            Action onPrevMove = fieldPreMove.GetValue(mover) as Action;
            Action onPostMove = fieldPostMove.GetValue(mover) as Action;
            List<KeyValuePair<Entity, Vector2>> nonPlatforms = new List<KeyValuePair<Entity, Vector2>>(4);
            onPrevMove += () =>
            {
                IEnumerable entities = methodGetEntities.Invoke(mover, null) as IEnumerable;
                nonPlatforms.Clear();
                foreach (Entity entity in entities)
                {
                    if (entity is Platform)
                        continue;
                    nonPlatforms.Add(new KeyValuePair<Entity, Vector2>(entity, entity.Position));
                }
            };
            onPostMove += () =>
            {
                foreach (var pair in nonPlatforms)
                {
                    Vector2 oldPos = pair.Value;
                    Vector2 newPos = pair.Key.Position;
                    Vector2 movement = newPos - oldPos;
                    pair.Key.PostMovePatch(movement);
                }
            };
            fieldPreMove.SetValue(mover, onPrevMove);
            fieldPostMove.SetValue(mover, onPostMove);
            DynamicData.For(self).Set("eevee_patched", true);
        }
    }
}
