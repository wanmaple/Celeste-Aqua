using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class PufferExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Puffer.ctor_Vector2_bool += Puffer_Construct;
        }

        public static void Uninitialize()
        {
            On.Celeste.Puffer.ctor_Vector2_bool -= Puffer_Construct;
        }

        private static void Puffer_Construct(On.Celeste.Puffer.orig_ctor_Vector2_bool orig, Puffer self, Vector2 position, bool faceRight)
        {
            orig(self, position, faceRight);
            self.Add(new HookInteractable(self.OnHookInteract));
        }

        private static bool OnHookInteract(this Puffer self, GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            Audio.Play("event:/new_content/game/10_farewell/puffer_boop", self.Position);
            self.Add(new Coroutine(self.UndraggableRoutine(self.sprite, Vector2.Normalize(at - self.Center), 0.4f, 8.0f)));
            return true;
        }
    }
}
