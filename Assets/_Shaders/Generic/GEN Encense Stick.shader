Shader "LOCAL/Encense Stick"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_EmitTex("Emission Map", 2D) = "black" {}
		_EmitColor("Emission Color", color) = (1,1,1,1)
		_EmitIntensity("Emission Intensity", range(0,2)) = 1
		_MainColor("Color (Human)", color) = (1,1,1,1)
		_SpiritColor("Color (Spirit)", color) = (1,1,1,1)
		_BurntFactor("Burnt Factor", range(0,1)) = 1
		_Height("Height", float) = 0.05
		_Speed("Speed", float) = 1
	}
	SubShader
	{
		Pass
		{
			Name "Opaque"
			Tags{"LightMode" = "Opaque" "Queue"="Geometetry+20" "RenderType"="Opaque"}

			ZWrite On
			ZTest Less
			Blend One Zero

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex GenericVertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float _Height;
			float _Speed;

			sampler2D _EmitTex;
			float4 _EmitColor;
			float _EmitIntensity;
			float _BurntFactor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float3 objPos = ObjectPosition();
				if (input.worldPosition.y + 0.4f > objPos.y + _BurntFactor) discard;
				if (input.worldPosition.y + 0.4f + _Height > objPos.y + _BurntFactor) return _EmitColor * _EmitIntensity;
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				float4 emit = tex2D(_EmitTex, input.uv) * _EmitIntensity * (abs(cos(time*_Speed))*0.5+0.5);
				return color + emit;
			}

			ENDHLSL
        }
    }
}
