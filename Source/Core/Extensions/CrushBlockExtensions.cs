using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class CrushBlockExtensions
    {
        public static void Initialize()
        {
            using (new DetourConfigContext(new DetourConfig(ModConstants.MOD_NAME, priority: 1)).Use())
            {
                _ilAttackSequence = new ILHook(_methodAttackSequence, CrushBlock_ILAttackSequence);
            }
            On.Celeste.CrushBlock.ctor_Vector2_float_float_Axes_bool += CrushBlock_Construct;
            On.Celeste.CrushBlock.Update += CrushBlock_Update;
            On.Celeste.CrushBlock.OnDashed += CrushBlock_OnDashed;
            On.Celeste.CrushBlock.TurnOffImages += CrushBlock_TurnOffImages;
        }

        public static void Uninitialize()
        {
            _ilAttackSequence?.Dispose();
            _ilAttackSequence = null;
            On.Celeste.CrushBlock.ctor_Vector2_float_float_Axes_bool -= CrushBlock_Construct;
            On.Celeste.CrushBlock.Update -= CrushBlock_Update;
            On.Celeste.CrushBlock.OnDashed -= CrushBlock_OnDashed;
            On.Celeste.CrushBlock.TurnOffImages -= CrushBlock_TurnOffImages;
        }

        private static void CrushBlock_ILAttackSequence(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, ins => ins.MatchLdcR4(CrushBlock.CrushSpeed)))
            {
                cursor.EmitLdloc1();
                cursor.EmitDelegate(CountExtraSpeed);
            }
        }

        private static void CrushBlock_Construct(On.Celeste.CrushBlock.orig_ctor_Vector2_float_float_Axes_bool orig, CrushBlock self, Vector2 position, float width, float height, CrushBlock.Axes axes, bool chillOut)
        {
            orig(self, position, width, height, axes, chillOut);
            self.Add(new ExtraSpeedComponent());
            self.Add(new AccelerationAreaInOut(self.OnKeepInAccelerationArea, null, self.OnExitAccelerationArea));
            self.Add(new PureColorTrails(e => (e as CrushBlock).GetAccelerateState() == AccelerationArea.AccelerateState.Accelerate && self.Scene.OnInterval(0.05f), e => AccelerationColor, Vector2.Zero));
            self.SetAccelerateState(AccelerationArea.AccelerateState.None);
        }

        private static void CrushBlock_Update(On.Celeste.CrushBlock.orig_Update orig, CrushBlock self)
        {
            orig(self);
            if (self.chillOut || self.crushDir == Vector2.Zero)
            {
                var com = self.Get<ExtraSpeedComponent>();
                com.ExtraSpeed = 0.0f;
            }
        }

        private static DashCollisionResults CrushBlock_OnDashed(On.Celeste.CrushBlock.orig_OnDashed orig, CrushBlock self, Player player, Vector2 direction)
        {
            var result = orig(self, player, direction);
            if (result == DashCollisionResults.Rebound)
            {
                float sign = self.crushDir.X == 0.0f ? self.crushDir.Y : self.crushDir.X;
                var com = self.Get<ExtraSpeedComponent>();
                com.ExtraSpeed = 0.0f;
            }
            return result;
        }

        private static void CrushBlock_TurnOffImages(On.Celeste.CrushBlock.orig_TurnOffImages orig, CrushBlock self)
        {
            orig(self);
            if (self is GrappleAttackCrushBlock block && block.NoReturn)
            {
                block.returnStack.Clear();
            }
        }

        public static void AddExtraSpeed(this CrushBlock self, float toAdd)
        {
            self.Get<ExtraSpeedComponent>().ExtraSpeed += toAdd;
        }

        private static float CountExtraSpeed(float speed, CrushBlock self)
        {
            ExtraSpeedComponent com = self.Get<ExtraSpeedComponent>();
            float finalSpeed = speed + com.ExtraSpeed;
            return finalSpeed;
        }

        private static void OnExitAccelerationArea(this CrushBlock self, AccelerationArea area)
        {
            self.SetAccelerateState(AccelerationArea.AccelerateState.None);
        }

        private static void OnKeepInAccelerationArea(this CrushBlock self, AccelerationArea area)
        {
            if (self.chillOut || self.crushDir == Vector2.Zero)
            {
                self.Get<ExtraSpeedComponent>().ExtraSpeed = 0.0f;
                self.SetAccelerateState(AccelerationArea.AccelerateState.None);
            }
            else
            {
                self.SetAccelerateState(area.TryAccelerate(self));
            }
        }

        private static MethodInfo _methodAttackSequence = typeof(CrushBlock).GetMethod("AttackSequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget();
        private static ILHook _ilAttackSequence;
        private static Color AccelerationColor = Calc.HexToColor("007eff");
    }
}
