using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.Aqua.Core
{
    public static class DreamBlockExtensions
    {
        public static void Initialize()
        {
            On.Celeste.DreamBlock.ctor_Vector2_float_float_Nullable1_bool_bool_bool += DreamBlock_Construct;
            On.Celeste.DreamBlock.Added += DreamBlock_Added;
            On.Celeste.DreamBlock.Update += DreamBlock_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.DreamBlock.ctor_Vector2_float_float_Nullable1_bool_bool_bool -= DreamBlock_Construct;
            On.Celeste.DreamBlock.Added -= DreamBlock_Added;
            On.Celeste.DreamBlock.Update -= DreamBlock_Update;
        }

        private static void DreamBlock_Construct(On.Celeste.DreamBlock.orig_ctor_Vector2_float_float_Nullable1_bool_bool_bool orig, DreamBlock self, Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below)
        {
            orig(self, position, width, height, node, fastMoving, oneUse, below);
            self.Add(new HookInOut(self.OnHookIn, self.OnHookOut, self.OnHookKeepIn));
            DynamicData.For(self).Set("hook_in_sound", new SoundSource());
        }

        private static void DreamBlock_Added(On.Celeste.DreamBlock.orig_Added orig, DreamBlock self, Scene scene)
        {
            orig(self, scene);
            self.SetHookable(!self.playerHasDreamDash);
        }

        private static void DreamBlock_Update(On.Celeste.DreamBlock.orig_Update orig, DreamBlock self)
        {
            orig(self);
            self.SetHookable(!self.playerHasDreamDash);
        }

        private static void OnHookIn(this DreamBlock self, GrapplingHook hook)
        {
            hook.EmitSpeedMultiplier *= DREAM_BLOCK_HOOK_ACCELERATION;
            if (self.playerHasDreamDash)
            {
                Audio.Play("event:/char/madeline/dreamblock_enter");
                SoundSource hookInSound = DynamicData.For(self).Get<SoundSource>("hook_in_sound");
                hookInSound.Play("event:/char/madeline/dreamblock_travel");
            }
        }

        private static void OnHookOut(this DreamBlock self, GrapplingHook hook)
        {
            if (hook != null)
            {
                hook.EmitSpeedMultiplier /= DREAM_BLOCK_HOOK_ACCELERATION;
            }
            SoundSource hookInSound = DynamicData.For(self).Get<SoundSource>("hook_in_sound");
            hookInSound.Stop();
        }

        private static void OnHookKeepIn(this DreamBlock self, GrapplingHook hook)
        {
            SoundSource hookInSound = DynamicData.For(self).Get<SoundSource>("hook_in_sound");
            if (hook.State == GrapplingHook.HookStates.Fixed)
            {
                hookInSound.Stop();
            }
            else if (!hookInSound.Playing)
            {
                hookInSound.Play("event:/char/madeline/dreamblock_travel");
            }
            if (hook.State != GrapplingHook.HookStates.Fixed && self.playerHasDreamDash && self.Scene.OnInterval(0.04f))
            {
                DisplacementRenderer.Burst burst = (self.Scene as Level).Displacement.AddBurst(hook.Center, 0.3f, 0f, 20f);
                burst.WorldClipCollider = self.Collider;
                burst.WorldClipPadding = 2;
            }
        }

        private const float DREAM_BLOCK_HOOK_ACCELERATION = 1.2f;
    }
}
