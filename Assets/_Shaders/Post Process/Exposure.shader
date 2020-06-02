Shader "VFX/RenderPipeline/Exposure" {
	Properties{
	}
	SubShader{
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex GetPostProcessFragmentInput
			#pragma fragment FragmentProgram
			#include "../Libraries/PostProcess.hlsl"

			float _Exposure;
			float _Contrast;
			float _Saturation;
			half _Blend;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 uv = input.uv;
				float4 pixel = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv);

				float4 p = pixel;
				// Exposure
				p *= _Exposure;
				// Contrast
				p = pow(p, _Contrast);
				// Saturation
				float4 bw = (p.r + p.g + p.b) * (1.0 / 3.0);
				p = lerp(p, bw, 1 - _Saturation);

				return lerp(pixel, p, _Blend);
			}

			ENDHLSL
		}
	}
}