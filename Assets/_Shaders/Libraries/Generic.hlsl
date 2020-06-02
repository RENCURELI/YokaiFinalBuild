#if !defined(GENERIC_LIB)
#define GENERIC_LIB

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

// Types

struct GenericVertexInput {
	float4 position : POSITION;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
	float2 uv : TEXCOORD0;
	float3 color : COLOR;
};

struct GenericFragmentInput {
	float4 position : POSITION;
	float2 uv : TEXCOORD0;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float3 color : COLOR;
	float3 worldPosition : TEXCOORD1;
	float3 localPosition : TEXCOORD2;
};

// Variables

CBUFFER_START(UnityPerFrame)
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float time;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float3 _WorldSpaceCameraPos;
float3 _CameraDirection;
CBUFFER_END

#if defined(GENERIC_DEPTH_BUFFER)
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float4 _ZBufferParams;
#endif

// Functions

float rand(float3 i) {
	return fmod(sin(dot(i.xyz, float3(12.9898, 78.233, 58.124))) * 43758.5453, 1);
}

GenericFragmentInput GenericVertexProgram(GenericVertexInput input) {
	GenericFragmentInput output;
	output.worldPosition = mul(unity_ObjectToWorld, float4(input.position.xyz, 1)).xyz;
	output.normal = normalize(mul((float3x3)unity_ObjectToWorld, input.normal));
	output.position = mul(unity_MatrixVP, float4(output.worldPosition,1));
	output.tangent = mul(unity_ObjectToWorld, input.tangent);
	output.color = input.color;
	output.uv = input.uv;
	output.localPosition = input.position.xyz;
	return output;
}

half3 BlendNormals(half3 n1, half3 n2) { // Same as Unity's generic function
	return normalize(half3(n1.xy + n2.xy, n1.z * n2.z));
}

half3 UnpackScaleNormal(half4 packednormal, half bumpScale) { // Same as Unity's generic function
#if defined(UNITY_NO_DXT5nm)
	return packednormal.xyz * 2 - 1;
#else
	half3 normal;
	normal.xy = (packednormal.wy * 2 - 1);
#if (SHADER_TARGET >= 30)
	// SM2.0: instruction count limitation
	// SM2.0: normal scaler is not supported
	normal.xy *= bumpScale;
#endif
	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
	return normal;
#endif
}

float3 GetBinormal(float3 normal, float4 tangent) {
	return normalize(cross(normal, tangent.xyz) * tangent.w);
}

float3 ConvertTangentNormal(float3 tangentNormal, float3 normal, float4 tangent) {
	float3 binormal = GetBinormal(normal, tangent);

	float3 result = float3(
		tangentNormal.x * normalize(tangent.xyz) +
		tangentNormal.y * binormal + 
		tangentNormal.z * normalize(normal)
		);

	return normalize(result);
}

void ApplyNormalmap(float3 normalmap, inout GenericFragmentInput input) {
	float3 n = ConvertTangentNormal(normalmap, input.normal, input.tangent);
	input.normal = (n + input.normal)*0.5;
}

float3 ProjectOnPlane(float3 pos, float3 normal, float3 origin)
{
	float3 op = pos - origin;
	if (length(op) < 0.01)
	{
		return pos;
	}
	float cosine = dot(normalize(op), normalize(normal));
	float dist = cosine * length(op);
	float3 pos_on_plane = pos - normalize(normal) * dist;
	return pos_on_plane;
}

float3 ProjectOnNormal(float3 pos, float3 normal, float3 origin) {
	float3 op = pos - origin;
	float cosine = dot(normalize(op), normalize(normal));
	float dist = cosine * length(op);
	return normalize(normal) * dist;
}

bool TriangleIsBelowClipPlane(float3 p0, float3 p1, float3 p2, int planeIndex)
{
	return false;
	/*
	float4 plane = unity_CameraWorldClipPlanes[planeIndex];
	return
		dot(float4(p0, 1), plane) < 0 &&
		dot(float4(p1, 1), plane) < 0 &&
		dot(float4(p2, 1), plane) < 0;*/
}

bool WorldTriangleVisible(float3 p0, float3 p1, float3 p2)
{
	return !(TriangleIsBelowClipPlane(p0, p1, p2, 0) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 1) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 2) ||
		TriangleIsBelowClipPlane(p0, p1, p2, 3)
		);

}

float3 ViewDirection(GenericFragmentInput input)
{
    return normalize(input.worldPosition - _WorldSpaceCameraPos);
}

float3 ViewDirection(float3 position)
{
    return normalize(position - _WorldSpaceCameraPos);
}

float3 ObjectDirection(float3 direction)
{
    float3 worldfwd = mul((float3x3) unity_ObjectToWorld, direction);
    return normalize(worldfwd);
}

float3 ObjectDirection()
{
    float3 fwd = float3(0, 0, 1);
    return ObjectDirection(fwd);
}

void ApplyWorldPosition(inout GenericFragmentInput input)
{
    input.position = mul(unity_MatrixVP, float4(input.worldPosition, 1));
}

float3 ObjectPosition() {
	return mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
}

float Angle(float3 v1, float3 v2) {
	return acos(dot(normalize(v1), normalize(v2)));
}

float4x4 Rotation(float3 axis, float angle)
{
	axis = normalize(axis);
	float s = sin(angle);
	float c = cos(angle);
	float oc = 1.0 - c;

	return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
		oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
		oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
		0.0, 0.0, 0.0, 1.0);
}

float3 AxisCross(float3 a, float3 b) {
	return normalize(cross(normalize(a), normalize(b)));
}

#if defined(GENERIC_GEOMETRY)
void GenerateQuad(GenericFragmentInput reference, inout TriangleStream<GenericFragmentInput> stream, float3 center, float2 size, float3 view, float3 normal) {
	GenericFragmentInput i = reference;
	float3 up = AxisCross(view, normal);
	//if (abs(dot(normalize(view), normalize(normal))) < 0.1) up = float3(0, 1, 0);
	float3 x = up * size.x * 0.5;// AxisCross(view, up) * size.x * 0.5;
	float3 y = AxisCross(view, x) * size.y * 0.5;
	i.normal = normal;

	float2 uvCenter = float2(0,0);
	float uvScale = 1;

	i.worldPosition = center + x - y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x - y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x + y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 1) * uvScale + uvCenter;
	stream.Append(i);

	stream.RestartStrip();

	i.worldPosition = center + x + y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 1) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center + x - y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x + y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 1) * uvScale + uvCenter;
	stream.Append(i);

	stream.RestartStrip();
}

void GenerateQuad(GenericFragmentInput reference, inout TriangleStream<GenericFragmentInput> stream, float3 center, float2 size, float3 view, float3 normal, float2 uvScale, float2 uvCenter) {
	GenericFragmentInput i = reference;
	float3 up = AxisCross(view, normal);
	//if (abs(dot(normalize(view), normalize(normal))) < 0.1) up = float3(0, 1, 0);
	float3 x = up * size.x * 0.5;// AxisCross(view, up) * size.x * 0.5;
	float3 y = AxisCross(view, x) * size.y * 0.5;
	i.normal = normal;

	i.worldPosition = center + x - y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x - y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x + y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 1) * uvScale + uvCenter;
	stream.Append(i);

	stream.RestartStrip();

	i.worldPosition = center + x + y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 1) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center + x - y;
	ApplyWorldPosition(i);
	i.uv = float2(1, 0) * uvScale + uvCenter;
	stream.Append(i);

	i.worldPosition = center - x + y;
	ApplyWorldPosition(i);
	i.uv = float2(0, 1) * uvScale + uvCenter;
	stream.Append(i);

	stream.RestartStrip();
}
#endif

#endif