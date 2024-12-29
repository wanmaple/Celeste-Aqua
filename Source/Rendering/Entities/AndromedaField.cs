using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

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
    }

    public class AndromedaField : CustomBackground
    {
        public AndromedaFieldParameters Arguments { get; private set; }

        public AndromedaField(AndromedaFieldParameters args)
            : base("andromeda_field")
        {
            Arguments = args;
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
