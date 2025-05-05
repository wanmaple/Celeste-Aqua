using Celeste.Mod.Aqua.Module;

namespace Celeste.Mod.Aqua.Core
{
    public static class HookBeginnerTutorial
    {
        public static bool IsHookFixed(Level level)
        {
            GrapplingHook hook = level.Tracker.GetEntity<GrapplingHook>();
            if (hook != null && hook.State == GrapplingHook.HookStates.Fixed)
            {
                return true;
            }

            return false;
        }

        public static bool IsSwingingInRedBubble(Level level)
        {
            Player player = level.Tracker.GetEntity<Player>();
            GrapplingHook grapple = player.GetGrappleHook();
            if (player != null && grapple != null)
            {
                return player.StateMachine.State == (int)AquaStates.StRedDash && grapple.State == GrapplingHook.HookStates.Fixed && (Input.GrabCheck || AquaModule.Settings.AutoGrabRopeIfPossible);
            }
            return false;
        }

        public static bool IsLevelFrozen(Level level)
        {
            return level.Frozen;
        }
    }
}
