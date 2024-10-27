using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class ExtendedFields
    {
        public static T GetField<T>(object owner, string name)
        {
            if (!_allFields.TryGetValue(owner, out var fields))
            {
                return default(T);
            }
            if (!fields.TryGetValue(name, out var field))
            {
                return default(T);
            }
            return (T)field;
        }

        public static void SetField(object owner, string name, object value)
        {
            if (!_allFields.TryGetValue(owner, out var fields))
            {
                fields = new Dictionary<string, object>(16);
                _allFields.Add(owner, fields);
            }
            fields[name] = value;
        }

        private static Dictionary<object, Dictionary<string, object>> _allFields = new Dictionary<object, Dictionary<string, object>>(32);
    }
}
