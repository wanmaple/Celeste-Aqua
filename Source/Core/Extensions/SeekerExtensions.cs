using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Linq;

namespace Celeste.Mod.Aqua.Core
{
    public static class SeekerExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Seeker.ctor_Vector2_Vector2Array += Seeker_Construct;
            On.Celeste.Seeker.Update += Seeker_Update;
            On.Celeste.Seeker.OnCollideH += Seeker_OnCollideH;
        }

        private static void Seeker_Update(On.Celeste.Seeker.orig_Update orig, Seeker self)
        {
            var barriers = self.Scene.Tracker.GetEntities<SeekerExplodeBarrier>();
            foreach (var barrier in barriers)
            {
                barrier.Collidable = true;
            }
            orig(self);
            foreach (var barrier in barriers)
            {
                barrier.Collidable = false;
            }
        }

        public static void Uninitialize()
        {
            On.Celeste.Seeker.ctor_Vector2_Vector2Array -= Seeker_Construct;
            On.Celeste.Seeker.Update -= Seeker_Update;
            On.Celeste.Seeker.OnCollideH -= Seeker_OnCollideH;
        }

        private static void Seeker_Construct(On.Celeste.Seeker.orig_ctor_Vector2_Vector2Array orig, Seeker self, Vector2 position, Vector2[] patrolPoints)
        {
            orig(self, position, patrolPoints);
            self.SetHookable(true);
            self.SetMass(0.5f);
            self.SetStaminaCost(10.0f);
            self.SetAgainstBoostCoefficient(0.9f);
            var interactable = new HookInteractable(self.OnInteractGrapple);
            interactable.Collider = new Circle(12.0f);
            self.Add(interactable);
        }

        private static void Seeker_OnCollideH(On.Celeste.Seeker.orig_OnCollideH orig, Seeker self, CollisionData data)
        {
            if (data.Hit is SeekerExplodeBarrier barrier && self.State == Seeker.StStunned && !self.dead && MathF.Abs(self.Speed.X) >= 100.0f)
            {
                Seeker.RecoverBlast.Spawn(self.Position);
                Seeker.RecoverBlast blast = self.Scene.Entities.ToAdd.Last(e => e is Seeker.RecoverBlast) as Seeker.RecoverBlast;
                blast.AddTag(Tags.FrozenUpdate);
                Celeste.Freeze(0.05f);
                self.DieWithExplosion(barrier);
            }
            else
            {
                orig(self, data);
            }
        }

        private static bool OnInteractGrapple(this Seeker self, GrapplingHook grapple, Vector2 at)
        {
            if (self.State.State == Seeker.StRegenerate)
                return false;
            Player player = grapple.Owner;
            if (player == null)
                return false;
            grapple.Revoke();
            self.State.ForceState(Seeker.StStunned);
            var result = self.HandleMomentumOfActor(player, self.Speed, player.Speed, grapple.ShootDirection);
            self.Speed = result.OwnerSpeed;
            player.Speed = result.OtherSpeed;
            Celeste.Freeze(0.05f);
            Audio.Play("event:/char/madeline/jump_superslide", player.Center);
            return true;
        }

        private static void DieWithExplosion(this Seeker self, SeekerExplodeBarrier barrier)
        {
            barrier.Collidable = false;
            Audio.Play("event:/char/badeline/boss_hug", self.Position);
            Audio.Play("event:/new_content/game/10_farewell/puffer_splode", self.Position);
            barrier.Flashing = true;
            barrier.Flash = 1.0f;
            self.Collider = self.pushRadius;
            self.State.Locked = true;
            Player player = self.CollideFirst<Player>();
            if (player != null && !self.Scene.CollideCheck<Solid>(self.Position, player.Center))
            {
                player.ExplodeLaunch(self.Position, true, true);
            }
            TheoCrystal theoCrystal = self.CollideFirst<TheoCrystal>();
            if (theoCrystal != null && !self.Scene.CollideCheck<Solid>(self.Position, theoCrystal.Center))
            {
                theoCrystal.ExplodeLaunch(self.Position);
            }
            foreach (TempleCrackedBlock block in self.Scene.Tracker.GetEntities<TempleCrackedBlock>())
            {
                if (self.CollideCheck(block))
                {
                    block.Break(self.Position);
                }
            }
            foreach (TouchSwitch touch in self.Scene.Tracker.GetEntities<TouchSwitch>())
            {
                if (self.CollideCheck(touch))
                {
                    touch.TurnOn();
                }
            }

            Level level = self.SceneAs<Level>();
            level.Displacement.AddBurst(self.Position, 0.4f, 12f, 36f, 0.5f);
            level.Displacement.AddBurst(self.Position, 0.4f, 24f, 48f, 0.5f);
            level.Displacement.AddBurst(self.Position, 0.4f, 36f, 60f, 0.5f);
            for (float num = 0f; num < MathF.PI * 2f; num += 0.17453292f)
            {
                Vector2 position = self.Center + Calc.AngleToVector(num + Calc.Random.Range(-MathF.PI / 90f, MathF.PI / 90f), Calc.Random.Range(12, 18));
                level.Particles.Emit(Seeker.P_Regen, position, num);
            }

            self.shaker.On = false;
            self.dead = true;
            self.RemoveSelf();
            barrier.Collidable = true;
        }
    }
}
