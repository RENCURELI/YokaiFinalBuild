using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.RenderPipelines.PostProcessing
{

    [CreateAssetMenu(menuName = "Rendering/Post Process/Ambient Occlusion")]
    public class AmbientOcclusion : PostProcessEffect
    {
        [Range(8, 16)]
        public int blurQuality = 8;
        [Range(1, 50)]
        public float blurStrength = 10;
        [Range(1, 4)]
        public int blurLayer = 2;
        [Range(0, 1)]
        public float blurBlend = 0.5f;

        private static int blurQualityId = Shader.PropertyToID("_BlurQuality");
        private static int blurLayerId = Shader.PropertyToID("_BlurLayers");
        private static int blurStrengthId = Shader.PropertyToID("_BlurStrength");
        private static int blurBlendId = Shader.PropertyToID("_BlurBlend");
        private static int depthTexId = Shader.PropertyToID("_DepthTex");

        public override void Render(CommandBuffer cb, Camera camera, int colorTex, int depthTex, int tempRT)
        {
            material.SetFloat(blurQualityId, blurQuality);
            material.SetFloat(blurLayerId, blurLayer);
            material.SetFloat(blurStrengthId, blurStrength);
            material.SetFloat(blurBlendId, blend);
            material.SetFloat(blendId, blend);
            //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            cb.Blit(colorTex, tempRT, material);
        }
    }
}