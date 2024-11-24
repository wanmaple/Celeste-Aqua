using Monocle;

namespace Celeste.Mod.Aqua.Debug
{
    public static class AquaDebugger
    {
        public static void LogInfo(string message, params object[] args)
        {
#if DEBUG
            string formated = args.Length > 0 ? string.Format(message, args) : message;
            Logger.Info(ModConstants.MOD_NAME, FrameCounterString() + formated);
#endif
        }

        public static void LogWarning(string message, params object[] args)
        {
            string formated = args.Length > 0 ? string.Format(message, args) : message;
            Logger.Warn(ModConstants.MOD_NAME, FrameCounterString() + formated);
        }

        public static void LogError(string message, params object[] args)
        {
            string formated = args.Length > 0 ? string.Format(message, args) : message;
            Logger.Error(ModConstants.MOD_NAME, FrameCounterString() + formated);
        }

        public static void Assert(bool condition, string message, params object[] args)
        {
            System.Diagnostics.Debug.Assert(condition, FrameCounterString() + string.Format(message, args));
        }

        private static string FrameCounterString()
        {
            return $"[{Engine.FrameCounter}] ";
        }
    }
}
