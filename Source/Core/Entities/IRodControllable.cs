namespace Celeste.Mod.Aqua.Core
{
    public interface IRodControllable
    {
        int Group { get; }
        bool IsRunning { get; }

        void Run();
    }
}
