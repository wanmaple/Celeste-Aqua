using System;

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
