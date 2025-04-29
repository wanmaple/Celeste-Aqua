using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class BoosterExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Booster.ctor_Vector2_bool += Booster_Construct;
            On.Celeste.Booster.PlayerReleased += Booster_PlayerReleased;
            On.Celeste.Booster.Respawn += Booster_Respawn;
            On.Celeste.Booster.AppearParticles += Booster_AppearParticles;
            On.Celeste.Booster.Update += Booster_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Booster.ctor_Vector2_bool -= Booster_Construct;
            On.Celeste.Booster.PlayerReleased -= Booster_PlayerReleased;
            On.Celeste.Booster.Respawn -= Booster_Respawn;
            On.Celeste.Booster.AppearParticles -= Booster_AppearParticles;
            On.Celeste.Booster.Update -= Booster_Update;
        }

        private static void Booster_Construct(On.Celeste.Booster.orig_ctor_Vector2_bool orig, Booster self, Vector2 position, bool red)
        {
            orig(self, position, red);
            self.Add(new HookInteractable(self.OnGrappleInteract));
            self.SetHookable(true);
            DynamicData.For(self).Set("undraggable_routine", null);
        }

        private static void Booster_PlayerReleased(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
        {
            orig(self);
            self.SetHookable(false);
        }

        private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
        {
            self.ResetPosition();
            self.SetHookable(true);
            orig(self);
        }

        private static void Booster_AppearParticles(On.Celeste.Booster.orig_AppearParticles orig, Booster self)
        {
            if (self is AquaBooster booster)
            {
                ParticleSystem particlesBG = self.SceneAs<Level>().ParticlesBG;
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(booster.AppearParticle, 1, self.Center, Vector2.One * 2f, i * (MathF.PI / 180f));
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void Booster_Update(On.Celeste.Booster.orig_Update orig, Booster self)
        {
            orig(self);
            if (!self.IsBoostingPlayer() && self.cannotUseTimer <= 0.0f)
            {
                var transporters = self.Scene.Tracker.GetEntities<BoosterTransporter>();
                float minDistance = float.MaxValue;
                BoosterTransporter bestTransporter = null;
                foreach (BoosterTransporter transporter in transporters)
                {
                    if (transporter.IsBusy)
                        continue;
                    if (self.CollideCheck(transporter))
                    {
                        float distanceSq = Vector2.DistanceSquared(self.Position, transporter.Position);
                        if (distanceSq < minDistance)
                        {
                            minDistance = distanceSq;
                            bestTransporter = transporter;
                        }
                    }
                }
                if (bestTransporter != null)
                {
                    bestTransporter.BeginTransport(self);
                }
            }
        }

        private static void ResetPosition(this Booster self)
        {
            self.Position = self.outline.Position;
        }

        private static bool OnGrappleInteract(this Booster self, GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            Audio.Play(self.red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", self.Position);
            Coroutine routine = DynamicData.For(self).Get<Coroutine>("undraggable_routine");
            if (routine == null)
            {
                self.Add(routine = new Coroutine(self.UndraggableRoutine(self.sprite, Calc.SafeNormalize(at - self.Center), 0.4f, 8.0f)));
            }
            else
            {
                routine.Replace(self.UndraggableRoutine(self.sprite, Calc.SafeNormalize(at - self.Center), 0.4f, 8.0f));
            }
            return true;
        }

        public static bool IsBoostingPlayer(this Booster self)
        {
            var players = self.Scene.Tracker.GetEntities<Player>();
            foreach (Player player in players)
            {
                if (player.CurrentBooster == self)
                    return true;
            }
            return self.BoostingPlayer;
        }
    }
}
