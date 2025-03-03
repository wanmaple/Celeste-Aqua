﻿using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
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
        }

        private static void Actor_Update(On.Celeste.Actor.orig_Update orig, Actor self)
        {
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
            Vector2 toActor = direction;
            float mySaveSpeed = MathF.Max(Vector2.Dot(mySpeed, -toActor), 0.0f);
            float otherSaveSpeed = MathF.Max(Vector2.Dot(otherSpeed, toActor), 0.0f);
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
                bool blocked = false;
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
            var state = self.SceneAs<Level>().GetState();
            return new MomentumResults
            {
                OwnerSpeed = MathF.Min(myRatio * totalSpeed + mySaveSpeed, state.HookSettings.MaxLineSpeed) * -toActor,
                OtherSpeed = MathF.Min(otherRatio * totalSpeed + otherSaveSpeed, state.HookSettings.MaxLineSpeed) * toActor,
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
                    hook.Revoke();
                    fieldNoGravityTimer.SetValue(self, 0.15f);
                    Vector2 entitySpeed = (Vector2)fieldSpeed.GetValue(self);
                    var result = self.HandleMomentumOfActor(player, entitySpeed, player.Speed, hook.ShootDirection);
                    fieldSpeed.SetValue(self, result.OwnerSpeed);
                    Vector2 oldSpeed = player.Speed;
                    player.Speed = result.OtherSpeed;
                    player.Stamina = MathF.Max(player.Stamina - self.GetStaminaCost(), 0.0f);
                    Celeste.Freeze(0.05f);
                    Audio.Play("event:/char/madeline/jump_superslide", player.Center);
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
