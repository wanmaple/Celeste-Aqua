using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public class DefaultValueAttribute : Attribute
    {
        public object DefaultValue { get; set; }

        public DefaultValueAttribute(object value)
        {
            DefaultValue = value;
        }
    }
}
