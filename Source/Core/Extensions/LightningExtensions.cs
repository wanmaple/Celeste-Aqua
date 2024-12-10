using Microsoft.Xna.Framework;

namespace Celeste.Mod.Aqua.Core
{
    public static class LightningExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Lightning.ctor_Vector2_int_int_Nullable1_float += Lightning_Construct;
            On.Celeste.Lightning.Update += Lightning_Update;
        }

        public static void Uninitialize()
        {
            On.Celeste.Lightning.ctor_Vector2_int_int_Nullable1_float -= Lightning_Construct;
            On.Celeste.Lightning.Update -= Lightning_Update;
        }

        private static void Lightning_Construct(On.Celeste.Lightning.orig_ctor_Vector2_int_int_Nullable1_float orig, Lightning self, Vector2 position, int width, int height, Vector2? node, float moveTime)
        {
            orig(self, position, width, height, node, moveTime);
            self.Add(new HookInOut(self.OnHookEnter, self.OnHookOut));
        }

        private static void OnHookEnter(this Lightning self, GrapplingHook hook)
        {
            hook.ElectricShocking = true;
            if (hook.Material == GrapplingHook.RopeMaterial.Metal)
            {
                Player player = self.Scene.Tracker.GetEntity<Player>();
                player.StateMachine.ForceState((int)AquaStates.StElectricShocking);
            }
        }

        private static void OnHookOut(this Lightning self, GrapplingHook hook)
        {
            if (hook != null)
            {
                hook.ElectricShocking = false;
            }
        }

        private static void Lightning_Update(On.Celeste.Lightning.orig_Update orig, Lightning self)
        {
            orig(self);
            if (self.IntersectsWithRope())
            {
                GrapplingHook hook = self.Scene.Tracker.GetEntity<GrapplingHook>();
                if (hook.Material == GrapplingHook.RopeMaterial.Metal)
                {
                    Player player = self.Scene.Tracker.GetEntity<Player>();
                    if (player.StateMachine.State != (int)AquaStates.StElectricShocking)
                    {
                        player.StateMachine.ForceState((int)AquaStates.StElectricShocking);
                    }
                }
            }
        }
    }
}
