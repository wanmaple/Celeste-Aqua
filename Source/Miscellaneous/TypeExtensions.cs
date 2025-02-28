using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class TypeExtensions
    {
        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        public static FieldInfo FindField(this Type type, BindingFlags flags, params string[] possibleNames)
        {
            FieldInfo field = null;
            foreach (string name in possibleNames)
            {
                field = type.GetField(name, flags);
                if (field != null)
                    break;
            }
            return field;
        }

        public static PropertyInfo FindProperty(this Type type, BindingFlags flags, params string[] possibleNames)
        {
            PropertyInfo prop = null;
            foreach (string name in possibleNames)
            {
                prop = type.GetProperty(name, flags);
                if (prop != null && prop.CanRead)
                    break;
            }
            return prop;
        }

        public static object GetFieldOrPropertyValue(this Type type, object instance, BindingFlags flags, string name)
        {
            PropertyInfo property = type.FindProperty(flags, name);
            if (property != null)
            {
                return property.GetValue(instance);
            }
            FieldInfo field = type.FindField(flags, name);
            if (field != null)
            {
                return field.GetValue(instance);
            }
            return null;
        }

        public static bool CallMethodIfExist(this Type type, object instance, BindingFlags flags, string name, out object ret, params object[] args)
        {
            ret = null;
            MethodInfo method = type.GetMethod(name, flags);
            if (method != null)
            {
                ret = method.Invoke(instance, args);
                return true;
            }
            return false;
        }
    }
}
