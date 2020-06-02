Shader "LOCAL/PBS/All Worlds/Opaque"
{
    Properties
    {
		_WorldMapScale("World Map Scale", float) = 0

		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_NormalTex("Normal", 2D) = "bump" {}
		_NormalFactor("Normal Factor", float) = 1
		_MaskTex("Mask", 2D) = "white" {}

		_SpiritTex("Spirit Texture", 2D) = "white" {}
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)
		_SpiritNormalTex("Spirit Normal", 2D) = "bump" {}
		_SpiritMaskTex("Spirit Mask", 2D) = "white" {}
		
		_Smoothness("Smoothness", range(0.001, 1.2)) = 0.5
		_Metallic("Metallic", range(0, 1)) = 0
		_Reflectivity("Reflectivity", range(0, 1)) = 0
	}
	SubShader
	{ 
		HLSLINCLUDE
		#define GENERIC_GEOMETRY
		#include "../Libraries/Generic.hlsl"

		float _SpiritWorldForce;
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

		float _WorldMapScale;

		float2 GetUVCorrds(float2 uv, float3 normal, float3 position) {
			if (_WorldMapScale == 0) return uv;
			float3 w = float3(dot(normal, float3(1, 0, 0)), dot(normal, float3(0, 1, 0)), dot(normal, float3(0, 0, 1)));
			uv = position.zy * w.x + position.xz * w.y + position.xy * w.z;
			uv *= _WorldMapScale;
			return uv;
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
			#define LIT_WORLD_HUMAN
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			sampler2D _NormalTex;
			float _NormalFactor;

			sampler2D _MaskTex;
			float _Smoothness;
			float _Metallic;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) > 0 && _SpiritWorldForce < 1) discard;

				float2 coords = GetUVCorrds( input.uv, input.normal, input.worldPosition);

				float4 albedo = tex2D(_MainTex, coords) * _MainColor;

				float4 mask = tex2D(_MaskTex, coords);
				float smoothness = mask.g * _Smoothness;
				float metallic = mask.r * _Metallic;

				float3 normal = UnpackScaleNormal(tex2D(_NormalTex, coords), _NormalFactor);
				ApplyNormalmap(normal, input);

				float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
				albedo.rgb *= light;
				return albedo;
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
			#define LIT_WORLD_SPIRIT
			#include "../Libraries/Lighting.hlsl"

			sampler2D _SpiritTex;
			float4 _SpiritColor;

			sampler2D _SpiritNormalTex;
			float _NormalFactor;

			sampler2D _SpiritMaskTex;
			float _Smoothness;
			float _Metallic;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) < 1 && _SpiritWorldForce < 1) discard;

				float2 coords = GetUVCorrds(input.uv, input.normal, input.worldPosition);

				float4 albedo = tex2D(_SpiritTex, coords) * _SpiritColor;

				float4 mask = tex2D(_SpiritMaskTex, coords);
				float smoothness = mask.g * _Smoothness;
				float metallic = mask.r * _Metallic;

				float3 normal = UnpackScaleNormal(tex2D(_SpiritNormalTex, coords), _NormalFactor);
				ApplyNormalmap(normal, input);

				float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
				albedo.rgb *= light;
				return albedo;
			}

			ENDHLSL
		}
		Pass
		{
			Name "Transition"
			Tags{"LightMode" = "Transition" "Queue" = "Geometetry" "RenderType" = "Opaque"}

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
			//#define TESSELLATION_BY_DISTANCE
			#define TESSELLATION_EDGE
			#define TESSEL_EDGE_LENGTH 1
			#define TESSEL_MAX_DIST 40

			#define _Blend 1

			#include "../Libraries/Tessellation.hlsl"
			#include "../Libraries/Lighting.hlsl"

			void GenerateQuadRotated(GenericFragmentInput reference, inout TriangleStream<GenericFragmentInput> stream, float3 center, float2 size, float3 view, float3 normal, float2 uvScale, float2 uvCenter) {
				GenericFragmentInput i = reference;
				float3 up = AxisCross(view, normal);
				//if (abs(dot(normalize(view), normalize(normal))) < 0.1) up = float3(0, 1, 0);
				float3 x = up * size.x * 0.5;// AxisCross(view, up) * size.x * 0.5;
				float3 y = -AxisCross(view, x) * size.y;
				i.normal = normal;

				i.worldPosition = center + x;
				ApplyWorldPosition(i);
				i.uv = float2(1, 0) * uvScale + uvCenter;
				stream.Append(i);

				i.worldPosition = center - x;
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

				i.worldPosition = center + x;
				ApplyWorldPosition(i);
				i.uv = float2(1, 0) * uvScale + uvCenter;
				stream.Append(i);

				i.worldPosition = center - x + y;
				ApplyWorldPosition(i);
				i.uv = float2(0, 1) * uvScale + uvCenter;
				stream.Append(i);

				stream.RestartStrip();
			}

			[maxvertexcount(6)]
			void GeometryProgram(triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {

				float3 p0 = i[0].worldPosition;
				float3 p1 = i[1].worldPosition;
				float3 p2 = i[2].worldPosition;

				float size = min(min(distance(p0, p1), distance(p0, p2)), 0.8);
				size *= 0.5;

				float3 center = (p0 + p1 + p2) / 3.0;

				float3 toObject = ObjectPosition() - center;
				float rdm = (cos(toObject.z) + sin(toObject.x) + tan(toObject.y)) * 10;
				center = lerp(lerp(p0, p1, abs(cos(rdm))), p2, abs(sin(rdm)));

				float3 normal = i[0].normal + i[1].normal + i[2].normal;
				normal = normalize(normal);

				float distToPlayer = distance(center, _PlayerPosition);

				if (distToPlayer < 3) return;

				if (distToPlayer > _SpiritWorldRadius + _Blend * 2) return;
				if (distToPlayer < _SpiritWorldRadius - _Blend * 2) return;

				float h = abs(distToPlayer - _SpiritWorldRadius - _Blend * 2) / _Blend / 2;
				float angle = h*0.5 + 0.5;
				if (h > 0.5) h = 1 - h;

				float3 pos = center;

				float3 toCenter = normalize(center - _PlayerPosition);
				float3 tangent = AxisCross(toCenter, normal);
				float4x4 rotation = Rotation(tangent, -angle * 3.14);
				float3 dir = normalize(mul((float3x3)rotation, toCenter));

				GenerateQuadRotated(i[0], stream, pos, float2(1, 1) * size, dir, normal, 0.01, i[0].uv);
			}

			sampler2D _MainTex;
			float4 _MainColor;
			float4 _SpiritColor;
			float _Smoothness;
			float _Metallic;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{

				float2 coords = GetUVCorrds(input.uv, input.normal, input.worldPosition);

				float4 humanColor = tex2D(_MainTex, coords);
				float4 color = lerp(humanColor*_MainColor, humanColor*_SpiritColor,0.5);
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				return saturate(color);
			}

			ENDHLSL
		}
		UsePass "LOCAL/GEN/Diffuse/Shadow"
    }
}
