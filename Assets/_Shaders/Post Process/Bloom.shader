Shader "VFX/RenderPipeline/Bloom" {
	SubShader {
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex GetPostProcessFragmentInput
			#pragma fragment FragmentProgram
			#include "../Libraries/PostProcess.hlsl"

			float _BlurQuality; // Strips
			float _BlurLayers; // Distance
			float _BlurStrength; // Distance
			half _BlurBlend;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 uv = input.uv;
				float4 pixel = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv);
				float angle = 2 * 3.14 / _BlurQuality;
				float strength = 1.0 / (_BlurQuality + _BlurLayers + 1);
				float layer = 1.0 / _BlurLayers;
				float4 blurredPixel = float4(0, 0, 0, 1);
				for (int i = 0; i < _BlurQuality; i++)
				{
					for (int l = 0; l < _BlurLayers; l++)
					{
						float2 dir = normalize(float2(cos(angle * i), sin(angle * i)));
						float2 blurUV = uv + dir * _BlurStrength * _BlurBlend * 0.001 * layer * l;
						float4 otherPixel =SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, blurUV);
						float lumin = 1.0 / 3.0 * (otherPixel.r, max(otherPixel.g, otherPixel.b));
						blurredPixel += lerp(0, pow(otherPixel,2) * strength * 2, lumin);
					}
				}
				return pixel + blurredPixel * _BlurBlend;
			}

			ENDHLSL
		}
	}
}