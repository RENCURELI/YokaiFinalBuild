Shader "LOCAL/Gizmo/Object"
{
    Properties
    {
		_MainColor("Color", color) = (1,1,1,1)
		_RotateSpeed("Speed", float) = 10
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "StaticFX" "Queue"="Transparent+2" "RenderType"="Opaque"}

				ZWrite On
				ZTest Less
				Blend One Zero

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram
				#include "../Libraries/Lighting.hlsl"

				float _RotateSpeed;

				GenericFragmentInput VertexProgram(GenericVertexInput input){
					GenericFragmentInput output = GenericVertexProgram(input);
					float3 localPos = output.worldPosition - ObjectPosition();
					float4x4 rotation = Rotation(float3(0, 1, 0), time*_RotateSpeed);
					localPos = mul(rotation, float4(localPos, 0)).xyz;
					output.worldPosition = localPos + ObjectPosition();
					output.normal = mul(rotation, float4(output.normal, 0)).xyz;
					ApplyWorldPosition(output);
					return output;
				}
				
				float4 _MainColor;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = _MainColor;
					float3 view = _CameraDirection;
					float ndotv = dot(normalize(-view), normalize(input.normal));
					color.rgb *= ndotv;
					return color;
				}

				ENDHLSL
        }
    }
}
