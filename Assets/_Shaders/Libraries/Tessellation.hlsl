#if !defined(TESSELLATION_LIB)
#define TESSELLATION_LIB

#include "Generic.hlsl"

float TessellationUniform;
float TessellationEdgeLength;
float TessellationMinDist;

struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct TessellationControlPoint
{
	float4 position : INTERNALTESSPOS;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float3 worldPosition : TEXCOORD1;
	float3 color : COLOR;
	float3 localPosition : TEXCOORD2;
};

float TessellationEdgeFactor(float3 p0, float3 p1)
{
	float edgeLength = distance(p0, p1);
	float3 edgeCenter = (p0 + p1) * 0.5;
	float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);

    float edgeThreshold = TessellationEdgeLength;
    float maxDist = TessellationMinDist;
    #if defined(TESSEL_EDGE_LENGTH)
    edgeThreshold = TESSEL_EDGE_LENGTH;
    #endif
    #if defined(TESSEL_MAX_DIST)
    maxDist = TESSEL_MAX_DIST;
    #endif

    if (viewDistance <= maxDist)
    {
        return edgeLength * edgeThreshold;
    }
    /*
	float factor = 1 - saturate((viewDistance - TessellationMinDist) / TessellationEdgeLength);

	if (factor == 0) {
		return 1;
	}
*/
	return 0;
}

bool IsTriangleVisible(float3 p0, float3 p1, float3 p2) {
#if defined(TRIANGLE_ALWAYS_VISIBLE)
	return true; 
#else
	return (WorldTriangleVisible(p0, p1, p2));
#endif
}

TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch)
{
	float3 p0 = mul(unity_ObjectToWorld, patch[0].position).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].position).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].position).xyz;
	TessellationFactors f;
	if (IsTriangleVisible(p0, p1, p2))
	{
#if defined(TESSELLATION_EDGE)
		f.edge[0] = TessellationEdgeFactor(p1, p2);
		f.edge[1] = TessellationEdgeFactor(p2, p0);
		f.edge[2] = TessellationEdgeFactor(p0, p1);
		f.inside =
			(TessellationEdgeFactor(p1, p2) +
				TessellationEdgeFactor(p2, p0) +
				TessellationEdgeFactor(p0, p1)) * (1 / 3.0);
#endif
#if defined(TESSELLATION_BY_DISTANCE)
		float3 center = (p0 + p1 + p2) * (1.0/3.0);
		float dist = distance(center, _WorldSpaceCameraPos);
		if (dist <= TessellationMinDist) {
			f.edge[0] = TessellationUniform;
			f.edge[1] = TessellationUniform;
			f.edge[2] = TessellationUniform;
			f.inside = TessellationUniform;
		}
#endif
#if defined(TESSELLATION_UNIFORM)
		f.edge[0] = TessellationUniform;
		f.edge[1] = TessellationUniform;
		f.edge[2] = TessellationUniform;
		f.inside = TessellationUniform;
#endif
	}
	else
	{
		f.edge[0] = f.edge[1] = f.edge[2] = f.inside = 0;
	}
	return f;
}

[domain("tri")]
[outputcontrolpoints(3)]
[outputtopology("triangle_cw")]
#if defined(TESSELLATION_SHARP)
[partitioning("integer")] //fractional_odd integer
#else 
[partitioning("fractional_odd")] //fractional_odd integer
#endif
[patchconstantfunc("PatchConstantFunction")]
TessellationControlPoint HullProgram(InputPatch<TessellationControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{

	return patch[id];
}

[domain("tri")]
GenericFragmentInput DomainProgram(TessellationFactors factors, OutputPatch<TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
{
	GenericVertexInput data;

#define DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

		DOMAIN_PROGRAM_INTERPOLATE(position)
		DOMAIN_PROGRAM_INTERPOLATE(normal)
		DOMAIN_PROGRAM_INTERPOLATE(tangent)
		DOMAIN_PROGRAM_INTERPOLATE(uv)
		//DOMAIN_PROGRAM_INTERPOLATE(worldPosition)
		DOMAIN_PROGRAM_INTERPOLATE(color)

#if defined(DISPLACEMENT_PHONG)
		float3 p0 = patch[0].position.xyz;
		float3 p1 = patch[1].position.xyz;
		float3 p2 = patch[2].position.xyz;
		float3 n0 = patch[0].normal;
		float3 n1 = patch[1].normal;
		float3 n2 = patch[2].normal;
		float3 world_barycenter = data.position.xyz;
		float4 phong0 = float4(ProjectOnPlane(world_barycenter, n0, p0), 1);
		float4 phong1 = float4(ProjectOnPlane(world_barycenter, n1, p1), 1);
		float4 phong2 = float4(ProjectOnPlane(world_barycenter, n2, p2), 1);
		data.position = phong0 * barycentricCoordinates.x +
			phong1 * barycentricCoordinates.y +
			phong2 * barycentricCoordinates.z;
#endif


		return GenericVertexProgram(data);
}


void TesselApplyWorldPosition(inout TessellationControlPoint input)
{
	input.position = mul(unity_MatrixVP, float4(input.worldPosition, 1));
}

TessellationControlPoint TessellationVertexProgram(GenericVertexInput v)
{
	TessellationControlPoint p;
	p.position = v.position;
	p.normal = v.normal;
	p.tangent = v.tangent;
	p.uv = v.uv;
	p.worldPosition = mul(unity_ObjectToWorld, float4(v.position.xyz, 1)).xyz;
	p.color = v.color;
	p.localPosition = p.position.xyz;
	return p;
}

#endif