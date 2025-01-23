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
            if (orig(a, b))
            {
                if (a.CanCollide(b) && b.CanCollide(a))
                    return true;
                return false;
            }
            return false;
        }
    }
}
