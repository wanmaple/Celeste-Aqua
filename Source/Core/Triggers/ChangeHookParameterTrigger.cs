﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

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
                    if (int.TryParse(Value, out int num) && num > 0)
                    {
                        state.FeatureEnabled = true;
                    }
                    else if (bool.TryParse(Value, out bool bl) && bl)
                    {
                        state.FeatureEnabled = true;
                    }
                    else
                    {
                        state.FeatureEnabled = false;
                    }
                    break;
                case "RopeMaterial":
                    if (int.TryParse(Value, out int material))
                    {
                        state.RopeMaterial = (GrapplingHook.RopeMaterial)material;
                        var hook = player.GetGrappleHook();
                        hook.Material = state.RopeMaterial;
                    }
                    break;
                case "RopeLength":
                    if (int.TryParse(Value, out int len))
                    {
                        state.HookSettings.RopeLength = Calc.Clamp(len, 80, 200);
                        var hook = player.GetGrappleHook();
                        hook.SetRopeLength(len);
                    }
                    break;
                case "EmitSpeed":
                    if (int.TryParse(Value, out int emitSpeed))
                    {
                        state.HookSettings.EmitSpeed = Calc.Clamp(emitSpeed, 600, 2000);
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
                        state.HookSettings.FlyTowardSpeed = Calc.Clamp(flySpeed, 300, 600);
                    }
                    break;
                case "ActorPullForce":
                    if (int.TryParse(Value, out int pullForce))
                    {
                        state.HookSettings.ActorPullForce = Calc.Clamp(pullForce, 300, 600);
                    }
                    break;
                case "SwingJumpStaminaCost":
                    if (int.TryParse(Value, out int swingJumpStamina))
                    {
                        state.HookSettings.SwingJumpStaminaCost = Math.Max(swingJumpStamina, 0);
                    }
                    break;
                case "DisableGrappleBoost":
                    if (int.TryParse(Value, out int num2) && num2 > 0)
                    {
                        state.DisableGrappleBoost = true;
                    }
                    else if (bool.TryParse(Value, out bool bl) && bl)
                    {
                        state.DisableGrappleBoost = true;
                    }
                    else
                    {
                        state.DisableGrappleBoost = false;
                    }
                    break;
                case "ShortDistanceGrappleBoost":
                    if (int.TryParse(Value, out int num3) && num3 > 0)
                    {
                        state.ShortDistanceGrappleBoost = true;
                    }
                    else if (bool.TryParse(Value, out bool bl) && bl)
                    {
                        state.ShortDistanceGrappleBoost = true;
                    }
                    else
                    {
                        state.ShortDistanceGrappleBoost = false;
                    }
                    break;
                case "Ungrapple16Direction":
                    if (int.TryParse(Value, out int num4) && num4 > 0)
                    {
                        state.Ungrapple16Direction = true;
                    }
                    else if (bool.TryParse(Value, out bool bl) && bl)
                    {
                        state.Ungrapple16Direction = true;
                    }
                    else
                    {
                        state.Ungrapple16Direction = false;
                    }
                    break;
                case "HookStyle":
                    if (int.TryParse(Value, out int style))
                    {
                        if (style < 1 || style > 4)
                            style = 0;
                        state.HookStyle = style;
                        var hook = player.GetGrappleHook();
                        hook.SetStyle(style);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
