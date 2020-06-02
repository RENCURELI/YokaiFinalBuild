Shader "LOCAL/GEN/Diffuse"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)

		_Radius("Radius", float) = 5
		_Blend("Blend", float) = 1
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_Amplitude("Amplitude", float) = 1
		_Speed("Speed", float) = 1

		TessellationLimit("Tessellation Limit", Range(10, 200)) = 30
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

		float TransitionFactor(GenericFragmentInput input){
			float3 pos = input.worldPosition;
			float m = 8;
			pos.x += cos(input.worldPosition.z * m);
			pos.z += sin(input.worldPosition.x * m);
			pos.y += cos(input.worldPosition.y * m);
			float distToPlayer = distance(pos, _PlayerPosition);
			if (distToPlayer < _SpiritWorldRadius) return 1;
			return 0;
		}

		ENDHLSL
	
		Pass
		{
			Name "Human"
			Tags{"LightMode" = "HumanOpaque" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite On
			ZTest Less
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) > 0) discard;
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				return color;
			}

			ENDHLSL
        }
		Pass
		{
			Name "Spirit"
			Tags{"LightMode" = "SpiritOpaque" "Queue" = "Geometetry" "RenderType" = "Opaque"}

			ZWrite On
			ZTest LEqual
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _SpiritColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) < 1) discard;
				float4 color = tex2D(_MainTex, input.uv) * _SpiritColor * input.color.r;
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				return color;
			}

			ENDHLSL
		}
		Pass
		{
			Name "Transition"
			Tags{"LightMode" = "Transition" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite On
			ZTest Less
			Blend One Zero
			Cull Off

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

			[maxvertexcount(6)]
			void GeometryProgram(triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {

				float3 p0 = i[0].worldPosition;
				float3 p1 = i[1].worldPosition;
				float3 p2 = i[2].worldPosition;

				float3 center = (p0 + p1 + p2) / 3.0;

				float3 toObject = ObjectPosition() - center;
				float rdm = (cos(toObject.z) + sin(toObject.x) + tan(toObject.y)) * 10;
				center = lerp(lerp(p0, p1, abs(cos(rdm))), p2, abs(sin(rdm)));

				float3 normal = i[0].normal + i[1].normal + i[2].normal;
				normal = normalize(normal);

				float distToPlayer = distance(center, _PlayerPosition);

				if (distToPlayer > _SpiritWorldRadius + _Blend*2) return;
				if (distToPlayer < _SpiritWorldRadius - _Blend*2) return;

				float h = abs(distToPlayer - _SpiritWorldRadius - _Blend*2)/_Blend/2;
				float angle = h;
				if (h > 0.5) h = 1 - h;

				float3 pos = center + normal*0.2 + normal * h * _Amplitude;

				float3 toCenter = normalize(center - _PlayerPosition);
				float3 tangent = AxisCross(toCenter, normal);
				float4x4 rotation = Rotation(tangent, angle * 3.14);
				float3 dir = normalize(mul((float3x3)rotation, toCenter));

				GenerateQuad(i[0], stream, pos, float2(_Width, _Height) * (1-h*0.5), dir, normal, 0.01, i[0].uv);//normalize(-_CameraDirection)
			}

			sampler2D _MainTex;
			float4 _MainColor;
			float4 _SpiritColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float4 humanColor = tex2D(_MainTex, input.uv);
				float4 color = lerp(humanColor*_MainColor, humanColor*_SpiritColor,0.5);
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				return saturate(color);
			}

			ENDHLSL
        }
		Pass
		{
			Name "Shadow"
			Tags{"LightMode" = "Shadow" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite On
			ZTest Less
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Generic.hlsl"

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float depth = distance(input.worldPosition, _WorldSpaceCameraPos);
				return float4(depth,1,1,1);//depth;
			}

			ENDHLSL
        }
    }
}
