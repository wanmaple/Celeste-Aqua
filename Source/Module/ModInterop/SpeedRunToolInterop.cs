using Celeste.Mod.Aqua.Core;
using Celeste.Mod.Aqua.Miscellaneous;
using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Module
{
    public class SpeedRunToolInterop : Interop
    {
        public SpeedRunToolInterop()
            : base("SpeedrunTool", 3, 27, 14)
        { }

        [ModImportName("SpeedrunTool.SaveLoad")]
        public static class SpeedRunToolImports
        {
            public static Func<Action<Dictionary<Type, Dictionary<string, object>>, Level>,
            Action<Dictionary<Type, Dictionary<string, object>>, Level>, Action,
            Action<Level>, Action<Level>, Action, object> RegisterSaveLoadAction;
            public static Func<Type, string[], object> RegisterStaticTypes;
        }

        public void Load()
        {
            typeof(SpeedRunToolImports).ModInterop();
        }

        public override void Initialize()
        {
            // do nothing
        }

        public object RegisterSaveLoadAction(
            Action<Dictionary<Type, Dictionary<string, object>>, Level> saveState,
            Action<Dictionary<Type, Dictionary<string, object>>, Level> loadState,
            Action clearState,
            Action<Level> beforeSaveState,
            Action<Level> beforeLoadState,
            Action preCloneEntities)
        {
            if (IsLoaded && SpeedRunToolImports.RegisterSaveLoadAction != null)
            {
                return SpeedRunToolImports.RegisterSaveLoadAction.Invoke(saveState, loadState, clearState, beforeSaveState, beforeLoadState, preCloneEntities);
            }
            return null;
        }

        public object RegisterStaticTypes(Type type, params string[] memberNames)
        {
            if (IsLoaded && SpeedRunToolImports.RegisterStaticTypes != null)
            {
                return SpeedRunToolImports.RegisterStaticTypes.Invoke(type, memberNames);
            }
            return null;
        }
    }
}
