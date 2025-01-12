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
            return null;
        }
    }
}
