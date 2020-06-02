Shader "LOCAL/VFX/Nemesis Body"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)

		_Radius("Radius", float) = 5
		_Blend("Blend", float) = 1
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_Amplitude("Amplitude", float) = 1
		_Speed("Speed", float) = 1
		_Spread("Spread", float) = 2

		TessellationMinDist("Tessellation Limit", Range(10, 200)) = 30
		TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		TessellationEdgeLength("Tessellation Edge Length", Range(5, 100)) = 50
	}
	SubShader
	{
		HLSLINCLUDE
		#define GENERIC_GEOMETRY
		#include "../Libraries/Generic.hlsl"

		float _SpiritWorldRadius;
		float3 _PlayerPosition;

		ENDHLSL

		Pass
		{
			Name "Spirit"
			Tags{"LightMode" = "Transparent" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite Off
			ZTest Less
			Blend One One
			Cull Front

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex TessellationVertexProgram
			#pragma fragment FragmentProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma geometry GeometryProgram
			#pragma require geometry
			#pragma require tessellation tessHW

			#define TESSELLATION_SHARP
			#define TESSELLATION_EDGE

			#include "../Libraries/Tessellation.hlsl"
			#include "../Libraries/Lighting.hlsl"

			float _Width;
			float _Height;
			float _Blend;
			float _Amplitude;
			float _Speed;
			float _Spread;

			[maxvertexcount(6)]
			void GeometryProgram(triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {

				float3 p0 = i[0].worldPosition;
				float3 p1 = i[1].worldPosition;
				float3 p2 = i[2].worldPosition;

				float3 center = (p0 + p1 + p2) / 3.0;
				float3 normal = i[0].normal + i[1].normal + i[2].normal;
				normal = normalize(normal);

				float distToPlayer = distance(center, _PlayerPosition);

				float3 toObjCenter = center - ObjectPosition();
				float rdm = (cos(toObjCenter.x) + sin(toObjCenter.z) + tan(toObjCenter.y))*10;

				float lifetime = 1 - fmod(time * _Speed + rdm, 2);
				i[0].color.r = lifetime;

				float3 pos = center + float3(0,1,0) * (1-lifetime) * _Amplitude + normal * _Spread;

				float3 dir = normalize(-_CameraDirection);

				GenerateQuad(i[0], stream, pos, float2(_Width, _Height) * lifetime, dir, float3(0,1,0));//normalize(-_CameraDirection)
			}

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 coords = float2(input.uv.x, 1 - input.uv.y);
				float4 color = tex2D(_MainTex, coords) * _MainColor * input.color.r;
				return saturate(color);
			}

			ENDHLSL
        }
    }
}
