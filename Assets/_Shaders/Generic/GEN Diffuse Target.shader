Shader "LOCAL/GEN/Diffuse Target"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
	}
	SubShader
	{

		Pass
		{
			Tags{"LightMode" = "Static" "Queue"="Geometetry" "RenderType"="Opaque"}

			ZWrite On
			ZTest Less
			Blend One Zero
			Cull Off

			HLSLPROGRAM

			#pragma target 3.5
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "../Libraries/Lighting.hlsl"

			float3 target;

			GenericFragmentInput VertexProgram(GenericVertexInput input) {
				GenericFragmentInput output = GenericVertexProgram(input);
				float3 origin = ObjectPosition();
				float3 toTarget = target - origin;
				float3 x = -toTarget;
				float3 z = normalize(cross(x, float3(0, 1, 0)));
				float3 y = normalize(cross(x, z));
				float scale = 100;
				output.worldPosition = origin + input.position.x * x * scale + input.position.y * z * scale + input.position.z * -y * scale;
				ApplyWorldPosition(output);
				return output;
			}

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				float4 color = tex2D(_MainTex, input.uv) * _MainColor * input.color.r;
				if (color.a < 0.5) discard;
				float3 light = GetLightingDiffuse(input);
				color.rgb *= light;
				return color;
			}

			ENDHLSL
		}
    }
}
