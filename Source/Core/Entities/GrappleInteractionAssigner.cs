using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Core
{
    public abstract class GrappleInteractionAssigner : Entity
    {
        public IReadOnlyList<string> Blacklist { get; private set; }

        protected GrappleInteractionAssigner(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            string blacklist = data.Attr("blacklist");
            if (!string.IsNullOrWhiteSpace(blacklist))
            {
                string[] array = blacklist.Split(',').Select(str => str.Trim()).Where(str => !string.IsNullOrEmpty(str)).ToArray();
                Blacklist = array;
            }
            else
            {
                Blacklist = Array.Empty<string>();
            }
        }

        protected bool IsInBlacklist(Entity entity)
        {
            string fullname = entity.GetType().FullName;
            foreach (string black in Blacklist)
            {
                if (fullname.Contains(black, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
