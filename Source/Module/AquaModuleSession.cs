using Celeste.Mod.Aqua.Core;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module;

public class AquaModuleSession : EverestModuleSession
{
    public const string CH1A_SATELLITE_SOLVED = "satellite_solved";
    public const string CH2A_WHEEL_SOLVED = "wheel_solved";

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public void MarkFlag(string flag)
    {
        flags.Add(flag);
    }

    public HashSet<string> flags = new HashSet<string>();
    public LevelStates.LevelState levelState;
}