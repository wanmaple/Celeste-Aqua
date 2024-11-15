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
    }
}
