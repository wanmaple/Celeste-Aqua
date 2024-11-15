using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class ReflectionHelper
    {
        public struct MethodDeclaration
        {
            public Type ReturnType { get; set; }
            public List<Type> ParameterTypes { get; private set; }

            public MethodDeclaration(Type returnType, params Type[] paramTypes)
            {
                ReturnType = returnType;
                ParameterTypes = new List<Type>(paramTypes);
            }
        }

        public static MethodInfo FindMethodInMod(string modName, string className, string methodName, MethodDeclaration declaration)
        {
            EverestModule everestModule = Enumerable.FirstOrDefault(Everest.Modules, (EverestModule m) => m.Metadata.Name == modName);
            if (everestModule != null)
            {
                Assembly asm = everestModule.GetType().Assembly;
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    if (type.Name == className)
                    {
                        MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                        if (method != null && SequenceTypeMatch(method.GetParameters(), declaration.ParameterTypes))
                        {
                            return method;
                        }
                    }
                }
            }
            return null;
        }

        private static bool SequenceTypeMatch(IReadOnlyList<ParameterInfo> argInfos, IReadOnlyList<Type> argTypes)
        {
            if (argInfos.Count != argTypes.Count)
                return false;

            for (int i = 0; i < argInfos.Count; i++)
            {
                ParameterInfo argInfo = argInfos[i];
                Type requireType = argTypes[i];
                if (argInfo.ParameterType != requireType)
                    return false;
            }
            return true;
        }
    }
}
