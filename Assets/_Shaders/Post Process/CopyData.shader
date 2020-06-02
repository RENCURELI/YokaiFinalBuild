Shader "VFX/RenderPipeline/CopyData" {
	SubShader {
		Pass {
			Cull Off
			ZTest Always
			ZWrite Off
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex GetPostProcessFragmentInput
			#pragma fragment GetCurrentPassColor
			#include "../Libraries/PostProcess.hlsl"
			ENDHLSL
		}
	}
}