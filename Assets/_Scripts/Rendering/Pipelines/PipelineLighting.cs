using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.RenderPipelines
{
    public static class PipelineLighting
    {
        public static bool enabled = true;

        public const int MAXIMUM_DYNAMIC_LIGHTS = 32;
        public const int PER_OBJECT_MAXIMUM_DYNAMIC_LIGHT = 4;
        public const int PER_OBJECT_MAXIMUM_STATIC_LIGHT = 16;
        public const int LIGHT_UPDATE_SLEEP_TIME = 60; // in miliseconds

        // Dynamic lights
        public static Vector4[] lightPositions = new Vector4[MAXIMUM_DYNAMIC_LIGHTS];
        public static Vector4[] lightColors = new Vector4[MAXIMUM_DYNAMIC_LIGHTS];
        public static Vector4[] lightParams = new Vector4[MAXIMUM_DYNAMIC_LIGHTS];

        public static int lightPositionsId = Shader.PropertyToID("_DynamicLightPositions");
        public static int lightColorsId = Shader.PropertyToID("_DynamicLightColors");
        public static int lightIndicesId = Shader.PropertyToID("_PerObjectLightIndices");

        public static int staticLightPositionsId = Shader.PropertyToID("_LightPositions");
        public static int staticLightColorsId = Shader.PropertyToID("_LightColors");
        public static int staticLightParamsId = Shader.PropertyToID("_LightParams");

        private static float cancelProcessToken;
        public static async void PopulateShaderVariables()
        {
#if UNITY_EDITOR
            if (LightSource.main == null) GenerateLighting();
#endif
            
            if (lightPositions.Length != MAXIMUM_DYNAMIC_LIGHTS)
            {
                lightPositions = new Vector4[MAXIMUM_DYNAMIC_LIGHTS];
                lightColors = new Vector4[MAXIMUM_DYNAMIC_LIGHTS];
            }
            
            float cancel = cancelProcessToken = Random.value;

            while (cancel == cancelProcessToken)
            {
                for (int i = 0; i < MAXIMUM_DYNAMIC_LIGHTS; i++)
                {
                    if (i < LightSource.dynamics.Count)
                    {
                        LightSource l = LightSource.dynamics[i];
                        if (l != null)
                        {
                            lightPositions[i] = l.Position;
                            lightColors[i] = l.ColorVector;
                            continue;
                        }
                    }
                    lightPositions[i] = Vector4.zero;
                    lightColors[i] = Vector4.zero;
                }

                Shader.SetGlobalVectorArray(lightPositionsId, lightPositions);
                Shader.SetGlobalVectorArray(lightColorsId, lightColors);

                await Task.Delay(System.TimeSpan.FromMilliseconds(LIGHT_UPDATE_SLEEP_TIME));

                //if (Application.isPlaying) break;
            }

            Debug.Log("Pipeline lighting process cancelled");
        }

        private static int shaderWorldId = Shader.PropertyToID("LightWorldIndex");
        public static void SetWorldIndex(LightSource.WORLD world)
        {
            Shader.SetGlobalInt(shaderWorldId, (int)world);
        }

        public static void PopulateStaticShaderVariables(MaterialPropertyBlock mpb, int[] indices)
        {
#if UNITY_EDITOR
            if (LightSource.main == null) GenerateLighting();
#endif
            lightPositions = new Vector4[PER_OBJECT_MAXIMUM_STATIC_LIGHT];
            lightColors = new Vector4[PER_OBJECT_MAXIMUM_STATIC_LIGHT];
            lightParams = new Vector4[PER_OBJECT_MAXIMUM_STATIC_LIGHT];

            for (int i = 0; i < indices.Length; i++)
            {
                if (i < PER_OBJECT_MAXIMUM_STATIC_LIGHT && indices[i] >= 0)
                {
                    LightSource l = LightSource.statics[indices[i]];
                    if (l != null)
                    {
                        lightPositions[i] = l.Position;
                        lightColors[i] = l.ColorVector;
                        lightParams[i] = l.Info;
                        continue;
                    }
                }
                lightPositions[i] = Vector4.zero;
                lightColors[i] = Vector4.zero;
                lightParams[i] = Vector4.zero;
            }

            //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetVectorArray(staticLightPositionsId, lightPositions);
            mpb.SetVectorArray(staticLightColorsId, lightColors);
            mpb.SetVectorArray(staticLightParamsId, lightParams);
            //r.SetPropertyBlock(mpb);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Rendering/Generate Lighting")]
#endif
        public static void GenerateLighting()
        {
            if (Application.isPlaying)
            {
                ReplaceUnityLights();
            }
            LightSource.main = null;
            LightSource.dynamics.Clear();
            LightSource.statics.Clear();
            foreach (var l in GameObject.FindObjectsOfType<LightSource>())
            {
                l.Awake();
            }
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Rendering/Replace Unity Lights")]
#endif
        public static void ReplaceUnityLights()
        {
            foreach (var u in GameObject.FindObjectsOfType<Light>())
            {
                var l = u.gameObject.AddComponent<LightSource>();
                l.color = u.color;
                l.radius = u.range;
                l.intensity = u.intensity;
                Light.DestroyImmediate(u);
            }
        }
    }
}