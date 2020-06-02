using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Post Process/Exposure")]
public class ExposureEffect : PostProcessEffect
{
    [Range(0,5)]
    public float exposure = 1;
    [Range(0.1f, 10)]
    public float contrast = 1;
    [Range(0, 2)]
    public float saturation = 1;

    private static int exposureId = Shader.PropertyToID("_Exposure");
    private static int contrastId = Shader.PropertyToID("_Contrast");
    private static int saturationId = Shader.PropertyToID("_Saturation");

    public override void Render(CommandBuffer cb, Camera camera, int textureIn, int textureOut, int tempRT)
    {
        material.SetFloat(exposureId, exposure);
        material.SetFloat(contrastId, contrast);
        material.SetFloat(saturationId, saturation);
        material.SetFloat(blendId, blend);
        //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        cb.Blit(textureIn, tempRT, material);
    }
}
