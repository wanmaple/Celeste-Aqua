#include "common.fxh"

DECLARE_TEXTURE(source, 0);
float HueOffset;
float SaturationOffset;

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};

float4 FS_Main(VertexShaderOutput input) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(source, input.uv) * input.color;
    float3 hsl = RGB2HSL(color.rgb);
    hsl.x = frac(hsl.x + HueOffset);
    hsl.y = saturate(hsl.y + SaturationOffset);
    color.rgb = HSL2RGB(hsl);
    return color;
}

technique HueOffsetTech
{
    pass
    {
        // VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}