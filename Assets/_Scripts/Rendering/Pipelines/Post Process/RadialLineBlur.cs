using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Post Process/Radial Line Blur")]
public class RadialLineBlur : PostProcessEffect
{
    [Range(8, 16)]
    public int blurQuality = 8;
    [Range(1, 100)]
    public float blurStrength = 10;
    [Range(1, 4)]
    public int blurLayer = 2;
    
    private static int blurQualityId = Shader.PropertyToID("_BlurQuality");
    private static int blurLayerId = Shader.PropertyToID("_BlurLayers");
    private static int blurStrengthId = Shader.PropertyToID("_BlurStrength");

    public override void Render(CommandBuffer cb, Camera camera, int textureIn, int textureOut, int tempRT)
    {

        material.SetFloat(blurQualityId, blurQuality);
        material.SetFloat(blurLayerId, blurLayer);
        material.SetFloat(blurStrengthId, blurStrength);
        material.SetFloat(blendId, blend);
        //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        cb.Blit(textureIn, tempRT, material);
    }
}
