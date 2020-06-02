using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Post Process/Far Clip Wave")]
public class FarClipWave : PostProcessEffect
{

    public override void Render(CommandBuffer cb, Camera camera, int textureIn, int textureOut, int tempRT)
    {
        material.SetFloat(blendId, blend);
        //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        cb.Blit(textureIn, tempRT, material);
    }
}
