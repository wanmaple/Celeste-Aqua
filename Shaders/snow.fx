#include "common.fxh"

#define LAYERS 4
#define DEPTH 1.0
#define WIDTH 0.2
#define SPEED 0.6
// blizzard
// #define LAYERS 200
// #define DEPTH 0.1
// #define WIDTH 0.8
// #define SPEED 1.5

float2 Resolution;
float Time;

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

//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(VS_Out input) : SV_TARGET0
{
    const float3x3 mat = float3x3(13.323122, 23.5112, 21.71123, 21.1212, 28.7312, 11.9312, 21.8112, 14.7212, 61.3934);
    float2 uv = float2(0.5, 0.5) + float2(1.0, Resolution.y / Resolution.x) * input.uv;
    float3 acc = (0.0).xxx;
	float dof = 5.0 * sin(Time * 0.1);
	for (float i = 0; i < LAYERS; i++) {
        float2 q = uv * (1.0 + (i + 4.0) * DEPTH);
        q += float2(q.y * (WIDTH * fmod(i * 7.238917, 1.0) - WIDTH * 0.5), SPEED * Time / (1.0 + i * DEPTH * 0.03));
        float3 n = float3(floor(q), 31.189 + i);
        float3 m = floor(n) * 0.00001 + frac(n);
        float3 mp = (31415.9 + m) / frac(mul(m, mat));
        float3 r = frac(mp);
        float2 s = abs(fmod(q, 1.0) - (0.5).xx + 0.9 * r.xy - (0.45).xx);
        s += 0.01 * abs(2.0 * frac(10.0 * q.yx) - (1.0).xx);
        float d = 0.6 * max(s.x - s.y, s.x + s.y) + max(s.x, s.y) - 0.01;
        float edge = 0.005 + 0.05 * min(0.5 * abs(i - 5.0 - dof), 1.0);
        acc += (smoothstep(edge, -edge, d) * (r.x / (1.0 + 0.02 * i * DEPTH))).xxx;
	}
    return float4(acc, acc.x);
}

technique Snow
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}