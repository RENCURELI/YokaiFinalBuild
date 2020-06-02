Shader "LOCAL/PBS/All Worlds/Emit"
{
	Properties
	{
		_WorldMapScale("World Map Scale", float) = 0

		_EmitTex("Emit", 2D) = "black" {}
		_EmitColor("Emit Color", color) = (1,1,1,1)
		_EmitFactor("Emit Factor", float) = 1
		_EmitFlicker("Emit Flicker", range(0,1)) = 0.2

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

			float TransitionFactor(GenericFragmentInput input) {
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
				Tags{"LightMode" = "HumanOpaque" "Queue" = "Geometetry" "RenderType" = "Opaque"}

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

				sampler2D _EmitTex;
				float4 _EmitColor;
				float _EmitFactor;
				float _EmitFlicker;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					if (TransitionFactor(input) > 0 && _SpiritWorldForce < 1) discard;

					float2 coords = GetUVCorrds(input.uv, input.normal, input.worldPosition);

					float4 albedo = tex2D(_MainTex, coords) * _MainColor;

					float4 mask = tex2D(_MaskTex, coords);
					float smoothness = mask.g * _Smoothness;
					float metallic = mask.r * _Metallic;

					float3 normal = UnpackScaleNormal(tex2D(_NormalTex, coords), _NormalFactor);
					ApplyNormalmap(normal, input);

					float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
					albedo.rgb *= light;

					float emitflicker = (ObjectPosition().x + ObjectPosition().z) * 100;
					float4 emit = tex2D(_EmitTex, input.uv) * _EmitColor * _EmitFactor * ( 1 - _EmitFlicker * abs(cos(time+ emitflicker)));

					return albedo + emit;
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

				sampler2D _EmitTex;
				float4 _EmitColor;
				float _EmitFactor;
				float _EmitFlicker;

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

					float emitflicker = (ObjectPosition().x + ObjectPosition().z) * 100;
					float4 emit = tex2D(_EmitTex, input.uv) * _EmitColor * _EmitFactor * (1 - _EmitFlicker * abs(cos(time + emitflicker)));

					return albedo + emit;
				}

				ENDHLSL
			}
		}
}
