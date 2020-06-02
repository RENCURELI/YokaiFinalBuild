Shader "VFX/RenderPipeline/FarClipWave" {
	Properties{
		_Speed("Speed", float) = 1
		_Amplitude("Amplitude", float) = 1
		_Frequency("Frequency", float) = 1
		_Start("Start", float) = 20
		_End("End", float) = 50
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

			float _Speed;
			float _Amplitude;
			float _Frequency;
			float _Start;
			float _End;
			half _Blend;

			sampler2D _DepthTex;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 uv = input.uv;
				float4 pixel = SAMPLE_TEXTURE2D( _MainTex, sampler_MainTex, uv);
				float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv);
				float linearDepth = _ZBufferParams.z * LinearEyeDepth(rawDepth, _ZBufferParams);
				if (linearDepth < _Start) return pixel;
				float strength = min(1, (linearDepth - _Start) / (_End - _Start));
				uv.x += cos(uv.y * _Frequency + time * _Speed) * _Amplitude * 0.01 * strength;
				rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
				linearDepth = _ZBufferParams.z * LinearEyeDepth(rawDepth, _ZBufferParams);
				if (linearDepth < _Start) return pixel;
				float4 otherPixel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
				return lerp(pixel, otherPixel, _Blend);
			}

			ENDHLSL
		}
	}
}