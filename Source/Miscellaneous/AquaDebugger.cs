using Monocle;
using System.Diagnostics;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class AquaDebugger
    {
        public static void LogInfo(string message, params object[] args)
        {
#if DEBUG
            Logger.Info(ModConstants.MOD_NAME, FrameCounterString() + string.Format(message, args));
#endif
        }

        public static void LogWarning(string message, params object[] args)
        {
            Logger.Warn(ModConstants.MOD_NAME, FrameCounterString() + string.Format(message, args));
        }

        public static void LogError(string message, params object[] args)
        {
            Logger.Error(ModConstants.MOD_NAME, FrameCounterString() + string.Format(message, args));
        }

        public static void Assert(bool condition, string message, params object[] args)
        {
            Debug.Assert(condition, FrameCounterString() + string.Format(message, args));
        }

        private static string FrameCounterString()
        {
            return $"[{Engine.FrameCounter}] ";
        }
    }
}
