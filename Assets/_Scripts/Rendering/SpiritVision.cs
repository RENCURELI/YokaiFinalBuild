using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpiritVision : MonoBehaviour
{
    public static SpiritVision main;

    public const float bias = 0.00001f;

    public static bool Active
    {
        get
        {
            if (!Application.isPlaying)
            {
                Transition = false;
                Shader.SetGlobalFloat("_SpiritWorldForce", 1);
                return true;
            }
            return radius > 0+ bias;
        }
    }

    public static bool Inactive
    {
        get
        {
            if (!Application.isPlaying)
            {
                Transition = false;
                Shader.SetGlobalFloat("_SpiritWorldForce", 1);
                return true;
            }
            return radius < 1- bias;
        }
    }

    public static bool Transition = false;

    public static float radius;
    public static float maxRadius = 100;
    public const float transitionTime = 2;
    public const float transitionSlope = 0.2f;
    public const float transitionUpdateTime = 0.01f;

    [Range(0,1)] public float manualRadius;
    public bool testFadeIn = false;

    private int radiusPropId = Shader.PropertyToID("_SpiritWorldRadius");

    private void Awake()
    {
        main = this;
        radius = 0;
        //FadeIn();
        Shader.SetGlobalFloat("_SpiritWorldForce", 0);
    }

    void Update()
    {
        if (!main) main = this;
        radius = manualRadius;
        Shader.SetGlobalFloat(radiusPropId, radius * maxRadius);
        if (testFadeIn)
        {
            testFadeIn = false;
            FadeIn();
        }
    }

    private float cancelFadeToken = -1;
    public async void FadeIn()
    {
        float cancel = cancelFadeToken = UnityEngine.Random.value;
        float timer = transitionTime;
        Transition = true;
        while (timer > 0)
        {
            if (cancel != cancelFadeToken) return;
            timer -= transitionUpdateTime;
            float x = Mathf.Clamp01( timer / transitionTime);
            float y = Mathf.Pow(x, transitionSlope);
            manualRadius = 1-y;
            await Task.Delay(TimeSpan.FromSeconds(transitionUpdateTime));
        }
        Transition = false;
        manualRadius = 1;
    }

    public async void FadeOut()
    {
        float cancel = cancelFadeToken = UnityEngine.Random.value;
        float timer = transitionTime;
        Transition = true;
        while (timer > 0)
        {
            if (cancel != cancelFadeToken) return;
            timer -= transitionUpdateTime;
            float x = timer / transitionTime;
            float y = Mathf.Pow(x, 1.0f/transitionSlope);
            manualRadius = y;
            await Task.Delay(TimeSpan.FromSeconds(transitionUpdateTime));
        }
        Transition = false;
        manualRadius = 0;
    }
}
