using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class PostProcessEffect : ScriptableObject
{
    [Range(0, 1)]
    public float blend = 1;

    public static int blendId = Shader.PropertyToID("_Blend");

    public Material material;

    public virtual void Initialize()
    {

    }

    public virtual void Render(CommandBuffer cb, Camera cam, int cameraColorId, int cameraDepthId, int tempRT)
    {

    }
}
