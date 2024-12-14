using Celeste.Mod.Aqua.Rendering;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    public HookCenter HookCenter { get; private set; } = new HookCenter();

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
        HookCenter.Hook();
        Everest.Events.AssetReload.OnBeforeReload += AssetReload_OnBeforeReload;
        Everest.Events.AssetReload.OnAfterReload += AssetReload_OnAfterReload;
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
        HookCenter.Unhook();
        Everest.Events.AssetReload.OnBeforeReload -= AssetReload_OnBeforeReload;
        Everest.Events.AssetReload.OnAfterReload -= AssetReload_OnAfterReload;
    }

    public override void LoadContent(bool firstLoad)
    {
        ReloadAllShaders();
    }

    private void AssetReload_OnBeforeReload(bool silent)
    {
        ClearAllShaders();
    }

    private void AssetReload_OnAfterReload(bool silent)
    {
        ReloadAllShaders();
        if (Engine.Instance.scene is Level level)
        {
            List<Entity> drawables = level.Tracker.GetEntities<CustomShaderEntity>();
            foreach (CustomShaderEntity drawable in drawables)
            {
                drawable.OnReload();
            }
        }
    }

    private void ClearAllShaders()
    {
        FXCenter.Instance.ClearAll();
    }

    private void ReloadAllShaders()
    {
        ModContent mod = Everest.Content.Mods.First(mod => mod.Name == ModConstants.MOD_NAME);
        foreach (ModAsset asset in mod.List)
        {
            if (asset.PathVirtual.StartsWith("Shaders/") && Path.GetExtension(asset.PathVirtual) == ".cso")
            {
                string shaderName = Path.GetFileNameWithoutExtension(asset.PathVirtual);
                FXCenter.Instance.PrepareLoad(shaderName, asset);
            }
        }
    }
}