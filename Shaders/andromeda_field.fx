#include "common.fxh"

float2 Resolution;
float Time;
float TimeOffset;
float3 BaseColor;
float3 OffsetColor;
float Speed;
float Angle;
float LayerCount;

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

float star(float2 uv, float flare)
{
    float d = length(uv);
    float m = 0.06 / (d + 1e-4);
    float rays = max(0., 1. -abs(uv.x * uv.y * 1000.0));
    m += rays * flare;
    uv = mul(uv, RotationMatrix(PI * 0.25));
    rays = max(0.0, 1.0 - abs(uv.x * uv.y * 1000.0));
    m += rays * 0.3 * flare;
    
    m *= smoothstep(1.0, 0.2, d);
    return m;
}

float hash21(float2 p)
{
    p = frac(p * float2(109.11, 872.17));
    p += dot(p, p + 36.42);
    return frac(p.x * p.y);
}

float3 starLayer(float2 uv)
{
    float3 col = (0.0).xxx;
    float2 gv = frac(uv) - 0.5;
    float2 id = floor(uv);
    float time = Time + TimeOffset;
    for (int y = -1; y <= 1; y++) {
        for (int x = -1; x <= 1; x++) {
            float2 offset = float2(x, y);
            float n = hash21(id + offset);
            float size = frac(n * 982.35);
            float d = star(gv - offset - float2(n, frac(n * 46.0)) + 0.5, smoothstep(0.85, 1.0, size) * 0.6);
            float3 color = sin(float3(0.4, 0.3, 0.5) * frac(n * 297.2) * time) * 0.5 + 0.5;
            // color = color * float3(1.0 + size, 0.5, 1.0);
            color = color * (BaseColor + OffsetColor * size);
            d *= sin(time * 3.0 + n * 6.2831) * 0.5 + 1.0;
            col += d * size * color;
        }
    }
    return col;
}

//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(VS_Out input) : SV_TARGET0
{
    float2 uv = (input.uv - 0.5 * Resolution.xy) / Resolution.y;
    float2 ratio = (Resolution.xy * 0.5 * Angle2Direction(Angle)) / Resolution.y;
    float time = Time + TimeOffset;
    float t = time * 0.01;
    uv += ratio * Speed;
    uv = mul(uv, RotationMatrix(t));
    float3 col = (0.0).xxx;
    for (float i = 0.0; i < 1.0; i += 1.0 / LayerCount) {
        float depth = frac(i + t);
        float scale = lerp(20.0, 0.5, depth);
        float fade = depth * smoothstep(1.0, 0.9, depth);
        col += starLayer(uv * scale + i * 456.1 - ratio) * fade;
    }
    float4 color = float4(col, 1.0);
    return color;
}

technique AndromedaField
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}