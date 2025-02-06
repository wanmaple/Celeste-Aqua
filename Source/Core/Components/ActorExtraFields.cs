using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class ActorExtraFields : Component
    {
        public float Mass { get; set; }
        public float StaminaCost { get; set; }

        public ActorExtraFields()
            : base(false, false)
        { }
    }
}
