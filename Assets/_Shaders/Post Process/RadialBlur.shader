Shader "VFX/RenderPipeline/RadialBlur" {
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
			float _Blend;

			#define IRIS 0.25
			#define FADE 1.0

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 uv = input.uv;
				float4 pixel = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv);
				float2 screenCenter = float2(0.5, 0.5);
				float2 toCenter = screenCenter - uv;
				//if (length(toCenter) < 0.5) return pixel;
				float fade = saturate((length(toCenter) - IRIS) / FADE);
				float strength = 1.0 / (_BlurQuality + _BlurLayers + 1);
				float layer = 1.0 / _BlurLayers;
				float4 blurredPixel =  float4(0,0,0,1);
				for (int i = 0; i < _BlurQuality; i++)
				{
					for (int l = 0; l < _BlurLayers; l++)
					{
						float2 dir = -toCenter * fade;
						float2 blurUV = uv + dir * _BlurStrength * _Blend * 0.001 * (layer * l + i * 0.1);
						float4 otherPixel =SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, blurUV);
						blurredPixel += otherPixel * strength;
					}
				}
				return lerp(pixel, blurredPixel, _Blend);
			}

			ENDHLSL
		}
	}
}