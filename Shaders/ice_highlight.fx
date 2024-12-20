#include "common.fxh"

float Time;
float2 Resolution;

struct VertexShaderOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};

float4 FS_Main(VertexShaderOutput input) : SV_TARGET0
{
    float2 st = input.uv * 2.0 - 1.0;
    st.x *= Resolution.x / Resolution.y;
    float len = min(Resolution.x / Resolution.y, 1.0) * 0.4;
    float off2 = len / 1.414 + 3.0 / Resolution.y;
    float off3 = off2 + 4.0 / Resolution.y + len * 0.5 / 1.414;
    float2 startOffset = -(max(Resolution.x / Resolution.y, 1.0)).xx;
    float2 endOffset = -startOffset + off3;
    startOffset -= len * 0.5;
    float duration = 1.5;
    float t = fmod(Time, duration) / duration * 2.0;
    t = t * step(t, 1.0);
    t = SineInOut(t);
    float2 offset = lerp(startOffset, endOffset, t);
    float seg1 = Segment(st - offset, float2(-1000.0, 1000.0), float2(1000.0, -1000.0), len);
    float seg2 = Segment(st - offset + off2.xx, float2(-1000.0, 1000.0), float2(1000.0, -1000.0), 1.0 / Resolution.y);
    float seg3 = Segment(st - offset + off3.xx, float2(-1000.0, 1000.0), float2(1000.0, -1000.0), len * 0.5);
    float a = step(min(min(seg1, seg2), seg3), 0.0);
    float4 color = saturate(a.xxxx) * 1.0;
    float4 bgColor = float4(105.0 / 255.0, 241.0 / 255.0, 255.0 / 255.0, 1.0);
    color = lerp(bgColor, color, a);
    return color;
}

technique IceHighlight
{
    pass
    {
        PixelShader = compile ps_3_0 FS_Main();
    }
}