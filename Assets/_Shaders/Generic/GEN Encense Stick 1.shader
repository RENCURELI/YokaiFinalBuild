Shader "LOCAL/Encense Stick Prison"
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
		_Opacity("Opacity", range(0,1)) = 1
		_Speed("Speed", float) = 1
	}
	SubShader
	{
			Tags{"Queue" = "Geometetry+20"}
		Pass
		{
			Name "Opaque"
			Tags{"LightMode" = "Transparent"}

			ZWrite On
			ZTest Less
			Blend SrcAlpha OneMinusSrcAlpha

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

			float _Opacity;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float3 objPos = ObjectPosition();
				const float stickHeight = 3.8;
				const float stickOffset = 1.5;
				float burnt = 1;
				if (input.worldPosition.y + stickOffset > objPos.y + _BurntFactor * stickHeight) burnt = 0;
				if (input.worldPosition.y + stickOffset + _Height > objPos.y + _BurntFactor * stickHeight
					&& input.worldPosition.y + stickOffset < objPos.y + _BurntFactor * stickHeight) return _EmitColor * _EmitIntensity;
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				float3 light = GetLighting(input.worldPosition, input.normal, color.rgb, 0, 0);
				color.rgb *= light;
				float4 emit = tex2D(_EmitTex, input.uv) * _EmitIntensity * (abs(cos(time*_Speed))*0.5+0.5);
				color.a = _Opacity;
				emit.a = 0;
				return color + emit * burnt;
			}

			ENDHLSL
        }
    }
}
