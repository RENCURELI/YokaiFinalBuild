#if !defined(PARTICLES_LIB)
#define PARTICLES_LIB

#define TESSELLATION_BY_DISTANCE
#define TESSELLATION_SHARP

#include "Tessellation.hlsl"
#include "Lighting.hlsl"

float3 DefaultParticleTrajectory(float3 center, float lifetime, float seed) {
	float3 origin = ObjectPosition();
	float3 toObjCenter = center - origin;
	return origin + normalize(-toObjCenter) * lifetime * _Spread;
}

#if !defined(PARTICLE_TRAJECTORY)
#define PARTICLE_TRAJECTORY DefaultParticleTrajectory
#endif

float DefaultLifetime(float seed) {
	return _Lifetime;
}

#if !defined(PARTICLE_LIFETIME)
#define PARTICLE_LIFETIME DefaultLifetime
#endif

float DefaultSeed(float3 center) {
	//float3 pos = mul(unity_ObjectToWorld, float4(i.worldPosition, 1)).xyz * 100;
	float seed = (rand(center));
	return seed;
}

#if !defined(PARTICLE_SEED)
#define PARTICLE_SEED DefaultSeed
#endif

float2 DefaultSize(float seed, float lifetime) {
	return float2(_Width, _Height);
}

#if !defined(PARTICLE_SIZE)
#define PARTICLE_SIZE DefaultSize
#endif

#if defined(PARTICLE_LOCAL_CENTER_COMPUTE)
#define PARTICLE_CENTER localCenter
#else
#define PARTICLE_CENTER center
#endif

[maxvertexcount(6)]
void ParticlesGeometryProgram(triangle GenericFragmentInput i[3], inout TriangleStream<GenericFragmentInput> stream) {

	float3 p0 = i[0].worldPosition;
	float3 p1 = i[1].worldPosition;
	float3 p2 = i[2].worldPosition;

	float3 center = (p0 + p1 + p2) / 3.0;
	float3 localCenter = (i[0].localPosition + i[1].localPosition + i[2].localPosition) * 1.0 / 3.0;
	float3 normal = i[0].normal + i[1].normal + i[2].normal;
	normal = normalize(normal);
	float seed = PARTICLE_SEED(PARTICLE_CENTER);

	float distToCamera = distance(center, _WorldSpaceCameraPos);
	if (distToCamera > TessellationMinDist - _Blend) {
		i[0].color.g = 1 - min((distToCamera - (TessellationMinDist - _Blend)) / _Blend, 1);
	}
	else {
		i[0].color.g = 1;
	}

	float lifetime = saturate(PARTICLE_LIFETIME(seed));
	i[0].color.r = lifetime;
	i[0].color.b = seed;

	float3 pos = PARTICLE_TRAJECTORY(PARTICLE_CENTER, lifetime, seed);

	float3 dir = normalize(-_CameraDirection);

#if defined(PARTICLE_LIGHT)
	i[0].localPosition = GetLighting(center, _CameraDirection, 1, 0, 0);
#endif

	GenerateQuad(i[0], stream, pos, PARTICLE_SIZE(seed, lifetime), dir, float3(0, 1, 0));//normalize(-_CameraDirection)
}

#endif