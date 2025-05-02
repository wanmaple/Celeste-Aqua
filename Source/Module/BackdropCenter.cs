using Celeste.Mod.Aqua.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.Aqua.Module
{
    public static class BackdropCenter
    {
        public static void Initialize()
        {
            Everest.Events.Level.OnLoadBackdrop += Level_OnLoadBackdrop;
        }

        public static void Uninitialize()
        {
            Everest.Events.Level.OnLoadBackdrop -= Level_OnLoadBackdrop;
        }

        private static Backdrop Level_OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above)
        {
            if (child.Name.Equals("Aqua/Gravity Control Parallax", StringComparison.OrdinalIgnoreCase))
            {
                string texturePath = child.Attr("texture");
                Color normalColor = Calc.HexToColor(child.Attr("normal_gravity_color", "ffffff"));
                Color invertedColor = Calc.HexToColor(child.Attr("inverted_gravity_color", "ffffff"));
                var parallax = new GravityControlParallax(GFX.Game[texturePath], normalColor, invertedColor);
                if (child.HasAttr("blendmode"))
                {
                    switch (child.Attr("blendmode"))
                    {
                        case "additive":
                            parallax.BlendState = BlendState.Additive;
                            break;
                        default:
                            parallax.BlendState = BlendState.AlphaBlend;
                            break;
                    }
                }
                parallax.DoFadeIn = bool.Parse(child.Attr("fadeIn", "false"));
                return parallax;
            }
            else if (child.Name.Equals("Aqua/Andromeda Field", StringComparison.OrdinalIgnoreCase))
            {
                float timeOffset = child.AttrFloat("time_offset", 0.0f);
                Color baseColor = Calc.HexToColor(child.Attr("base_color", "ff7fff"));
                Color offsetColor = Calc.HexToColor(child.Attr("offset_color", "ff0000"));
                float speed = child.AttrFloat("speed", 3.0f);
                float angle = child.AttrFloat("angle", 135.0f);
                int layerCount = child.AttrInt("layer_count", 4);
                AndromedaFieldParameters args = new AndromedaFieldParameters
                {
                    TimeOffset = timeOffset,
                    BaseColor = baseColor,
                    OffsetColor = offsetColor,
                    Speed = speed,
                    Angle = angle,
                    LayerCount = layerCount,
                };
                var eff = new AndromedaField(args);
                return eff;
            }
            else if (child.Name.Equals("Aqua/Self Reconfiguration Circuit", StringComparison.OrdinalIgnoreCase))
            {
                float timeRatio = child.AttrFloat("time_ratio", 0.5f);
                float periodAngle = child.AttrFloat("period_angle", 90.0f);
                float flowStrength = child.AttrFloat("flow_strength", 4.0f);
                float density = child.AttrFloat("density", 2.85f);
                Color bgColor1 = Calc.HexToColor(child.Attr("background_color1", "7300cc"));
                Color bgColor2 = Calc.HexToColor(child.Attr("background_color2", "d93333"));
                Color bgColor3 = Calc.HexToColor(child.Attr("background_color3", "80d98c"));
                Color lineColor1 = Calc.HexToColor(child.Attr("line_color1", "00ff00"));
                Color lineColor2 = Calc.HexToColor(child.Attr("line_color2", "ff0000"));
                bool gravityControl = bool.Parse(child.Attr("gravity_control", "false"));
                SelfCircuitParameters args = new SelfCircuitParameters
                {
                    TimeRatio = timeRatio,
                    PeriodAngle = periodAngle,
                    FlowStrength = flowStrength,
                    Density = density,
                    BackgroundColor1 = bgColor1,
                    BackgroundColor2 = bgColor2,
                    BackgroundColor3 = bgColor3,
                    LineColor1 = lineColor1,
                    LineColor2 = lineColor2,
                    GravityControl = gravityControl
                };
                var eff = new SelfCircuit(args);
                return eff;
            }
            else if (child.Name.Equals("Aqua/Vortex", StringComparison.OrdinalIgnoreCase))
            {
                Color color1 = Calc.HexToColor(child.Attr("color1", "ff0000"));
                Color color2 = Calc.HexToColor(child.Attr("color2", "0000ff"));
                float duration = child.AttrFloat("duration", 4.0f);
                VortexParameters args = new VortexParameters
                {
                    Color1 = color1,
                    Color2 = color2,
                    Duration = duration,
                };
                var eff = new Vortex(args);
                return eff;
            }
            return null;
        }
    }
}
