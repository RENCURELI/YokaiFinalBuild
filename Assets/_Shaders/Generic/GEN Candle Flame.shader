Shader "LOCAL/GEN/Candle Flame"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_DispTex("Displacement", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
		_Flicker("Flicker", float) = 1
		_DispFactor("Displacement Factor", float) = 1
	}
		SubShader
		{

			Pass
			{
				Tags{"LightMode" = "Transparent" "Queue"="Geometetry" "RenderType"="Transparent"}

				ZWrite Off
				ZTest Less
				Blend One One
				Cull Off

				HLSLPROGRAM

				#pragma target 3.5
				#pragma vertex VertexProgram
				#pragma fragment FragmentProgram
				#include "../Libraries/Lighting.hlsl"

				GenericFragmentInput VertexProgram(GenericVertexInput input){
					GenericFragmentInput output = GenericVertexProgram(input);
					float3 center = ObjectPosition();
					float3 scale = mul(unity_ObjectToWorld, float4(1, 1, 1, 0)).xyz;
					float3 view = output.worldPosition - _WorldSpaceCameraPos;
					float3 tangent = AxisCross(view, float3(0, 1, 0));
					float3 dir = AxisCross(tangent, float3(0, 1, 0));
					float3 pos = float3(0,0,0);
					pos += input.position.x * tangent;
					pos += input.position.y * float3(0, 1, 0);
					pos += input.position.z * dir;
					pos *= scale;
					pos += center;
					output.worldPosition = pos;
					output.color.g = rand(center);
					ApplyWorldPosition(output);
					return output;
				}

				sampler2D _MainTex;
				sampler2D _DispTex;
				float4 _MainColor;
				float _Flicker;
				float _DispFactor;
				float _FadeFactor = 0;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float h = input.uv.y;
					float2 coords = input.uv;
					float4 color = tex2D(_MainTex, coords);
					float energy = (color.r + color.g + color.b) * (1.0 / 3.0) * 0.8;
					float2 disp = tex2D(_DispTex, input.uv).rg*_DispFactor*(1-_FadeFactor) * (1 - energy);
					float rndm = input.color.g * 35;
					coords += float2(cos(time * _Flicker+ disp.x+ rndm*2), sin(time * _Flicker - disp.y+ rndm)) * h * 0.1;
					color = tex2D(_MainTex, coords) * _MainColor * (1-_FadeFactor);
					return color;
				}

				ENDHLSL
        }
    }
}
