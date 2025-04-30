using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class JumpThruExtensions
    {
        public static void Initialize()
        {
            On.Celeste.JumpThru.ctor += JumpThru_Construct;
            On.Celeste.JumpThru.HasPlayerRider += JumpThru_HasPlayerRider;
        }

        public static void Uninitialize()
        {
            On.Celeste.JumpThru.ctor -= JumpThru_Construct;
            On.Celeste.JumpThru.HasPlayerRider -= JumpThru_HasPlayerRider;
        }

        private static void JumpThru_Construct(On.Celeste.JumpThru.orig_ctor orig, JumpThru self, Vector2 position, int width, bool safe)
        {
            orig(self, position, width, safe);
            if (ModInterop.AttachedJumpThroughType != null && self.GetType() == ModInterop.AttachedJumpThroughType)
            {
                self.Add(new GrapplingHookAttachBehavior(ExtraUpdate));
            }
        }

        private static bool JumpThru_HasPlayerRider(On.Celeste.JumpThru.orig_HasPlayerRider orig, JumpThru self)
        {
            if (!orig(self))
            {
                if (!self.IsHookAttached())
                    return false;
            }
            return true;
        }

        private static void ExtraUpdate(Entity self, GrapplingHook hook)
        {
            JumpThru jumpthru = self as JumpThru;
            if (jumpthru.HasPlayerRider())
            {
                MethodInfo methodTrigger = self.GetType().GetMethod("TriggerPlatform", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodTrigger != null)
                {
                    methodTrigger.Invoke(jumpthru, null);
                }
            }
        }
    }
}
