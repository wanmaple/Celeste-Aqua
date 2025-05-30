﻿using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class GliderExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Glider.ctor_Vector2_bool_bool += Glider_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Glider.ctor_Vector2_bool_bool -= Glider_Construct;
        }

        private static void Glider_Construct(On.Celeste.Glider.orig_ctor_Vector2_bool_bool orig, Glider self, Vector2 position, bool bubble, bool tutorial)
        {
            orig(self, position, bubble, tutorial);
            self.SetMass(PlayerStates.MADELINE_MASS * 0.5f);
            self.SetStaminaCost(20.0f);
            self.SetAgainstBoostCoefficient(0.6f);
            HookInteractable interactable = new HookInteractable(self.OnInteractGrapple);
            interactable.Collider = self.Get<Holdable>().PickupCollider;
            interactable.CollideOutside = true;
            self.Add(interactable);
        }

        public static bool OnInteractGrapple(this Glider self, GrapplingHook hook, Vector2 at)
        {
            Player player = hook.Owner;
            if (player != null)
            {
                if (self.bubble)
                {
                    self.OnPickup();
                }
                self.noGravityTimer = 0.15f;
                Vector2 direction = hook.ShootDirection;
                if (hook.State == GrapplingHook.HookStates.Bouncing && Vector2.DistanceSquared(hook.Owner.Center, self.Center) > 256.0f)
                {
                    direction = AquaMaths.TurnToDirection8(-hook.HookDirection);
                }
                var result = self.HandleMomentumOfActor(player, self.Speed, player.Speed, direction);
                self.Speed = result.OwnerSpeed;
                player.Speed = result.OtherSpeed;
                hook.Revoke();
                Celeste.Freeze(0.05f);
                Audio.Play("event:/char/madeline/jump_superslide", player.Center);
                return true;
            }
            return false;
        }
    }
}
