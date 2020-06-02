Shader "LOCAL/PBS/All Worlds/Transparent"
{
    Properties
    {
		_WorldMapScale("World Map Scale", float) = 0

		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)

		_Opacity("Opacity", range(0,1)) = 1

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
		Tags{"Queue" = "Transparent+80" "RenderType" = "Transparent"}
		HLSLINCLUDE
		#define GENERIC_GEOMETRY
		#include "../Libraries/Generic.hlsl"

		ENDHLSL
	
		Pass
		{
			Name "Main"
			Tags{"LightMode" = "Transparent"}

			ZWrite Off
			ZTest Less
			Blend SrcAlpha OneMinusSrcAlpha

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

			float _Opacity;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 coords = input.uv;

				float4 albedo = tex2D(_MainTex, coords) * _MainColor;

				float4 mask = tex2D(_MaskTex, coords);
				float smoothness = mask.g * _Smoothness;
				float metallic = mask.r * _Metallic;

				float3 normal = UnpackScaleNormal(tex2D(_NormalTex, coords), _NormalFactor);
				ApplyNormalmap(normal, input);

				float3 light = GetLighting(input.worldPosition, input.normal, albedo.rgb, smoothness, metallic);
				albedo.rgb *= light;
				albedo.a *= _Opacity;
				return albedo;
			}

			ENDHLSL
        }
    }
}
