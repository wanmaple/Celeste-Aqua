namespace Celeste.Mod.Aqua.Core
{
    public class PlayerData
    {
        public bool ThrowingHook { get; set; } = false;

        public void Reset()
        {
            ThrowingHook = false;
        }
    }
}
