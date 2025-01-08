using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Aqua.Module;
using Celeste.Mod.Aqua.Debug;

namespace Celeste.Mod.Aqua.Rendering
{
    public class SelfCircuitParameters
    {
        public float TimeRatio { get; set; }
        public float PeriodAngle { get; set; }
        public float FlowStrength { get; set; }
        public float Density { get; set; }
        public Color BackgroundColor1 { get; set; }
        public Color BackgroundColor2 { get; set; }
        public Color BackgroundColor3 { get; set; }
        public Color LineColor1 { get; set; }
        public Color LineColor2 { get; set; }
        public bool GravityControl { get; set; }

        public SelfCircuitParameters()
        {
            TimeRatio = 0.5f;
            PeriodAngle = 90.0f;
            FlowStrength = 4.0f;
            Density = 2.85f;
            BackgroundColor1 = new Color(0.45f, 0.0f, 0.8f);
            BackgroundColor2 = new Color(0.85f, 0.2f, 0.2f);
            BackgroundColor3 = new Color(0.5f, 0.85f, 0.55f);
            LineColor1 = Color.Green;
            LineColor2 = Color.Red;
            GravityControl = false;
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

    public class SelfCircuit : CustomBackground
    {
        public SelfCircuitParameters Arguments { get; private set; }

        public SelfCircuit(SelfCircuitParameters args)
            : base("self_circuit")
        {
            Arguments = args;
            UpdateUniforms();
        }

        public override void Update()
        {
            base.Update();
            UpdateLineColors();
        }

        protected override void UpdateUniforms()
        {
            Effect eff = GetEffect();
            eff.Parameters["TimeRatio"].SetValue(Arguments.TimeRatio);
            eff.Parameters["FlowStrength"].SetValue(Arguments.FlowStrength);
            eff.Parameters["Density"].SetValue(Arguments.Density);
            eff.Parameters["BackgroundColor1"].SetValue(Arguments.BackgroundColor1.ToVector3());
            eff.Parameters["BackgroundColor2"].SetValue(Arguments.BackgroundColor2.ToVector3());
            eff.Parameters["BackgroundColor3"].SetValue(Arguments.BackgroundColor3.ToVector3());
            UpdateLineColors();
        }

        private void UpdateLineColors()
        {
            Effect eff = GetEffect();
            if (eff != null)
            {
                if (Arguments.GravityControl)
                {
                    float sign = ModInterop.GravityHelper.IsPlayerGravityInverted() ? -1.0f : 1.0f;
                    _timer = Calc.Clamp(_timer + Engine.DeltaTime * 4.0f * -sign, 0.0f, 1.0f);
                    Color color = Color.Lerp(Arguments.LineColor1, Arguments.LineColor2, Ease.CubeInOut(_timer));
                    eff.Parameters["LineColor1"].SetValue(color.ToVector3());
                    eff.Parameters["LineColor2"].SetValue(color.ToVector3());
                    eff.Parameters["PeriodAngle"].SetValue(Calc.DegToRad * Arguments.PeriodAngle * sign);
                }
                else
                {
                    eff.Parameters["LineColor1"].SetValue(Arguments.LineColor1.ToVector3());
                    eff.Parameters["LineColor2"].SetValue(Arguments.LineColor2.ToVector3());
                    eff.Parameters["PeriodAngle"].SetValue(Calc.DegToRad * Arguments.PeriodAngle);
                }
            }
        }

        private float _timer = 0.0f;
    }
}
