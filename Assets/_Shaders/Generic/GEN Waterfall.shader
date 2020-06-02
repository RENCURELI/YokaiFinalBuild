Shader "LOCAL/GEN/Waterfall"
{
    Properties
    {
		_MainColor("Color", color) = (1,1,1,1)
		_2ndColor("2nd Color", color) = (1,1,1,1)
		_Speed("Speed", float) = 1
		_Width("Width", range(0,1)) = 0.2
		_Strength("Strength", float) = 1
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "Static" "Queue"="Geometetry" "RenderType"="Opaque"}

				ZWrite On
				ZTest Less
				Blend One Zero

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex GenericVertexProgram
				#pragma fragment FragmentProgram
				#include "../Libraries/Lighting.hlsl"
		
				float4 _MainColor;
				float4 _2ndColor;
				float _Speed;
				float _Width;
				float _Strength;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = _MainColor;
					float factor = sin(input.worldPosition.y * _Strength + cos(input.worldPosition.x*_Strength)*_Strength*0.1 +cos(input.worldPosition.z*_Strength)*_Strength*0.1 + time * _Speed);
					factor *= 0.5;
					factor += 0.5;
					if (factor <= _Width) color = _2ndColor;
					float3 light = GetLightingDiffuse(input);
					color.rgb *= light;
					return color;
				}

				ENDHLSL
        }
    }
}
