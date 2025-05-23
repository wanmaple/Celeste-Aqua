﻿using Celeste.Mod.Aqua.Core;
using Celeste.Mod.Aqua.Rendering;

namespace Celeste.Mod.Aqua.Module
{
    public class HookCenter
    {
        public void Hook()
        {
            InputExtensions.Initialize();
            EntityListExtensions.Initialize();
            CollideExtensions.Initialize();
            LevelExtrasLoader.Initialize();
            LevelLoaderExtensions.Initialize();
            ParticleTypesExtensions.Initialize();
            EntityExtensions.Initialize();
            PlatformExtensions.Initialize();
            SolidExtensions.Initialize();
            ActorExtensions.Initialize();
            JumpThruExtensions.Initialize();
            CrumblePlatformExtensions.Initialize();
            FallingBlockExtensions.Initialize();
            MoveBlockExtensions.Initialize();
            CrushBlockExtensions.Initialize();
            BounceBlockExtensions.Initialize();
            SpringExtensions.Initialize();
            BumperExtensions.Initialize();
            RefillExtensions.Initialize();
            BoosterExtensions.Initialize();
            PufferExtensions.Initialize();
            FireBallExtensions.Initialize();
            FireBarrierExtensions.Initialize();
            IceBlockExtensions.Initialize();
            SeekerBarrierExtensions.Initialize();
            CassetteBlockExtensions.Initialize();
            FloatySpaceBlockExtensions.Initialize();
            DreamBlockExtensions.Initialize();
            TheoCrystalExtensions.Initialize();
            GliderExtensions.Initialize();
            SeekerExtensions.Initialize();
            LightningExtensions.Initialize();
            WaterExtensions.Initialize();
            WallBoosterExtensions.Initialize();
            HeartGemExtensions.Initialize();
            InvisibleBarrierExtensions.Initialize();
            PlayerSpriteExtensions.Initialize();
            PlayerHairExtensions.Initialize();
            PlayerStates.Initialize();
            GrabbyIconExtends.Initialize();
            LevelStates.Initialize();
            TalkComponentExtensions.Initialize();
            HangingLampExtensions.Initialize();
            BirdTutorialGUIHook.Initialize();
            PresentationHook.Initialize();
            RenderHook.Initialize();
        }

        public void Unhook()
        {
            InputExtensions.Uninitialize();
            EntityListExtensions.Uninitialize();
            CollideExtensions.Uninitialize();
            LevelExtrasLoader.Uninitialize();
            LevelLoaderExtensions.Uninitialize();
            ParticleTypesExtensions.Uninitialize();
            EntityExtensions.Uninitialize();
            PlatformExtensions.Uninitialize();
            SolidExtensions.Uninitialize();
            ActorExtensions.Uninitialize();
            JumpThruExtensions.Uninitialize();
            CrumblePlatformExtensions.Uninitialize();
            FallingBlockExtensions.Uninitialize();
            MoveBlockExtensions.Uninitialize();
            CrushBlockExtensions.Uninitialize();
            BounceBlockExtensions.Uninitialize();
            SpringExtensions.Uninitialize();
            BumperExtensions.Uninitialize();
            RefillExtensions.Uninitialize();
            BoosterExtensions.Uninitialize();
            PufferExtensions.Uninitialize();
            FireBallExtensions.Uninitialize();
            FireBarrierExtensions.Uninitialize();
            IceBlockExtensions.Uninitialize();
            SeekerBarrierExtensions.Uninitialize();
            CassetteBlockExtensions.Uninitialize();
            FloatySpaceBlockExtensions.Uninitialize();
            DreamBlockExtensions.Uninitialize();
            TheoCrystalExtensions.Uninitialize();
            GliderExtensions.Uninitialize();
            SeekerExtensions.Uninitialize();
            LightningExtensions.Uninitialize();
            WaterExtensions.Uninitialize();
            WallBoosterExtensions.Uninitialize();
            HeartGemExtensions.Uninitialize();
            InvisibleBarrierExtensions.Uninitialize();
            PlayerSpriteExtensions.Uninitialize();
            PlayerHairExtensions.Uninitialize();
            PlayerStates.Uninitialize();
            GrabbyIconExtends.Uninitialize();
            LevelStates.Uninitialize();
            TalkComponentExtensions.Uninitialize();
            HangingLampExtensions.Uninitialize();
            BirdTutorialGUIHook.Uninitialize();
            PresentationHook.Uninitialize();
            RenderHook.Uninitialize();
        }
    }
}
