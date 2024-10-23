using System;

namespace Celeste.Mod.Aqua.Module;

public class AquaModule : EverestModule
{
    public static AquaModule Instance { get; private set; }

    public override Type SettingsType => typeof(AquaModuleSettings);
    public static AquaModuleSettings Settings => (AquaModuleSettings)Instance._Settings;

    public override Type SessionType => typeof(AquaModuleSession);
    public static AquaModuleSession Session => (AquaModuleSession)Instance._Session;

    public override Type SaveDataType => typeof(AquaModuleSaveData);
    public static AquaModuleSaveData SaveData => (AquaModuleSaveData)Instance._SaveData;

    public AquaModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(AquaModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(AquaModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        // TODO: apply any hooks that should always be active
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
    }
}