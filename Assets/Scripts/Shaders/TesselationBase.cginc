#if !defined(TESSELATION_INCLUDED)
#define TESSELATION_INCLUDED

#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || defined(SHADER_API_PSSL)
#define UNITY_CAN_COMPILE_TESSELLATION 1
#define UNITY_domain                 domain
#define UNITY_partitioning           partitioning
#define UNITY_outputtopology         outputtopology
#define UNITY_patchconstantfunc      patchconstantfunc
#define UNITY_outputcontrolpoints    outputcontrolpoints
#endif


struct TessFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

struct TessControlPoint{
    float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float4 uv2 : TEXCOORD2;
};


struct VertexData{
    float4 vertex: POSITION;
    float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
};

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("constantFunction")]
TessControlPoint HullProgram(InputPatch<TessControlPoint,3> patch, uint ID: SV_outputControlPointID)
{
    return patch[ID];
}

TessFactors constantFunction(InputPatch<TessControlPoint,3> patch)
{
    TessFactors factors;
    factors.edge[0] = 10;
    factors.edge[1] = 10;
    factors.edge[2] = 10;
    factors.inside = 10;
    return factors;
}

/* TessControlPoint VertexProgram(VertexData v)
{
        TessControlPoint t;
        t.vertex = v.vertex;
        t.normal = v.normal;
        t.tangent = v.tangent;
        t.uv = v.uv;
        t.uv2 = v.uv2;
        t.uv1 = v.uv1;
        return t;
} */


#endif




