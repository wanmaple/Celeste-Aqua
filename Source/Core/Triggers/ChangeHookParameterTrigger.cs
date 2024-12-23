﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [CustomEntity("Aqua/Change Hook Parameter Trigger")]
    public class ChangeHookParameterTrigger : Trigger
    {
        public string Parameter { get; set; }
        public string Value { get; set; }

        public ChangeHookParameterTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            Parameter = data.Attr("parameter");
            Value = data.Attr("value");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            LevelStates.LevelState state = player.level.GetState();
            switch (Parameter)
            {
                case "FeatureEnabled":
                    if (bool.TryParse(Value, out bool val))
                    {
                        state.FeatureEnabled = val;
                    }
                    break;
                case "RopeMaterial":
                    if (int.TryParse(Value, out int material))
                    {
                        state.RopeMaterial = (GrapplingHook.RopeMaterial)material;
                        PlayerStates.MadelinesHook.Material = state.RopeMaterial;
                    }
                    break;
                case "RopeLength":
                    if (int.TryParse(Value, out int len))
                    {
                        state.HookSettings.RopeLength = Calc.Clamp(len, 80, 120);
                    }
                    break;
                case "EmitSpeed":
                    if (int.TryParse(Value, out int emitSpeed))
                    {
                        state.HookSettings.EmitSpeed = Calc.Clamp(emitSpeed, 600, 1200);
                    }
                    break;
                case "MaxLineSpeed":
                    if (int.TryParse(Value, out int maxSpeed))
                    {
                        state.HookSettings.MaxLineSpeed = Calc.Clamp(maxSpeed, 400, 2000);
                    }
                    break;
                case "FlyTowardSpeed":
                    if (int.TryParse(Value, out int flySpeed))
                    {
                        state.HookSettings.FlyTowardSpeed = Calc.Clamp(flySpeed, 300, 1000);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
