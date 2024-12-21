#define NUM_LAYERS 3.0

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
	vs_out.uv = uv * Resolution;
	return vs_out;
}

float2x2 Rot(float a) {
    float s=sin(a), c=cos(a);
    return float2x2(c, -s, s, c);
}

float Star(float2 uv, float flare) {
    float d = length(uv);
    float m = .06/d;
    //col += m;
    float rays = max(0., 1. -abs(uv.x*uv.y*1000.));
    m += rays * flare;
    uv = mul(uv, Rot(3.14159 / 4.0));
    rays = max(0., 1. -abs(uv.x*uv.y*1000.));
    m += rays*.3*flare;
    
    m *= smoothstep(1., .2, d);
    return m;
}

float Hash21(float2 p) {
    p = frac(p*float2(109.11, 872.17));
    p += dot(p, p+36.42);
    return frac(p.x*p.y);
}

float3 StarLayer(float2 uv) {
    float3 col = (0.0).xxx;

    float2 gv = frac(uv)-.5;
    float2 id = floor(uv);
    
    for(int y=-1;y<=1;y++) {
        for(int x=-1;x<=1;x++) {
            float2 offs = float2(x, y);
            
            float n = Hash21(id+offs);
            float size = frac(n*982.35);
            float star = Star(gv-offs-float2(n, frac(n*46.))+.5, smoothstep(.85, 1., size)*.6);
            
            float3 color = sin(float3(.4, .3, .5)*frac(n*297.2)*Time)*.5+.5;
            color = color*float3(1.+size,.5,1);
            
            star *= sin(Time*3.+n*6.2831)*.5+1.;
            col += star*size*color;
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
    float2 M = (Resolution.xy * 0.5 - Resolution.xy) / Resolution.y;
    float t = Time * 0.01;
    
    uv += M*3.;
    
    uv = mul(uv, Rot(t));
    
    float3 col = (0.0).xxx;
    
    for(float i=0.; i<1.; i+=1./NUM_LAYERS) {
        float depth = frac(i+t);
        
        float scale = lerp(20., .5, depth);
        float fade = depth*smoothstep(1., .9, depth);
        col += StarLayer(uv*scale+i*456.1-M)*fade;
    }
    
    //if(gv.x > .48 || gv.y >.48) col.r=1.;
    float4 color = float4(col,1.0);
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