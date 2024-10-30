namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class AquaLogger
    {
        public static void LogInfo(string message, params object[] args)
        {
#if DEBUG
            Logger.Info(ModConstants.MOD_NAME, string.Format(message, args));
#endif
        }

        public static void LogWarning(string message, params object[] args)
        {
            Logger.Warn(ModConstants.MOD_NAME, string.Format(message, args));
        }

        public static void LogError(string message, params object[] args)
        {
            Logger.Error(ModConstants.MOD_NAME, string.Format(message, args));
        }
    }
}
