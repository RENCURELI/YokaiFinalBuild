Shader "LOCAL/GEN/Plant"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_AlphaTex("Alpha", 2D) = "white" {}
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
			Cull Off

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				if (tex2D(_AlphaTex, input.uv).r < 0.5) discard;
				float3 light = GetLighting(input);
				color.rgb *= light;
				return color;
			}

			ENDHLSL
        }
    }
}
