Shader "LOCAL/GEN/View Light Projection"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
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

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
					float3 light = GetLightingProjection(input);
					color.rgb *= light * GetLightingDiffuse(input); 
					return color;
				}

				ENDHLSL
        }
    }
}
