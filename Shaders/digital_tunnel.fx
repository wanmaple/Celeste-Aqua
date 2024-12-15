
#define RAYMARCH_ITERATIONS 30.0
#define TIME (Time * 0.4)
#define LINE_LENGTH 1.0
#define LINE_SPACE 1.0
#define LINE_WIDTH 0.007
#define BOUNDING_CYLINDER 1.8
#define INSIDE_CYLINDER 0.32
#define EPS 0.0001
#define FOG_DISTANCE 30.0

#define FIRST_COLOR float3(1.2, 0.5, 0.2) * 1.2
#define SECOND_COLOR float3(0.2, 0.8, 1.1)
// #define FIRST_COLOR float3(1.2, 1.1, 0.2) * 1.2
// #define SECOND_COLOR float3(0.3, 0.5, 1.1)

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

float hash12(float2 x)
{
 	return frac(sin(dot(x, float2(42.2347, 43.4271))) * 342.324234);   
}

float3 hash33(float3 x)
{
 	return frac(sin(mul(x, float3x3(23.421, 24.4217, 25.3271, 27.2412, 32.21731, 21.27641, 20.421, 27.4217, 22.3271))) * 342.324234);   
}

float3 castPlanePoint(float2 uv)
{
 	float2 st = (2.0 * uv - Resolution.xy) / Resolution.x;
    return float3(st.xy, -1.0);
    // return float3(-st.y, -1.0, -st.x);
}

float planeSDF(float3 pt)
{
 	return pt.y;
}

float boxSDF( float3 pt, float3 bounds )
{
    float3 q = abs(pt) - bounds;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

//rgb - colors, a - sdf
float4 repeatBoxSDF(float3 pt)
{
    float3 rootPoint = floor(float3(pt.x / LINE_SPACE, pt.y / LINE_SPACE, pt.z / LINE_LENGTH)); 
    rootPoint.z *= LINE_LENGTH;
    rootPoint.xy *= LINE_SPACE;
    float minSDF = 10000.0;
    float3 mainColor = (0.0).xxx;
    
    for (float x = -1.0; x <= 1.1; x++)
    {
        for (float y = -1.0; y <= 1.1; y++)
        {
			for (float z = -1.0; z <= 1.1; z++)
            {
				float3 tempRootPoint = rootPoint + float3(x * LINE_SPACE, y * LINE_SPACE, z * LINE_LENGTH);
                
                float3 lineHash = hash33(tempRootPoint);
                lineHash.z = pow(lineHash.z, 10.0);
                
                float hash = hash12(tempRootPoint.xy) - 0.5;
                tempRootPoint.z += hash * LINE_LENGTH;
                
                float3 boxCenter = tempRootPoint + float3(0.5 * LINE_SPACE, 0.5 * LINE_SPACE, 0.5 * LINE_LENGTH);
                boxCenter.xy += (lineHash.xy - 0.5) * LINE_SPACE;
                float3 boxSize = float3(LINE_WIDTH, LINE_WIDTH, LINE_LENGTH * (1.0 - lineHash.z));
                
                float3 color = FIRST_COLOR;
                if(lineHash.x < 0.5) color = SECOND_COLOR;
                
                float sdf = boxSDF(pt - boxCenter, boxSize);
                if (sdf < minSDF)
                {
                    mainColor = color;
                    minSDF = sdf;
                }
            }
        }
    }
    
    return float4(mainColor, minSDF);
}

float cylinderSDF(float3 pt, float radius)
{
 	return length(pt.xy) - radius;
}

float multiplyObjects(float o1, float o2)
{
 	return max(o1, o2);   
}

float3 spaceBounding(float3 pt)
{
 	return float3(sin(pt.z * 0.15) * 5.0, cos(pt.z * 0.131) * 5.0, 0.0); 
}

//rgb - color, a - sdf
float4 objectSDF(float3 pt)
{
    pt += spaceBounding(pt);
    
    float4 lines = repeatBoxSDF(pt);
    float cylinder = cylinderSDF(pt, BOUNDING_CYLINDER);
    float insideCylinder = -cylinderSDF(pt, INSIDE_CYLINDER);
    
    float object = multiplyObjects(lines.a, cylinder);
    object = multiplyObjects(object, insideCylinder);
 	return float4(lines.rgb, object);
}

float3 rayMarch(float3 rayOrigin, float3 rayDirection, out float3 color)
{
    color = (0.0).xxx;
    float dist = 0.0;
 	for (float i = 0.0; i < RAYMARCH_ITERATIONS; i++)
    {
     	float4 sdfData = objectSDF(rayOrigin);
        color += sdfData.rgb * sqrt(smoothstep(0.8, 0.0, sdfData.a)) * pow(smoothstep(FOG_DISTANCE * 0.6, 0.0, dist), 3.0) * 0.2;
        rayOrigin += rayDirection * sdfData.a * 0.7;
        dist += sdfData.a;
        if (length(rayOrigin.xy) > BOUNDING_CYLINDER + 10.0) break;
    }


    return rayOrigin;
}
 
//-----------------------------------------------------------------------------
// Fragment Shaders.
//-----------------------------------------------------------------------------
float4 FS_Main(float4 inPosition : SV_Position, float4 inColor : COLOR0, float2 uv : TEXCOORD1) : SV_TARGET0
{
	float3 cameraCenter = float3(0.0, 0.0, -TIME * 10.0);
    cameraCenter -= spaceBounding(cameraCenter);
    
    float3 prevCameraCenter = float3(0.0, 0.0, -(TIME - 0.01) * 10.0);
    prevCameraCenter -= spaceBounding(prevCameraCenter);
    float3 nextCameraCenter = float3(0.0, 0.0, -(TIME + 0.4) * 10.0);
    nextCameraCenter -= spaceBounding(nextCameraCenter);
    
    float3 velocityVector = -normalize(nextCameraCenter - prevCameraCenter);
    float3 cameraUp = -normalize(cross(velocityVector, float3(1.0, 0.0, 0.0)));
    float3 cameraRight = -(cross(velocityVector, cameraUp));
    
    float3x3 cameraRotation = float3x3(cameraRight, cameraUp, velocityVector);
    
    float3 rayOrigin = cameraCenter;
    float3 rayDirection = mul(normalize(castPlanePoint(uv)), cameraRotation);
    
    float3 color = (0.0).xxx;
    float3 hitPoint = rayMarch(rayOrigin, rayDirection, color);
    float4 sdf = objectSDF(hitPoint);
    
    float vision = smoothstep(0.01, 0.0, sdf.a);
    float fog = sqrt(smoothstep(FOG_DISTANCE, 0.0, distance(cameraCenter, hitPoint)));
    float3 ambient = lerp(SECOND_COLOR, FIRST_COLOR, pow(sin(TIME) * 0.5 + 0.5, 2.0) * 0.6);
    ambient *= sqrt((sin(TIME) + sin(TIME * 3.0)) * 0.25 + 1.0);
    float3 bloom = smoothstep(-0.0, 15.0, color);
    
    color = color * vision * 0.07 * fog + bloom + ambient * 0.3;
    color = smoothstep(-0.01, 1.5, color * 1.1);
    
    return float4(color, 0.5);
}

technique DigitalTunnelTech
{
    pass
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 FS_Main();
    }
}