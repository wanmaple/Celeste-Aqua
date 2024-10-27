namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class Debug
    {
        public void LogInfo(string message, params object[] args)
        {
#if DEBUG
            Logger.Info(ModConstants.MOD_NAME, string.Format(message, args));
#endif
        }

        public void LogWarning(string message, params object[] args)
        {
            Logger.Warn(ModConstants.MOD_NAME, string.Format(message, args));
        }

        public void LogError(string message, params object[] args)
        {
            Logger.Error(ModConstants.MOD_NAME, string.Format(message, args));
        }
    }
}
