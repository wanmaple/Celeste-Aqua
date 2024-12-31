using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Rendering
{
    [Serializable]
    public struct UniformData
    {
        public enum UniformTypes
        {
            Integer,
            Color,
            Float,
            Float2,
            Float3,
            Float4,
        }

        public string UniformName;
        public UniformTypes UniformType;
        public string UniformValue;

        public object Parse()
        {
            string[] splits = UniformValue.Split(',');
            switch (UniformType)
            {
                case UniformTypes.Integer:
                    return int.Parse(splits[0]);
                case UniformTypes.Color:
                    return Calc.HexToColor(splits[0]);
                case UniformTypes.Float:
                    return float.Parse(splits[0]);
                case UniformTypes.Float2:
                    return new Vector2(float.Parse(splits[0]), float.Parse(splits[1]));
                case UniformTypes.Float3:
                    return new Vector3(float.Parse(splits[0]), float.Parse(splits[1]), float.Parse(splits[2]));
                case UniformTypes.Float4:
                    return new Vector4(float.Parse(splits[0]), float.Parse(splits[1]), float.Parse(splits[2]), float.Parse(splits[3]));
            }
            return null;
        }
    }

    [Serializable]
    public struct BackgroundData
    {
        public string EntityName;
        public List<UniformData> Uniforms;
    }
}
