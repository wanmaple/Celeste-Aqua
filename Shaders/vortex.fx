#include "common.fxh"

#define HASH_1 float4(0.0, 1.0, 57.0, 58.0)
#define HASH_2 float3(1.0, 57.0, 113.0)
#define HASH_M 43758.54

float2 Resolution;
float Time;
float3 Color1;
float3 Color2;
float Duration;

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
	vs_out.uv = uv * Resolution;
	return vs_out;
}

float4 Hashv4f(float p)
{
    return frac(sin(p + HASH_1) * HASH_M);
}

float Noisefv2(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float4 t = Hashv4f(dot(i, HASH_2.xy));
    return lerp(lerp(t.x, t.y, f.x), lerp(t.z, t.w, f.x), f.y);
}

float Fbm2(float2 p)
{
    float s = 0.;
    float a = 1.0;
    for (int i = 0; i < 6; i++) {
        s += a * Noisefv2(p);
        a *= 0.5;
        p *= 2.0;
    }
    return s;
}

float2 VortF(float2 q, float2 c)
{
    float2 d = q - c;
    return 0.25 * float2 (d.y, - d.x) / (dot(d, d) + 0.05);
}

float2 FlowField(float2 q)
{
    float2 vr, c;
    float dir = 1.;
    c = float2 (fmod(Time, 10.0) - 20.0, 0.6 * dir);
    vr = float2(0.0, 0.0);
    for (int k = 0; k < 30; k ++) {
        vr += dir * VortF(4.0 * q, c);
        c = float2 (c.x + 1.0, - c.y);
        dir = - dir;
    }
    return vr;
}

//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(VS_Out input) : SV_TARGET0
{
    float2 uv = input.uv / Resolution - 0.5;
    uv.x *= Resolution.x / Resolution.y;
    float2 p = uv;
    for (int i = 0; i < 10; i ++)
        p -= FlowField(p) * 0.03;
    float t = fmod(Time, Duration) / Duration;
    float3 baseColor = lerp(Color1, Color2, (t < 0.5 ? t * 2.0 : (1.0 - (t - 0.5) * 2.0)));
    float3 col = Fbm2(5.0 * p + float2(-0.1 * Time, 0.0)) * baseColor;
    float4 color = float4(col, 1.0);
    return color;
}

technique Vortex
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}