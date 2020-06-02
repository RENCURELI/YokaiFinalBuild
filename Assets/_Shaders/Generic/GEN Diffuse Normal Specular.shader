Shader "LOCAL/GEN/Diffuse Normal Specular"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MaskTex("Mask", 2D) = "black" {}
		_BumpTex("Bump", 2D) = "bump" {}
		_BumpScale("Bump Scale", float) = 1
		_MainColor("Color", color) = (1,1,1,1)
		_Smoothness("Smoothness", range(0.001,1.2)) = 0.5
		_Metallic("Metallic", range(0,1)) = 0
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "Opaque" "Queue"="Geometetry" "RenderType"="Opaque"}

				ZWrite On
				ZTest Less
				Blend One Zero

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex GenericVertexProgram
				#pragma fragment FragmentProgram
				#include "../Libraries/Lighting.hlsl"

				sampler2D _MainTex;
				sampler2D _MaskTex;
				sampler2D _BumpTex;
				float _BumpScale;
				float4 _MainColor;
				float _Smoothness;
				float _Metallic;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{ 
					float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
					float4 mask = tex2D(_MaskTex, input.uv);
					float3 normalmap = UnpackScaleNormal(tex2D(_BumpTex, input.uv), _BumpScale);
					ApplyNormalmap(normalmap, input);
					float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
					color.rgb *= light;
					return color;
				}

				ENDHLSL
        }
    }
}
