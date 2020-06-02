Shader "LOCAL/Gizmo/Sphere"
{
    Properties
    {
		_MainColor("Color", color) = (1,1,1,1)
		_Width("Width", range(0,2)) = 2
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "StaticFX" "Queue"="Transparent+1" "RenderType"="Opaque"}

				ZWrite Off
				ZTest Less
				Blend SrcAlpha OneMinusSrcAlpha

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex GenericVertexProgram
				#pragma fragment FragmentProgram
				#include "../Libraries/Lighting.hlsl"

				float4 _MainColor;
				float _Width;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = _MainColor;
					float2 screenCoords = input.position.xy * _Width;
					float x = sin(screenCoords.x + screenCoords.y);
					if (x < 0) discard;
					return color;
				}

				ENDHLSL
        }
    }
}
