Shader "LOCAL/Encense Pot"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_EmitTex("Emission Map", 2D) = "black" {}
		_EmitColor("Emission Color", color) = (1,1,1,1)
		_EmitIntensity("Emission Intensity", range(0,2)) = 1
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)

		_BumpTex("Bump", 2D) = "bump" {}
		_BumpScale("Bump Scale", float) = 1

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
		Pass
		{
			Name "Opaque"
			Tags{"LightMode" = "Opaque" "Queue"="Geometetry+20" "RenderType"="Opaque"}

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

			sampler2D _EmitTex;
			float4 _EmitColor;
			float _EmitIntensity;

			sampler2D _BumpTex;
			float _BumpScale;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				float3 normalmap = UnpackScaleNormal(tex2D(_BumpTex, input.uv), _BumpScale);
				ApplyNormalmap(normalmap, input);
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				float4 emit = tex2D(_EmitTex, input.uv) * _EmitColor * _EmitIntensity;
				return color + emit;
			}

			ENDHLSL
        }
		UsePass "LOCAL/GEN/Diffuse/Transition"
		UsePass "LOCAL/GEN/Diffuse/Shadow"
    }
}
