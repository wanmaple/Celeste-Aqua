using Celeste.Mod.Aqua.Debug;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class HangingLampExtensions
    {
        public static void Initialize()
        {
            IL.Celeste.HangingLamp.Update += HangingLamp_ILUpdate;
            On.Celeste.HangingLamp.ctor_Vector2_int += HangingLamp_Construct;
        }

        public static void Uninitialize()
        {
            IL.Celeste.HangingLamp.Update -= HangingLamp_ILUpdate;
            On.Celeste.HangingLamp.ctor_Vector2_int -= HangingLamp_Construct;
        }

        private static void HangingLamp_Construct(On.Celeste.HangingLamp.orig_ctor_Vector2_int orig, HangingLamp self, Vector2 position, int length)
        {
            orig(self, position, length);
            self.SetHookable(true);
        }

        private static void HangingLamp_ILUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            FieldInfo fieldRotation = typeof(HangingLamp).GetField("rotation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (cursor.TryGotoNext(MoveType.Before, ins => ins.MatchLdarg0(), ins => ins.MatchLdfld(fieldRotation)))
            {
                cursor.Index++;
                cursor.EmitDelegate(HandleGrappleTouch);
                cursor.EmitLdarg0();
            }
        }

        private static void HandleGrappleTouch(this HangingLamp self)
        {
            GrapplingHook grapple = self.Scene.Tracker.GetEntity<GrapplingHook>();
            if (grapple != null && self.Collider.Collide(grapple) && grapple.State != GrapplingHook.HookStates.Fixed && grapple.State != GrapplingHook.HookStates.Revoking)
            {
                float speedX = MathF.Sign(grapple.Velocity.X) * MathF.Min(MathF.Abs(grapple.Velocity.X), Player.DashSpeed * 1.25f);
                self.speed = (0.0f - speedX) * 0.005f * ((grapple.Y - self.Y) / self.Length);
                if (Math.Abs(self.speed) < 0.1f)
                {
                    self.speed = 0.0f;
                }
                else if (self.soundDelay <= 0.0f)
                {
                    self.sfx.Play("event:/game/02_old_site/lantern_hit");
                    self.soundDelay = 0.25f;
                }
            }
        }
    }
}
