Shader "LOCAL/GEN/Footsteps"
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
			Tags{"LightMode" = "Stencil Front" "Queue"="Geometetry+10" "RenderType"="Opaque"}

			ZWrite Off
			ZTest Greater
			Cull Off
			Blend Zero One
			Stencil {
				Ref 2
				Comp always
				Pass replace
			}

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram

			#include "../Libraries/Generic.hlsl"

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				return float4(0,0,0,0);
			}

			ENDHLSL
        }
		Pass
		{
			Tags{"LightMode" = "Stencil Back" "Queue" = "Geometetry+10" "RenderType" = "Opaque"}

			ZWrite Off
			ZTest Less
			Blend One Zero
			Stencil {
				Ref 2
				Comp equal
				Pass replace
			}

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram

			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				input.normal *= -1;
				float3 light = GetLightingDiffuse(input);
				color.rgb = lerp(color.rgb, color.rgb*light, 0.5);
				return color;
			}

			ENDHLSL
		}
		Pass
		{
			Tags{"LightMode" = "Stencil Clear" "Queue" = "Geometetry+10" "RenderType" = "Opaque"}

			ZWrite Off
			ZTest Always
			Blend Zero One
			Stencil {
				Ref 0
				Comp always
				Pass replace
			}

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram

			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				return float4(0,0,0,0);
			}

			ENDHLSL
		}
    }
}
