using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public class HookInteractable : Component
    {
        public delegate bool InteractHookHandler(GrapplingHook hook, Vector2 at);

        public InteractHookHandler Interaction { get; set; }

        public HookInteractable(InteractHookHandler interaction)
            : base(false, false)
        {
            Interaction = interaction;
        }

        public bool OnInteract(GrapplingHook hook, Vector2 at)
        {
            if (Interaction != null)
            {
                return Interaction.Invoke(hook, at);
            }
            return false;
        }
    }
}
