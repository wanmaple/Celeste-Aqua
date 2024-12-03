using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class AngryOshiroExtensions
    {
        public static void Initialize()
        {
            On.Celeste.AngryOshiro.ctor_Vector2_bool += AngryOshiro_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.AngryOshiro.ctor_Vector2_bool -= AngryOshiro_Construct;
        }

        private static void AngryOshiro_Construct(On.Celeste.AngryOshiro.orig_ctor_Vector2_bool orig, AngryOshiro self, Vector2 position, bool fromCutscene)
        {
            orig(self, position, fromCutscene);
            self.Add(new HookInteractable(self.OnHookInteract));
        }

        private static bool OnHookInteract(this AngryOshiro self, GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            return true;
        }
    }
}
