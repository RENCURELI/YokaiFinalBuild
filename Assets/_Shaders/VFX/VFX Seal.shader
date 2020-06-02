Shader "LOCAL/VFX/Seal"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_Strength("Strength", float) = 1
		_Speed("Speed", float) = 1
	}
	SubShader
	{
		Pass
		{
			Name "Seal"
			Tags{"LightMode" = "Transparent" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite Off
			ZTest Less
			Blend One One
			Cull Back

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram

			#include "../Libraries/Generic.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;
			float _Speed;
			float _Strength;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float2 coords = input.uv;
				float flicker = lerp(0.5, 1.0, abs(cos(time * _Speed)));
				float4 color = (1 - tex2D(_MainTex, coords)) * _MainColor * flicker * _Strength;
				return color;
			}

			ENDHLSL
        }
    }
}
