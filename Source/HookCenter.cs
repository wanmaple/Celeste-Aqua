using Celeste.Mod.Aqua.Core;
using Celeste.Mod.Aqua.Core.Extensions;

namespace Celeste.Mod.Aqua
{
    public class HookCenter
    {
        public void Hook()
        {
            EntityExtensions.Initialize();
            PlatformExtensions.Initialize();
            SolidExtensions.Initialize();
            FallingBlockExtensions.Initialize();
            ZipMoverExtensions.Initialize();
            CrumblePlatformExtensions.Initialize();
            MoveBlockExtensions.Initialize();
            BounceBlockExtensions.Initialize();
            SpringExtensions.Initialize();
            BumperExtensions.Initialize();
            CoreModeToggleExtensions.Initialize();
            PlayerSpriteExtensions.Initialize();
            PlayerStates.Initialize();
        }

        public void Unhook()
        {
            EntityExtensions.Uninitialize();
            PlatformExtensions.Uninitialize();
            SolidExtensions.Uninitialize();
            FallingBlockExtensions.Uninitialize();
            ZipMoverExtensions.Uninitialize();
            CrumblePlatformExtensions.Uninitialize();
            MoveBlockExtensions.Uninitialize();
            BounceBlockExtensions.Uninitialize();
            SpringExtensions.Uninitialize();
            BumperExtensions.Uninitialize();
            CoreModeToggleExtensions.Uninitialize();
            PlayerSpriteExtensions.Uninitialize();
            PlayerStates.Uninitialize();
        }
    }
}
