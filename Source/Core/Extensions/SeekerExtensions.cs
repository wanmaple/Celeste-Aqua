using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class SeekerExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Seeker.ctor_Vector2_Vector2Array += Seeker_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Seeker.ctor_Vector2_Vector2Array -= Seeker_Construct;
        }

        private static void Seeker_Construct(On.Celeste.Seeker.orig_ctor_Vector2_Vector2Array orig, Seeker self, Vector2 position, Vector2[] patrolPoints)
        {
            orig(self, position, patrolPoints);
            self.Add(new HookInteractable(self.OnHookInteract));
        }

        private static bool OnHookInteract(this Seeker self, GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            return true;
        }
    }
}
