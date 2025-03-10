using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.Aqua.Core
{
    public static class SolidExtensions
    {
        public static void Initialize()
        {
            On.Celeste.Solid.Awake += Solid_Awake;
            On.Celeste.Solid.HasPlayerRider += Solid_HasPlayerRider;
        }

        public static void Uninitialize()
        {
            On.Celeste.Solid.Awake -= Solid_Awake;
            On.Celeste.Solid.HasPlayerRider -= Solid_HasPlayerRider;
        }

        private static void Solid_Awake(On.Celeste.Solid.orig_Awake orig, Solid self, Monocle.Scene scene)
        {
            orig(self, scene);
            self.WorkWithConveyor();
        }

        private static bool Solid_HasPlayerRider(On.Celeste.Solid.orig_HasPlayerRider orig, Solid self)
        {
            if (!orig(self))
            {
                if (!self.IsHookAttached())
                    return false;
            }
            return true;
        }

        private static void WorkWithConveyor(this Solid self)
        {
            if (ModInterop.ConveyorType == null)
                return;
            if (self.GetType().IsAssignableTo(ModInterop.ConveyorType))
            {
                _propConveyorMovingLeft = self.GetType().GetProperty("IsMovingLeft", BindingFlags.Instance | BindingFlags.Public);
                _fieldConveyorSpeed = self.GetType().GetField("ConveyorMoveSpeed", BindingFlags.Static | BindingFlags.Public);
                if (_propConveyorMovingLeft != null && _fieldConveyorSpeed != null)
                {
                    self.SetHookMovement(0.0f);
                    self.SetJustAttached(false);
                    self.Add(new GrapplingHookAttachBehavior(UpdateGrapplingHookOnConveyor));
                }
            }
        }

        private static void UpdateGrapplingHookOnConveyor(Entity conveyor, GrapplingHook hook)
        {
            Solid solid = conveyor as Solid;
            if (!hook.Active || hook.State != GrapplingHook.HookStates.Fixed)
            {
                solid.SetJustAttached(false);
                return;
            }
            if (!solid.IsJustAttached())
            {
                solid.SetJustAttached(true);
                solid.SetHookMovement(0.0f);
                solid.SetStartPosition(hook.Position);
            }
            bool movingLeft = (bool)_propConveyorMovingLeft.GetValue(conveyor);
            float speed = (float)_fieldConveyorSpeed.GetValue(null);
            float hookSize = hook.HookSize;
            float dt = Engine.DeltaTime;
            Vector2 startPos = solid.GetStartPosition();
            float hookMovement = solid.GetHookMovement();
            if (hook.Top == conveyor.Bottom)
            {
                if (movingLeft && hook.Right <= conveyor.Right)
                {
                    hookMovement = MathF.Min(hookMovement + speed * dt, conveyor.Right - startPos.X - hookSize * 0.5f);
                    Vector2 targetPos = startPos + hookMovement * Vector2.UnitX;
                    targetPos.X = MathF.Min(targetPos.X, conveyor.Right - hookSize * 0.5f);
                    hook.SetPositionRounded(targetPos);
                }
                else if (!movingLeft && hook.Left >= conveyor.Left)
                {
                    hookMovement = MathF.Max(hookMovement - speed * dt, conveyor.Left - startPos.X + hookSize * 0.5f);
                    Vector2 targetPos = startPos + hookMovement * Vector2.UnitX;
                    targetPos.X = MathF.Max(targetPos.X, conveyor.Left + hookSize * 0.5f);
                    hook.SetPositionRounded(targetPos);
                }
                solid.SetHookMovement(hookMovement);
            }
            else if (hook.Bottom == conveyor.Top)
            {
                if (movingLeft && hook.Left >= conveyor.Left)
                {
                    hookMovement = MathF.Max(hookMovement - speed * dt, conveyor.Left - startPos.X + hookSize * 0.5f);
                    Vector2 targetPos = startPos + hookMovement * Vector2.UnitX;
                    targetPos.X = MathF.Max(targetPos.X, conveyor.Left + hookSize * 0.5f);
                    hook.SetPositionRounded(targetPos);
                }
                else if (!movingLeft && hook.Right <= conveyor.Right)
                {
                    hookMovement = MathF.Min(hookMovement + speed * dt, conveyor.Right - startPos.X - hookSize * 0.5f);
                    Vector2 targetPos = startPos + hookMovement * Vector2.UnitX;
                    targetPos.X = MathF.Min(targetPos.X, conveyor.Right - hookSize * 0.5f);
                    hook.SetPositionRounded(targetPos);
                }
                solid.SetHookMovement(hookMovement);
            }
        }

        public static float GetHookMovement(this Solid self)
        {
            return (float)DynamicData.For(self).Get<float>("hook_movement");
        }

        private static void SetHookMovement(this Solid self, float movement)
        {
            DynamicData.For(self).Set("hook_movement", movement);
        }

        public static Vector2 GetStartPosition(this Solid self)
        {
            return (Vector2)DynamicData.For(self).Get<Vector2>("hook_start_pos");
        }

        public static void SetStartPosition(this Solid self, Vector2 pos)
        {
            DynamicData.For(self).Set("hook_start_pos", pos);
        }

        public static bool IsJustAttached(this Solid self)
        {
            return (bool)DynamicData.For(self).Get("just_attached");
        }

        public static void SetJustAttached(this Solid self, bool value)
        {
            DynamicData.For(self).Set("just_attached", value);
        }

        private static PropertyInfo _propConveyorMovingLeft;
        private static FieldInfo _fieldConveyorSpeed;
    }
}
