Shader "LOCAL/GEN/Tessallation"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
		TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
		TessellationEdgeLength("Tessellation Edge Length", Range(5, 100)) = 50
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
				#pragma vertex TessellationVertexProgram
				#pragma fragment FragmentProgram
				#pragma hull HullProgram
				#pragma domain DomainProgram
				#pragma geometry MyGeometryProgram
				#pragma require geometry
				#pragma require tessellation tessHW

				#define TESSELLATION_SHARP
				#define TESSELLATION_EDGE
				#define DISPLACEMENT_PHONG

				#include "../Libraries/Tessellation.hlsl"

				

				[maxvertexcount(3)]
				void MyGeometryProgram (triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {
				
					float3 p0 = i[0].worldPosition;
					float3 p1 = i[1].worldPosition;
					float3 p2 = i[2].worldPosition;

					float3 triangleNormal = normalize(cross(p1 - p0, p2 - p0));

					i[0].color = float3(1,0,0);
					i[1].color = float3(0,1,0);
					i[2].color = float3(0,0,1);

					stream.Append(i[0]);
					stream.Append(i[1]);
					stream.Append(i[2]);
				}

				float4 _MainColor;

				float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
					float4 color = float4(input.color,1);
					return color;
				}

				ENDHLSL
        }
    }
}
