﻿Shader "LOCAL/VFX/Encens Sparkles"
{
    Properties
    {
		_MainTex("Texture", 2D) = "white" {}
		_MainColor("Color", color) = (1,1,1,1)
		_Emit("Emit", float) = 1

		_Blend("Blend", float) = 1
		_Width("Width", float) = 1
		_Height("Height", float) = 1
		_Amplitude("Amplitude", float) = 1
		_Speed("Speed", float) = 1
		_Spread("Spread", float) = 2
		_Spread2("Spread 2", float) = 2
		_Lifetime("Time", range(0,1)) = 0.5
		_Intensity("Intensity", range(0,1)) = 1

		TessellationMinDist("Tessellation Limit", Range(10, 200)) = 30
		TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
	}
	SubShader
	{
		Tags{"Queue" = "Geometry+52"}

		Pass
		{
			Name "Main"
			Tags{"LightMode" = "Transparent"}

			ZWrite Off
			ZTest Less
			Blend One One
			Cull Front

			HLSLPROGRAM

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma geometry ParticlesGeometryProgram
			#pragma require geometry
			#pragma require tessellation tessHW
			#pragma fragment FragmentProgram

			#define GENERIC_GEOMETRY
			#include "../Libraries/Generic.hlsl"

			float _Emit;
			float _Width;
			float _Height;
			float _Blend;
			float _Amplitude;
			float _Speed;
			float _Spread;
			float _Spread2;
			float _Lifetime;
			float _Intensity;

			float3 ParticleTrajectory(float3 center, float3 lifetime, float seed) {
				float3 origin = ObjectPosition();
				float3 toCenter = center - origin;
				float3 spin = float3(1, 0, 0) * cos(seed + lifetime * 12) * 0.1
					+ float3(0, 0, 1) * sin(seed + lifetime * 12) * 0.1;
				return origin + float3(0,1,0) * _Amplitude * lifetime + normalize(toCenter) * lerp(_Spread2, _Spread, lifetime) * length(toCenter) + spin * cos(seed*10);
			}

			float Lifetime(float seed) {
				return fmod(seed + time * _Speed, 1) * lerp(0.5,1.2, abs(cos(seed*20)*0.5));
			}

			#define PARTICLE_LIFETIME Lifetime
			#define PARTICLE_TRAJECTORY ParticleTrajectory

			#include "../Libraries/Particles.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (_Intensity <= 0) discard;
				float2 coords = float2(input.uv.x, 1 - input.uv.y);
				float4 color = tex2D(_MainTex, coords) * _MainColor;
				float ablend = 1 - abs(input.color.r * 2 - 1);
				ablend *= input.color.g;
				color *= ablend;
				if (color.a <= 0) discard;
				return color * _Emit * _Intensity;
			}

			ENDHLSL
		}
    }
}
