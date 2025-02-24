using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class FloatySpaceBlockExtensions
    {
        public static void Initialize()
        {
            IL.Celeste.FloatySpaceBlock.Update += FloatySpaceBlock_ILUpdate;
        }

        public static void Uninitialize()
        {
            IL.Celeste.FloatySpaceBlock.Update -= FloatySpaceBlock_ILUpdate;
        }

        private static void FloatySpaceBlock_ILUpdate(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            MethodInfo method = typeof(Solid).GetMethod("HasPlayerRider", BindingFlags.Instance | BindingFlags.Public);
            if (cursor.TryGotoNext(ins => ins.MatchCallvirt(method)))
            {
                cursor.Index++;
                cursor.EmitPop();
                cursor.EmitLdarg0();
                cursor.EmitDelegate(IsPlayerHanging);
                //cursor.EmitOr();
            }
        }

        private static bool IsPlayerHanging(FloatySpaceBlock self)
        {
            // Since I hook the HasPlayerRider method, the logic should be changed though.
            if (self.GetPlayerRider() != null)
            {
                return true;
            }
            var players = self.Scene.Tracker.GetEntities<Player>();
            if (players != null)
            {
                foreach (Player player in players)
                {
                    GrapplingHook hook = player.GetGrappleHook();
                    if (!player.onGround && hook.State == GrapplingHook.HookStates.Fixed && self.IsHookAttached())
                    {
                        Vector2 ropeDirection = hook.RopeDirection;
                        bool swingUp = AquaMaths.Cross(Vector2.UnitX, ropeDirection) >= 0.0f;
                        if (swingUp && hook.AlongRopeSpeed > 0.0f)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
