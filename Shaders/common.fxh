#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

#define PI 3.1415926535
#define HALF_PI 1.5707963705

float SineIn(float t)
{
    return 1.0 - cos(PI * t);
}

float SineOut(float t)
{
    return sin(PI * t);
}

float SineInOut(float t)
{
    return 0.5 - cos(PI * t) * 0.5;
}

float3 Hue2RGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0f;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 RGB2HCV(float3 rgb)
{
    float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0 / 3.0) : float4(rgb.gb, 0.0, -1.0 / 3.0);
    float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
    float c = q.x - min(q.w, q.y);
    float h = abs((q.w - q.y) / (6.0 * c + 1e-10) + q.z);
    return float3(h, c, q.x);
}

float3 RGB2HSL(float3 rgb)
{
    float3 hcv = RGB2HCV(rgb);
    float l = hcv.z - hcv.y * 0.5;
    float s = hcv.y / (1.0 - abs(l * 2.0 - 1.0) + 1e-10);
    return float3(hcv.x, s, l);
}

float3 HSL2RGB(float3 hsl)
{
    float3 rgb = Hue2RGB(hsl.x);
    float c = (1.0 - abs(2.0 * hsl.z - 1.0)) * hsl.y;
    return (rgb - 0.5) * c + hsl.z;
}

float2 Angle2Direction(float angle)
{
    float s = sin(angle), c = cos(angle);
    return float2(c, -s);
}

float2x2 RotationMatrix(float angle)
{
    float s = sin(angle), c = cos(angle);
    return float2x2(c, -s, s, c);
}

// 2D SDFs
float Circle(float2 p, float r)
{
    return length(p) - r;
}

float Segment(float2 p, float2 a, float2 b, float l)
{
    float2 ap = p - a;
    float2 ab = b - a;
    float h = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    return length(ap - ab * h) - l;
}