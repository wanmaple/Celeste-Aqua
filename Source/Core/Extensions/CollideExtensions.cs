using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public static class CollideExtensions
    {
        public static void Initialize()
        {
            On.Monocle.Collide.Check_Entity_Entity += Collide_CheckTwoEntities;
        }

        public static void Uninitialize()
        {
            On.Monocle.Collide.Check_Entity_Entity -= Collide_CheckTwoEntities;
        }

        private static bool Collide_CheckTwoEntities(On.Monocle.Collide.orig_Check_Entity_Entity orig, Entity a, Entity b)
        {
            if (a.Collider == null || b.Collider == null)
            {
                return false;
            }

            if (a != b && b.Collidable && a.CanCollide(b) && b.CanCollide(a))
            {
                return a.Collider.Collide(b);
            }

            return false;
        }
    }
}
