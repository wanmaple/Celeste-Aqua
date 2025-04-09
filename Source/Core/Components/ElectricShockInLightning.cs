using Monocle;

namespace Celeste.Mod.Aqua.Core
{
    public class ElectricShockInLightning : Component
    {
        public ElectricShockInLightning()
            : base(true, false)
        { }

        public override void Update()
        {
            Entity.CheckRopeIntersectionOfElectricity();
        }
    }
}
