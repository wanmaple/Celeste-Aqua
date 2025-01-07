#include "common.fxh"

float2 Resolution;
float Time;
float TimeRatio;
float PeriodAngle;
float FlowStrength;
float Density;
float3 BackgroundColor1;
float3 BackgroundColor2;
float3 BackgroundColor3;
float3 LineColor1;
float3 LineColor2;

struct VS_Out
{
	float4 position : SV_Position;
	float4 color : COLOR0;
	float2 uv : TEXCOORD1;
};

//-----------------------------------------------------------------------------
// Vertex Shaders.
//-----------------------------------------------------------------------------
VS_Out VS_Main(
	float4 position : POSITION0,
    float4 color    : COLOR0,
    float2 uv : TEXCOORD0)
{
	VS_Out vs_out;
	vs_out.position = float4(position.xy * 2.0f, 1.0f, 1.0f);
	vs_out.color = color;
	vs_out.uv = uv;
	return vs_out;
}

float2 random2(float2 p) {
    return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
}

float voronoi(float2 uv) 
{
    float2 cell = floor(uv);
    float2 fraction = frac(uv);
    float ret = 100.;
    float t = Time * TimeRatio;
    float change = t;
    // float change = floor(t / 2.0) + frac(t) * floor(fmod(t, 2.0));
    for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <= 1; j++) {
        	float2 neighbor = float2(float(i), float(j));
            float2 rand = random2(cell + neighbor);
            rand = 0.5 + 0.5 * sin(change * FlowStrength + 2.0 * PI * rand);
            float2 toCenter = neighbor + rand - fraction;
            ret = min(ret, max(abs(toCenter.x), abs(toCenter.y)));
        }
    }
    
    return ret;
}

float2 gradient(float2 uv, float thickness)
{
	float2 h = float2(thickness, 0.0);
    return float2(voronoi(uv + h.xy) - voronoi(uv - h.xy), voronoi(uv + h.yx) - voronoi(uv - h.yx)) / (2.0 * h.x);
}

//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(VS_Out input) : SV_TARGET0
{    
	float2 uv = input.uv;
    uv.y *= Resolution.y / Resolution.x;
    
    float t = Time * TimeRatio;
    float change = t;
    // float change = floor(t / 2.0) + frac(t) * floor(fmod(t, 2.0));
    float colSwitch = sin(change * PI / 2.0);
    uv -= 0.5;
    uv = mul(uv, RotationMatrix(change * PeriodAngle));
    uv += 0.5;
    uv *= Density;
    float val = voronoi(uv) / length(gradient(uv, 0.02));
    float colVal = pow(abs(val), 1.1) * 1.05;
    float3 color = lerp(colVal * LineColor1, colVal * LineColor2, saturate(colSwitch));
    color = lerp(lerp(BackgroundColor1, lerp(BackgroundColor2, BackgroundColor3, saturate(colSwitch)),
        saturate(colSwitch + 1.0)), color, colVal);
    return float4(color, 1.0);
}

technique SelfReconfigureCircuit
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}