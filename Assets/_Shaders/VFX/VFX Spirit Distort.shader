Shader "LOCAL/VFX/Spirit Distort"
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
		Tags{"Queue" = "Geometry-1"}

		Pass
		{
			Name "Main"
			Tags{"LightMode" = "DistortSpirit"}

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
				float3 toCenter = center;// -origin;
				float3 spin = float3(1, 0, 0) * cos(seed*5 + lifetime * 12) * 0.1
					+ float3(0, 0, 1) * sin(seed*5 + lifetime * 12) * 0.1;
				return origin + normalize(toCenter) * lerp(_Spread2, _Spread, pow(lifetime,0.8)) * abs(sin(seed*20)) + spin * abs(cos(seed*30));
			}

			float Lifetime(float seed) {
				return fmod(time*_Speed + seed * 34, 1) + abs(cos(seed * 30)) * 0.3 * _Lifetime;
				//return _Lifetime + abs(cos(seed*30)) * 0.3 * _Lifetime;
			}

			float2 Size(float seed, float lifetime) {
				return float2(1, 1) * lerp(_Width, _Height, abs(cos(seed * 17)));
			}

			float Seed(float3 center) {
				//float3 pos = mul(unity_ObjectToWorld, float4(i.worldPosition, 1)).xyz * 100;
				float seed = (rand(normalize(center)));
				return seed;
			}

			#define PARTICLE_SEED Seed
			#define PARTICLE_LIFETIME Lifetime
			#define PARTICLE_TRAJECTORY ParticleTrajectory
			#define PARTICLE_SIZE Size
			#define PARTICLE_LOCAL_CENTER_COMPUTE

			#include "../Libraries/Particles.hlsl"

			sampler2D _MainTex;
			float4 _MainColor;
			sampler2D _CameraColorTexture;

			float4 FragmentProgram(GenericFragmentInput input) : SV_TARGET{
				if (_Intensity <= 0) discard;
				float seed = input.color.b;
				float mask = tex2D(_MainTex, input.uv).r;
				if (mask < 0.01) discard;
				float4 viewPos = mul(unity_MatrixVP, float4(input.worldPosition, 1));
				float ablend = 1 - abs(input.color.r * 2 - 1);
				float2 coords = viewPos.xy / viewPos.w;
				coords.y += 1;
				coords.y *= 0.5;
				coords.y = 1 - coords.y;
				coords.x += 1;
				coords.x *= 0.5;
				coords += (input.uv*2-1) * ablend * 0.01 * mask * _Amplitude;
				float4 color = tex2D(_CameraColorTexture, coords) * _MainColor;
				ablend *= input.color.g;
				color.a *= ablend * mask;
				return color * _Emit * _Intensity;
			}

			ENDHLSL
		}
    }
}
