Shader "LOCAL/GEN/Snow"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
		_Radius("Radius", float) = 5
		_Blend("Blend", float) = 1
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_Speed("Speed", float) = 1
		TessellationLimit("Tessellation Limit", Range(10, 200)) = 30
		TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		TessellationEdgeLength("Tessellation Edge Length", Range(5, 100)) = 50
	}
		SubShader
		{

			Pass
		{
			Tags{"LightMode" = "Weather"}

				ZWrite Off
				ZTest Less
				Blend One Zero 
				Cull Off

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex TessellationVertexProgram
				#pragma fragment FragmentProgram
				#pragma hull HullProgram
				#pragma domain DomainProgram
				#pragma geometry MyGeometryProgram
				#pragma require geometry
				#pragma require tessellation tessHW

				#define TRIANGLE_ALWAYS_VISIBLE
				#define TESSELLATION_SHARP
				#define GENERIC_GEOMETRY

				#include "../Libraries/Tessellation.hlsl"
				#include "../Libraries/Lighting.hlsl"

				float _Width;
				float _Height;
				float _Radius;
				float _Blend;
				float3 _PlayerPosition;
				float _Speed;

				[maxvertexcount(6)]
				void MyGeometryProgram (triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {
				
					float3 p0 = i[0].worldPosition;
					float3 p1 = i[1].worldPosition;
					float3 p2 = i[2].worldPosition;

					float3 center = (p0 + p1 + p2)/3.0;

					float dist = distance(center*float3(1,0,1), _WorldSpaceCameraPos*float3(1, 0, 1));
					if (dist > _Radius) return;

					float h = fmod(time * _Speed + cos(center.x) + sin(center.z), 2);

					float3 pos = center - h * float3(0, 1, 0) * _Blend;

					GenerateQuad(stream, pos, float2(_Width, _Height), normalize(-_CameraDirection));
				}

				float4 _MainColor;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = _MainColor;
					if (color.a < 0.5) discard;
					return saturate(color);
				}

				ENDHLSL
        }
    }
}
