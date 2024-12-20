#define PI 3.14159
#define rot(a) float2x2(cos(a), -sin(a), sin(a), cos(a)) // col1a col1b col2a col2b
#define TIME_DIV 1.5

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

float2 random2(float2 p) {
    return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453);
}

float voronoi(float2 uv) 
{
    float2 cell = floor(uv);
    float2 fraction = frac(uv);
    float ret = 100.;
    
    float t = Time / TIME_DIV;
    float change = floor(t / 2.) + frac(t) * floor(fmod(t, 2.));
    
    for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <=1; j++) {
        	float2 neighbor = float2(float(i), float(j));
            float2 rand = random2(cell + neighbor);
            float t = Time / 1.5;
            rand = 0.5 + 0.5 * sin(change * 4. + 2. * PI * rand);
            float2 toCenter = neighbor + rand - fraction;
            ret = min(ret, max(abs(toCenter.x), abs(toCenter.y)));
        }
    }
    
    return ret;
}

float2 gradient(in float2 x, float thickness)
{
	float2 h = float2(thickness, 0.);
    return float2(voronoi(x + h.xy) - voronoi(x - h.xy),
               voronoi(x + h.yx) - voronoi(x - h.yx)) / (2. * h.x);
}

//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(VS_Out input) : SV_TARGET0
{    
	float2 uv = input.uv;
    uv.x *= Resolution.x / Resolution.y;
    
    float t = Time / TIME_DIV;
    float change = floor(t / 2.) + frac(t) * floor(fmod(t, 2.));
    float colSwitch = sin(change * PI / 2.);
    
    uv -= .5;
    uv = mul(uv, rot(change * PI / 2.));
    uv += .5;
    
    uv *= 2.85;
    
    float val = voronoi(uv) / length(gradient(uv, .02));
    float colVal = pow(val, 1.1) * 1.05;
    
    float3 color = lerp(float3(0., colVal, 0.), 
                        lerp(float3(0., 0., colVal), float3(colVal, 0., 0.), clamp(colSwitch, .0, 1.)),
                        clamp(colSwitch + 1., 0., 1.));
    color = lerp(lerp(float3(.45, .0, .8), 
                        lerp(float3(.85, .2, .2), float3(.5, .85, .55), clamp(colSwitch, .0, 1.)),
                        clamp(colSwitch + 1., 0., 1.)),
                        color, colVal);
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