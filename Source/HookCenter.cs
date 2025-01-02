﻿using Celeste.Mod.Aqua.Core;
using Celeste.Mod.Aqua.Rendering;

namespace Celeste.Mod.Aqua
{
    public class HookCenter
    {
        public void Hook()
        {
            CollideExtensions.Initialize();
            LevelExtrasLoader.Initialize();
            LevelLoaderExtensions.Initialize();
            ParticleTypesExtensions.Initialize();
            EntityExtensions.Initialize();
            PlatformExtensions.Initialize();
            SolidExtensions.Initialize();
            CrumblePlatformExtensions.Initialize();
            FallingBlockExtensions.Initialize();
            MoveBlockExtensions.Initialize();
            BounceBlockExtensions.Initialize();
            SpringExtensions.Initialize();
            BumperExtensions.Initialize();
            BoosterExtensions.Initialize();
            AngryOshiroExtensions.Initialize();
            SeekerExtensions.Initialize();
            PufferExtensions.Initialize();
            FireBallExtensions.Initialize();
            CassetteBlockExtensions.Initialize();
            FloatySpaceBlockExtensions.Initialize();
            DreamBlockExtensions.Initialize();
            LightningExtensions.Initialize();
            WaterExtensions.Initialize();
            HeartGemExtensions.Initialize();
            InvisibleBarrierExtensions.Initialize();
            PlayerSpriteExtensions.Initialize();
            PlayerStates.Initialize();
            GrabbyIconExtends.Initialize();
            LevelStates.Initialize();
            TalkComponentExtensions.Initialize();
            BirdTutorialGUIHook.Initialize();
            RenderHook.Initialize();
        }

        public void Unhook()
        {
            CollideExtensions.Uninitialize();
            LevelExtrasLoader.Uninitialize();
            LevelLoaderExtensions.Uninitialize();
            ParticleTypesExtensions.Uninitialize();
            EntityExtensions.Uninitialize();
            PlatformExtensions.Uninitialize();
            SolidExtensions.Uninitialize();
            CrumblePlatformExtensions.Uninitialize();
            FallingBlockExtensions.Uninitialize();
            MoveBlockExtensions.Uninitialize();
            BounceBlockExtensions.Uninitialize();
            SpringExtensions.Uninitialize();
            BumperExtensions.Uninitialize();
            BoosterExtensions.Uninitialize();
            AngryOshiroExtensions.Uninitialize();
            SeekerExtensions.Uninitialize();
            PufferExtensions.Uninitialize();
            FireBallExtensions.Uninitialize();
            CassetteBlockExtensions.Uninitialize();
            FloatySpaceBlockExtensions.Uninitialize();
            DreamBlockExtensions.Uninitialize();
            LightningExtensions.Uninitialize();
            WaterExtensions.Uninitialize();
            HeartGemExtensions.Uninitialize();
            InvisibleBarrierExtensions.Uninitialize();
            PlayerSpriteExtensions.Uninitialize();
            PlayerStates.Uninitialize();
            GrabbyIconExtends.Uninitialize();
            LevelStates.Uninitialize();
            TalkComponentExtensions.Uninitialize();
            BirdTutorialGUIHook.Uninitialize();
            RenderHook.Uninitialize();
        }
    }
}
