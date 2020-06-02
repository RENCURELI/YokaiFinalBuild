using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName ="Rendering/Post Process/Radial Blur")]
public class RadialBlur : PostProcessEffect
{
    public ComputeShader shader;

    [Range(8, 16)]
    public int blurQuality = 8;
    [Range(1, 50)]
    public float blurStrength = 10;
    [Range(1, 4)]
    public int blurLayer = 2;
    [Range(0, 1)]
    public float blurBlend = 0.5f;

    private static int sourceTextureId = Shader.PropertyToID("_Source");
    private static int destTextureId = Shader.PropertyToID("_Destination");
    private static int widthId = Shader.PropertyToID("_ScreenWidth");
    private static int HeightId = Shader.PropertyToID("_ScreenHeight");
    private static int blurQualityId = Shader.PropertyToID("_BlurQuality");
    private static int blurLayerId = Shader.PropertyToID("_BlurLayers");
    private static int blurStrengthId = Shader.PropertyToID("_BlurStrength");
    private static int blurBlendId = Shader.PropertyToID("_BlurBlend");
    private static int effectKernelId = -1;
    /*
    private Material material;

    public override void Initialize()
    {
        if (!material) material = new Material(Shader.Find("Hidden/RenderPipeline/Bloom"));
        material.SetFloat(blurQualityId, blurQuality);
        material.SetFloat(blurLayerId, blurLayer);
        material.SetFloat(blurStrengthId, blurStrength);
    }
    */
    public override void Render(CommandBuffer cb, Camera camera, int textureIn, int textureOut, int tempRT)
    {
        /*
        if (effectKernelId < 0) effectKernelId = shader.FindKernel("CSRadialBlur");
        
        cb.GetTemporaryRT(sourceTextureId, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, true);
        cb.GetTemporaryRT(destTextureId, camera.pixelWidth, camera.pixelHeight, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1, true);
        cb.Blit(cameraColorId, sourceTextureId);
        cb.Blit(cameraColorId, destTextureId);

        cb.SetComputeTextureParam(shader, effectKernelId, sourceTextureId, sourceTextureId);
        cb.SetComputeTextureParam(shader, effectKernelId, destTextureId, destTextureId);
        cb.SetComputeFloatParam(shader, widthId, camera.pixelWidth);
        cb.SetComputeFloatParam(shader, HeightId, camera.pixelHeight);
        cb.SetComputeFloatParam(shader, blurQualityId, blurQuality);
        cb.SetComputeFloatParam(shader, blurLayerId, blurLayer);
        cb.SetComputeFloatParam(shader, blurStrengthId, blurStrength); 
        cb.SetComputeFloatParam(shader, blurBlendId, blurBlend);
        cb.DispatchCompute(shader, effectKernelId, camera.pixelWidth, camera.pixelHeight, 1);
        
        cb.Blit(destTextureId, cameraColorId);

        cb.ReleaseTemporaryRT(sourceTextureId);
        cb.ReleaseTemporaryRT(destTextureId);
        */

        material.SetFloat(blurQualityId, blurQuality);
        material.SetFloat(blurLayerId, blurLayer);
        material.SetFloat(blurStrengthId, blurStrength);
        material.SetFloat(blurBlendId, blend);
        material.SetFloat(blendId, blend);
       
        //cb.SetGlobalTexture(PostProcessPipeline.mainTexId, textureIn);
        //cb.Blit(textureIn, material);
        //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        cb.Blit(textureIn, tempRT, material);
        // PostProcessPipeline.DrawTriangle(cb, material);
    }
}
