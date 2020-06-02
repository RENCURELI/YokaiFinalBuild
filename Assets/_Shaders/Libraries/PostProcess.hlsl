#if !defined(POST_PROCESS_LIB)
#define POST_PROCESS_LIB

#include "Generic.hlsl"

// Types

struct PostProcessVertexInput {
	float4 position : POSITION;
	float2 uv : TEXCOORD0;
};

struct PostProcessFragmentInput {
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD0;
};

// Variables

float4 _ProjectionParams;
float4 _ZBufferParams;

TEXTURE2D(_CameraColorTexture);
SAMPLER(sampler_CameraColorTexture);
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

// Functions

GenericFragmentInput GetPostProcessFragmentInput(GenericVertexInput input) {
	GenericFragmentInput output = GenericVertexProgram(input);
	if (_ProjectionParams.x < 0) {
		output.uv.y = 1 - output.uv.y;
	}
	return output;
}

float4 GetCurrentPassColor(PostProcessFragmentInput input) : SV_TARGET{
	return SAMPLE_TEXTURE2D(
		_MainTex, sampler_MainTex, input.uv
	);
}

float4 GetCurrentPassDepth(PostProcessFragmentInput input) : SV_TARGET{
    float rawDepth = SAMPLE_DEPTH_TEXTURE(_MainTex, sampler_MainTex, input.uv);
	float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
	return float4(linearDepth / _ZBufferParams.z, rawDepth, 0, 0); // pow(sin(linearDepth*3.14), 2);
}

#endif