Shader "LOCAL/PBS/All Worlds/Water"
{
    Properties
    {
		_WorldMapScale("World Map Scale", float) = 0

		_FogDistance("FogDistance", float) = 10

		_WaterFlowTex("Water Flow", 2D) = "black" {}
		_WaterFlow("Water Flow", range(0,1)) = 0.1
		_WaterSpeed("Water Speed", float) = 1

		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)

		_NormalTex("Normal", 2D) = "bump" {}
		_NormalFactor("Normal Factor", float) = 1
		_MaskTex("Mask", 2D) = "white" {}
		_Smoothness("Smoothness", range(0.001, 1.2)) = 0.5
		_Metallic("Metallic", range(0, 1)) = 0
		_Reflectivity("Reflectivity", range(0, 1)) = 0

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
		Tags { "Queue"="Geometry-10"}
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

		float _FogDistance;

		float FogFactor(float3 position) {
			position.y = 0;
			float3 camPos = _WorldSpaceCameraPos * float3(1, 0, 1);
			float dist = distance(position, camPos);
			if (dist >= _FogDistance) return 0;
			return 1-dist / _FogDistance;
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
			Tags{"LightMode" = "HumanTransparent"}

			ZWrite Off
			ZTest Less
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			sampler2D _NormalTex;
			float _NormalFactor;

			sampler2D _MaskTex;
			float _Smoothness;
			float _Metallic;

			sampler2D _WaterFlowTex;
			float _WaterSpeed;
			float _WaterFlow;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) > 0) discard;

				float2 coords = GetUVCorrds( input.uv, input.normal, input.worldPosition);

				float4 albedo = tex2D(_MainTex, coords) * _MainColor;

				float4 mask = tex2D(_MaskTex, coords);
				float smoothness = mask.g * _Smoothness;
				float metallic = mask.r * _Metallic;

				float4 flow = tex2D(_WaterFlowTex, coords);
				flow = lerp(tex2D(_WaterFlowTex, coords * float2(-1,1) + float2(0.5,0.5)), flow, abs(cos(time*0.1)));
				flow = lerp(flow, float4(0.8, 0.2, 0.3, 0.7), _WaterFlow);

				float2 waterCoords = coords;
				waterCoords.x += time * _WaterSpeed * flow.x;
				waterCoords.y += time * _WaterSpeed * flow.y;
				float3 normal1 = ConvertTangentNormal(UnpackScaleNormal(tex2D(_NormalTex, waterCoords), _NormalFactor), input.normal, input.tangent);

				waterCoords = coords * 0.5;
				waterCoords.x -= time * _WaterSpeed * flow.z;
				waterCoords.y += time * _WaterSpeed * flow.w;
				float3 normal2 = ConvertTangentNormal(UnpackScaleNormal(tex2D(_NormalTex, waterCoords), _NormalFactor), input.normal, input.tangent);

				float3 mixedNormal = BlendNormals(normal1, normal2);
				input.normal = normalize((mixedNormal + input.normal));

				float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
				albedo.rgb *= light;
				albedo *= FogFactor(input.worldPosition);
				return albedo;
			}

			ENDHLSL
        }
		Pass
		{
			Name "Spirit"
			Tags{"LightMode" = "SpiritTransparent" "Queue" = "Geometetry" "RenderType" = "Opaque"}

			ZWrite Off
			ZTest LEqual
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _SpiritColor;

			sampler2D _NormalTex;
			float _NormalFactor;

			sampler2D _MaskTex;
			float _Smoothness;
			float _Metallic;

			sampler2D _WaterFlowTex;
			float _WaterSpeed;
			float _WaterFlow;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (TransitionFactor(input) < 1) discard;

				float2 coords = GetUVCorrds(input.uv, input.normal, input.worldPosition);

				float4 albedo = tex2D(_MainTex, coords) * _SpiritColor;

				float4 mask = tex2D(_MaskTex, coords);
				float smoothness = mask.g * _Smoothness;
				float metallic = mask.r * _Metallic;

				float4 flow = tex2D(_WaterFlowTex, coords);
				flow = lerp(tex2D(_WaterFlowTex, coords * float2(-1, 1) + float2(0.5, 0.5)), flow, abs(cos(time*0.1)));
				flow = lerp(flow, float4(0.8, 0.2, 0.3, 0.7), _WaterFlow);

				float2 waterCoords = coords;
				waterCoords.x += time * _WaterSpeed * flow.x;
				waterCoords.y += time * _WaterSpeed * flow.y;
				float3 normal1 = ConvertTangentNormal(UnpackScaleNormal(tex2D(_NormalTex, waterCoords), _NormalFactor), input.normal, input.tangent);

				waterCoords = coords * 0.5;
				waterCoords.x -= time * _WaterSpeed * flow.z;
				waterCoords.y += time * _WaterSpeed * flow.w;
				float3 normal2 = ConvertTangentNormal(UnpackScaleNormal(tex2D(_NormalTex, waterCoords), _NormalFactor), input.normal, input.tangent);

				float3 mixedNormal = BlendNormals(normal1, normal2);
				input.normal = normalize((mixedNormal + input.normal));

				float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
				albedo.rgb *= light;
				albedo *= FogFactor(input.worldPosition);
				return albedo;
			}

			ENDHLSL
		}
    }
}
