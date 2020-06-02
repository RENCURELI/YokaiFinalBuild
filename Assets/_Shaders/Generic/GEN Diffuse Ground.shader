Shader "LOCAL/GEN/Diffuse Mix"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("1st Color", color) = (1,1,1,1)
		_2ndColor("2nd Color", color) = (1,1,1,1)
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "Static" "Queue"="Geometetry" "RenderType"="Opaque"}

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
				float4 _2ndColor;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = _MainColor;
					color.rgb = lerp(color.rgb, _2ndColor.rgb, input.color.r);
					float3 light = GetLightingDiffuse(input);
					color.rgb *= light;
					return color;
				}

				ENDHLSL
        }
    }
}
