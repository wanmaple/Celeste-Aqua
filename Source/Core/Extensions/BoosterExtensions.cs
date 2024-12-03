using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class BoosterExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Booster.ctor_Vector2_bool += Booster_Construct;
            On.Celeste.Booster.Respawn += Booster_Respawn;
        }

        public static void Uninitialize()
        {
            On.Celeste.Booster.ctor_Vector2_bool -= Booster_Construct;
            On.Celeste.Booster.Respawn -= Booster_Respawn;
        }

        private static void Booster_Construct(On.Celeste.Booster.orig_ctor_Vector2_bool orig, Booster self, Vector2 position, bool red)
        {
            orig(self, position, red);
            self.Add(new HookInteractable(self.OnHookCollide));
        }

        private static void Booster_Respawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
        {
            self.Position = self.outline.Position;
            if (self is AquaBooster booster)
            {
                booster._movingDirection = Vector2.Zero;
                booster._speed = 0.0f;
                booster._movingCoroutine.Active = true;
            }
            orig(self);
        }

        private static bool OnHookCollide(this Booster self, GrapplingHook hook, Vector2 at)
        {
            hook.Revoke();
            Audio.Play(self.red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", self.Position);
            self.Add(new Coroutine(self.UndraggableRoutine(self.sprite, Vector2.Normalize(at - self.Center), 0.4f, 8.0f)));
            return true;
        }
    }
}
