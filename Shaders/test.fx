//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

texture tex_albedo : register(t0);
sampler tex_albedoSampler : register(s0);

struct VS_Out
{
	float4 position : SV_Position;
	float4 color : COLOR0;
	float2 uv : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Vertex Shaders.
//-----------------------------------------------------------------------------

float4x4 MatrixTransform;

VS_Out VS_Function(
	float4 position : POSITION0,
    float4 color    : COLOR0,
    float2 uv : TEXCOORD0)
{
	VS_Out vs_out;
	vs_out.position = mul(position, MatrixTransform);
	vs_out.color = color;
	vs_out.uv = uv;
	return vs_out;
}

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_Function(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD1) : SV_TARGET0
{
    return float4(uv.xy, 0.0f, 1.0f);
    // return (1.0).xxxx;
    // float4 current = SAMPLE_TEXTURE(tex_albedo, uv);

    // float visible = 0;
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(pixel.x, 0)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(-pixel.x, 0)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(0, pixel.y)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(0, -pixel.y)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(pixel.x, pixel.y)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(-pixel.x, -pixel.y)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(-pixel.x, pixel.y)).a);
    // visible = max(visible, SAMPLE_TEXTURE(tex_albedo, uv + float2(pixel.x, -pixel.y)).a);

    // return visible * float4(current.rgb * current.a, 1);
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique Test
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Function();
        PixelShader = compile ps_3_0 PS_Function();
    }
}