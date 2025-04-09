using Celeste.Mod.Aqua.Module;
using Microsoft.Xna.Framework;
using Monocle;
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
                    self.Add(new GrapplingHookAttachBehavior(UpdateGrapplingHookOnConveyor));
                }
            }
        }

        private static void UpdateGrapplingHookOnConveyor(Entity conveyor, GrapplingHook hook)
        {
            Solid solid = conveyor as Solid;
            if (!hook.Active || hook.AttachedEntity != conveyor)
            {
                return;
            }
            bool movingLeft = (bool)_propConveyorMovingLeft.GetValue(conveyor);
            float speed = (float)_fieldConveyorSpeed.GetValue(null);
            float hookSize = hook.HookSize;
            float dt = Engine.DeltaTime;
            if (hook.Top == conveyor.Bottom)
            {
                if (movingLeft && hook.Right <= conveyor.Right)
                {
                    Vector2 movement = speed * dt * Vector2.UnitX;
                    Vector2 targetPos = hook.ExactPosition + movement;
                    targetPos.X = MathF.Min(targetPos.X, conveyor.Right - hookSize * 0.5f);
                    hook.AddMovement(targetPos - hook.ExactPosition);
                }
                else if (!movingLeft && hook.Left >= conveyor.Left)
                {
                    Vector2 movement = -speed * dt * Vector2.UnitX;
                    Vector2 targetPos = hook.ExactPosition + movement;
                    targetPos.X = MathF.Max(targetPos.X, conveyor.Left + hookSize * 0.5f);
                    hook.AddMovement(targetPos - hook.ExactPosition);
                }
            }
            else if (hook.Bottom == conveyor.Top)
            {
                if (movingLeft && hook.Left >= conveyor.Left)
                {
                    Vector2 movement = -speed * dt * Vector2.UnitX;
                    Vector2 targetPos = hook.ExactPosition + movement;
                    targetPos.X = MathF.Max(targetPos.X, conveyor.Left + hookSize * 0.5f);
                    hook.AddMovement(targetPos - hook.ExactPosition);
                }
                else if (!movingLeft && hook.Right <= conveyor.Right)
                {
                    Vector2 movement = speed * dt * Vector2.UnitX;
                    Vector2 targetPos = hook.ExactPosition + movement;
                    targetPos.X = MathF.Min(targetPos.X, conveyor.Right - hookSize * 0.5f);
                    hook.AddMovement(targetPos - hook.ExactPosition);
                }
            }
        }

        private static PropertyInfo _propConveyorMovingLeft;
        private static FieldInfo _fieldConveyorSpeed;
    }
}
