Shader "VFX/RenderPipeline/AmbientOcclusion" {
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

			sampler2D _DepthTex;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 uv = input.uv;
				float4 pixel = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv);
				float depth = tex2D(_DepthTex, uv).r;
				float angle = 2 * 3.14 / _BlurQuality;
				float strength = 1.0 / (_BlurQuality + _BlurLayers + 1);
				float layer = 1.0 / _BlurLayers;
				float4 occludedPixel = pixel;
				for (int i = 0; i < _BlurQuality; i++)
				{
					for (int l = 0; l < _BlurLayers; l++)
					{
						float2 dir = normalize(float2(cos(angle * i), sin(angle * i)));
						float2 blurUV = uv + dir * _BlurStrength * 0.001 * layer * l;
						float otherDepth = tex2D(_DepthTex, blurUV).r;
						if (otherDepth > depth){
							float depthDiff = min(abs(depth - otherDepth) * 100,1);
							occludedPixel -= occludedPixel * strength * depthDiff;
						}
						
					}
				}
				return lerp(pixel, saturate(occludedPixel), _BlurBlend);
			}

			ENDHLSL
		}
	}
}