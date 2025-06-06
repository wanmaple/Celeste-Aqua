﻿using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System;

namespace Celeste.Mod.Aqua.Core
{
    public static class TheoCrystalExtensions
    {
        public static void Initialize()
        {
            On.Celeste.TheoCrystal.ctor_Vector2 += TheoCrystal_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.TheoCrystal.ctor_Vector2 += TheoCrystal_Construct;
        }

        private static void TheoCrystal_Construct(On.Celeste.TheoCrystal.orig_ctor_Vector2 orig, TheoCrystal self, Vector2 position)
        {
            orig(self, position);
            self.SetMass(PlayerStates.MADELINE_MASS * 2.0f);
            self.SetStaminaCost(30.0f);
            self.SetAgainstBoostCoefficient(0.9f);
            HookInteractable interactable = new HookInteractable(self.OnInteractGrapple);
            interactable.Collider = self.Get<Holdable>().PickupCollider;
            interactable.CollideOutside = true;
            self.Add(interactable);
        }

        public static bool OnInteractGrapple(this TheoCrystal self, GrapplingHook grapple, Vector2 at)
        {
            Player player = grapple.Owner;
            if (player != null)
            {
                self.noGravityTimer = 0.15f;
                Vector2 direction = grapple.ShootDirection;
                if (grapple.State == GrapplingHook.HookStates.Bouncing && Vector2.DistanceSquared(grapple.Owner.Center, self.Center) > 256.0f)
                {
                    direction = AquaMaths.TurnToDirection8(-grapple.HookDirection);
                }
                var result = self.HandleMomentumOfActor(player, self.Speed, player.Speed, direction);
                self.Speed = result.OwnerSpeed;
                player.Speed = result.OtherSpeed;
                player.Stamina = MathF.Max(player.Stamina - self.GetStaminaCost(), 0.0f);
                grapple.Revoke();
                Celeste.Freeze(0.05f);
                Audio.Play("event:/char/madeline/jump_superslide", player.Center);
                return true;
            }
            return false;
        }
    }
}
