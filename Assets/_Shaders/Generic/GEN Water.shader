Shader "LOCAL/GEN/Water"
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
					float3 localPos = input.worldPosition - ObjectPosition();
					float3 x = ObjectDirection(float3(1, 0, 0));
					float3 z = ObjectDirection(float3(0, 0, 1));
					x = ProjectOnNormal(localPos, x, float3(0, 0, 0)) * _Strength;
					z = ProjectOnNormal(localPos, z, float3(0, 0, 0)) * _Strength;
					float factor = sin(cos(length(x)) + cos(length(z)));
					factor *= 0.5;
					factor += 0.5;
					if (factor > _Width) color = _2ndColor;
					float3 light = GetLightingDiffuse(input);
					color.rgb *= light;
					return color;
				}

				ENDHLSL
        }
    }
}
