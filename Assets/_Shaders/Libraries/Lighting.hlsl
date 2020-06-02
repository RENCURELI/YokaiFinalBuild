#if !defined(LIGHTING_LIB)
#define LIGHTING_LIB

#include "Generic.hlsl"

#define STATIC_LIGHT_COUNT 16
#define DYNAMIC_LIGHT_COUNT 32
#define MAXIMUM_LIGHTS_COUNT 20
#define PER_OBJECT_DYNAMIC_LIGHT_COUNT 4

CBUFFER_START(_Light)
int _PerObjectLightIndices[PER_OBJECT_DYNAMIC_LIGHT_COUNT];
float4 _DynamicLightPositions[DYNAMIC_LIGHT_COUNT];
float4 _DynamicLightColors[DYNAMIC_LIGHT_COUNT];
float4 _LightPositions[STATIC_LIGHT_COUNT];
float4 _LightColors[STATIC_LIGHT_COUNT];
float4 _LightParams[STATIC_LIGHT_COUNT];
CBUFFER_END

float4 GetProjection(float3 worldPosition, float4x4 projectionMatrix) {
	float4 projection = mul(projectionMatrix, float4(worldPosition, 1));
	projection.x += 1;
	projection.x *= 0.5;
	projection.y += 1;
	projection.y *= 0.5;
	return projection;
}

struct Light {
	float3 position;
	float3 color;
	float range;
    int type;
	int world;
	float2 flicker;
};

Light CreateDefaultLight() {
	Light l;
	l.position = 0;
	l.range = 1;
	l.color = 0;
	l.type = 0;
	l.flicker = 0;
	l.world = 0;
	return l;
}

Light CreateStaticLight(int id) {
	Light l;
	l.position = _LightPositions[id].xyz;
	l.range = _LightPositions[id].w;
	l.color = _LightColors[id].rgb;
    l.type = round(_LightColors[id].w);
	l.flicker = _LightParams[id].xy;
	l.world = round(_LightParams[id].z);
	return l;
}

Light CreateDynamicLight(int id) {
	Light l;
	l.position = _DynamicLightPositions[id].xyz;
	l.range = _DynamicLightPositions[id].w;
	l.color = _DynamicLightColors[id].rgb;
	l.type = ceil(_DynamicLightColors[id].w);
	l.flicker = 0;
	l.world = round(_LightParams[id].z);
	return l;
}

float GetFlicker(Light l) {
	return lerp(1-l.flicker.x, 1, abs(cos(time * l.flicker.y + (l.position.x + l.position.z + l.position.y) * 25)));
}

float GetDiffuseFactor(Light l, float3 position, float3 normal) {
    if (l.type == 1)
        return 1;
    else if (l.type == 0)
    {
        float3 lightDir = normalize(position - l.position);
        float ndotl = dot(-lightDir, normalize(normal));
        return max(ndotl, 0);
    }
    else if (l.type == 2)
    {
        float3 lightDir = normalize(position - l.position);
        float ndotl = dot(-lightDir, normalize(normal));
        return max(ndotl, 0);
    }
    return 1;
}

float GetDistanceFalloff(Light l, float3 position) {
    if (l.type == 0)
    {
        float dist = length(l.position - position);
        if (dist > l.range)
            return 0;
        else
        {
            float falloff = 1 - dist / l.range;
            return abs(falloff);
        }
    }
    return 1;
	
}

float GetSpecularHighlight(Light l, float3 position, float3 normal, float smoothness)
{
    if (l.type == 1)
        return 0;
    float3 lightDir = normalize(position - l.position);
    float3 view = ViewDirection(position);
    float3 reflected = reflect(lightDir, normal);
    float rdotv = dot(-normalize(view), normalize(reflected));
    return pow(max(rdotv, 0), smoothness * 100) * (smoothness * 5);
}

float3 GetSpecularColor(Light l, float metallic, float3 albedo)
{
    if (l.type == 1)
        return 0;
    return lerp(l.color, albedo, metallic);
}

#define FOG_DISTANCE 50.0
#define FOG_LENGTH 20.0

float GetFog(float3 position) {
	float distToCamera = distance(position, _WorldSpaceCameraPos);
	if (distToCamera < FOG_DISTANCE) return 1;

	float fogDensity = 1.0 / FOG_LENGTH;

	float fogOcclusion = 1.0 - (distToCamera - FOG_DISTANCE) * fogDensity;

	return fogOcclusion;
}

#define NEAR_LIGHT_RANGE 3
#define NEAR_LIGHT_COLOR float3(1,1,1) * 0.7

float3 GetNearLight(float3 position, float3 normal, float3 albedo, float smoothness, float metallic) {
	Light l = CreateDefaultLight();
	l.position = _WorldSpaceCameraPos;
	l.type = 0;
	l.color = NEAR_LIGHT_COLOR;
	l.range = NEAR_LIGHT_RANGE;

	float atten = GetDistanceFalloff(l, position);
	float3 diffuse = l.color
		* GetDiffuseFactor(l, position, normal)
		* (1 - metallic)
		* atten;
	float3 specular = GetSpecularHighlight(l, position, normal, smoothness)
		* GetSpecularColor(l, metallic, albedo)
		* atten;
	return diffuse + specular;
}

int LightWorldIndex = 0;

float GetWorldAtten(int world) {
	if (world == 0) return 1;
	else {
#if defined(LIT_WORLD_HUMAN)
		if (world == 1) return 1;
		else return 0;
#endif
#if defined(LIT_WORLD_SPIRIT)
		if (world == 2) return 1;
		else return 0;
#endif
		return 1;
	}
}

float3 GetLighting(float3 position, float3 normal, float3 albedo, float smoothness, float metallic) {
	float3 result = 0;
	Light lights[MAXIMUM_LIGHTS_COUNT];
	for (int i = 0; i < STATIC_LIGHT_COUNT; i++) {
		lights[i] = CreateStaticLight(i);
	}
	
	for (int i = 0; i < PER_OBJECT_DYNAMIC_LIGHT_COUNT; i++) {
		int index = _PerObjectLightIndices[i];
		Light l;
		if (index >= 0) {
			l = CreateDynamicLight(index);
		}
		else {
			l = CreateDefaultLight();
		}
		lights[STATIC_LIGHT_COUNT + i] = l;
	}
	
	for (int i = 0; i < MAXIMUM_LIGHTS_COUNT; i++)
	{
		Light l = lights[i];
        float atten = GetDistanceFalloff(l, position);
		atten *= GetFlicker(l) * GetWorldAtten(l.world);
        float3 diffuse = l.color
            * GetDiffuseFactor(l, position, normal)
            * (1 - metallic)
            * atten;
        float3 specular = GetSpecularHighlight(l, position, normal, smoothness)
            * GetSpecularColor(l, metallic, albedo)
            * atten;
        result += diffuse + specular;
	}

	float3 nearLight = GetNearLight(position, normal, albedo, smoothness, metallic);
	result += nearLight;

	float fog = GetFog(position);
	result *= fog;

	return result;
}

#endif