using Celeste.Mod.Aqua.Miscellaneous;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public static class HitboxExtensions
    {
        public static IReadOnlyList<Edge> Edges(this Hitbox self)
        {
            List<Edge> edges = DynamicData.For(self).Get<List<Edge>>("edges");
            if (edges == null)
            {
                edges = new List<Edge>(4);
                Vector2 tl = new Vector2(self.AbsoluteLeft, self.AbsoluteTop);
                Vector2 tr = new Vector2(self.AbsoluteRight, self.AbsoluteTop);
                Vector2 bl = new Vector2(self.AbsoluteLeft, self.AbsoluteBottom);
                Vector2 br = new Vector2(self.AbsoluteRight, self.AbsoluteBottom);
                edges.Add(new Edge(tl, tr, -Vector2.UnitY));
                edges.Add(new Edge(tl, bl, -Vector2.UnitX));
                edges.Add(new Edge(tr, br, Vector2.UnitX));
                edges.Add(new Edge(bl, br, Vector2.UnitY));
                DynamicData.For(self).Set("edges", edges);
            }
            return edges;
        }
    }
}
