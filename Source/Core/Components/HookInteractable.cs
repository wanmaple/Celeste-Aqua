using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    [Tracked(true)]
    public class HookInteractable : Component
    {
        public delegate bool InteractHookHandler(GrapplingHook hook, Vector2 at);

        public HookInteractable(InteractHookHandler interaction)
            : base(false, false)
        {
            _interaction = interaction;
        }

        public bool OnInteract(GrapplingHook hook, Vector2 at)
        {
            if (_interaction != null)
            {
                return _interaction.Invoke(hook, at);
            }
            return false;
        }

        private InteractHookHandler _interaction;
    }
}
