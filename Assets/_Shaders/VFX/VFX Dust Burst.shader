Shader "LOCAL/VFX/Dust Burst"
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
		Tags{"Queue" = "Geometry+51"}

		Pass
		{
			Name "Main"
			Tags{"LightMode" = "Transparent"}

			ZWrite Off
			ZTest Less
			Blend SrcAlpha OneMinusSrcAlpha
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
				float3 dir = ObjectDirection(float3(0, 0, 1));
				float3 spread = ObjectDirection(float3(1, 0, 0)) * sin(seed * 17) * lerp(_Spread2, _Spread, lifetime);
				float3 spin = float3(1, 0, 0) * cos(seed*5 + lifetime * 12) * 0.1
					+ float3(0, 1, 0) * sin(seed*5 + lifetime * 12) * 0.1;
				return origin + spread + normalize(dir) * _Amplitude * (lifetime + sin(seed*8)*0.2) + spin * abs(cos(seed*30));
			}

			float Lifetime(float seed) {
				return _Lifetime + abs(cos(seed*30)) * 0.3 * _Lifetime;
			}

			float2 Size(float seed, float lifetime) {
				return float2(1, 1) * lerp(_Width, _Height, abs(cos(seed * 17)));
			}

			#define PARTICLE_LIFETIME Lifetime
			#define PARTICLE_TRAJECTORY ParticleTrajectory
			#define PARTICLE_SIZE Size
			//#define PARTICLE_LIGHT

			#include "../Libraries/Particles.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (_Intensity <= 0) discard;
				float2 coords = float2(input.uv.x, 1 - input.uv.y);
				float4 color = tex2D(_MainTex, coords) * _MainColor;
				float ablend = 1 - abs(input.color.r * 2 - 1);
				ablend *= input.color.g;
				color.a *= ablend;
				if (color.a <= 0) discard;
				//float3 light = input.localPosition;
				//color.rgb *= light;
				return color * _Emit * _Intensity;
			}

			ENDHLSL
		}
    }
}
