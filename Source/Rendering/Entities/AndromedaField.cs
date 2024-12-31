using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.Aqua.Rendering
{
    public struct AndromedaFieldParameters
    {
        public float TimeOffset { get; set; }
        public Color BaseColor { get; set; }
        public Color OffsetColor { get; set; }
        public float Speed { get; set; }
        public float Angle { get; set; }
        public int LayerCount { get; set; }

        public AndromedaFieldParameters()
        {
            TimeOffset = new Random().Range(100.0f, 1000.0f);
            BaseColor = new Color(1.0f, 0.5f, 1.0f);
            OffsetColor = Color.Red;
            Speed = 3.0f;
            Angle = 135.0f;
            LayerCount = 4;
        }

        public void Parse(IReadOnlyList<UniformData> uniforms)
        {
            foreach (UniformData uniform in uniforms)
            {
                PropertyInfo prop = GetType().GetProperty(uniform.UniformName);
                if (prop != null)
                {
                    prop.SetValue(this, uniform.Parse());
                }
            }
        }
    }

    public class AndromedaField : CustomBackground
    {
        public AndromedaFieldParameters Arguments { get; private set; }

        public AndromedaField(AndromedaFieldParameters args)
            : base("andromeda_field")
        {
            Arguments = args;
            UpdateUniforms();
        }

        protected override void UpdateUniforms()
        {
            Effect eff = GetEffect();
            eff.Parameters["TimeOffset"].SetValue(Arguments.TimeOffset);
            eff.Parameters["BaseColor"].SetValue(Arguments.BaseColor.ToVector3());
            eff.Parameters["OffsetColor"].SetValue(Arguments.OffsetColor.ToVector3());
            eff.Parameters["Speed"].SetValue(Arguments.Speed);
            eff.Parameters["Angle"].SetValue(Calc.DegToRad * Arguments.Angle);
            eff.Parameters["LayerCount"].SetValue((float)Arguments.LayerCount);
        }
    }
}
