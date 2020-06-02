using Game.RenderPipelines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class OldConsolePipeline : RenderPipeline
{
    CommandBuffer cameraBuffer = new CommandBuffer() { name = "Render Camera" };
    Material errorMaterial;

    //Lighting
    public static bool isRenderingShadows = false;

    //Post Process
    public PostProcessPipeline postProcessPipeline;
    CommandBuffer postProcessBuffer = new CommandBuffer() { name = "Post-Processing" };
    static int cameraColorTextureId = Shader.PropertyToID("_CameraColorTexture");
    static int cameraDepthTextureId = Shader.PropertyToID("_CameraDepthTexture");

    static int grabTextureId = Shader.PropertyToID("_GrabTexture");

    private bool renderPostProcess
    {
        get
        {
            return postProcessPipeline && Application.isPlaying;
        }
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            RenderSingle(context, camera);
        }
    }

    private void RenderSingle(ScriptableRenderContext context, Camera camera)
    {
        SetupShaderProperties(context, camera);

        SetupEditorScene(context, camera);

        CullingResults cullingResults = CullRenderers(context, camera);

        cameraBuffer.ClearRenderTarget(true, false, Color.clear);
        
        cameraBuffer.BeginSample("Render Camera");

        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();

        if (isRenderingShadows)
        {
            ShadowProtocol(context, camera, cullingResults);
        }
        else
        {
            ScreenOutputProtocol(context, camera, cullingResults);

            if (renderPostProcess)
            {

                cameraBuffer.GetTemporaryRT(cameraColorTextureId, camera.pixelWidth, camera.pixelHeight, 0);
                cameraBuffer.GetTemporaryRT(cameraDepthTextureId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.Depth);
                cameraBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraColorTextureId);
                cameraBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraDepthTextureId);

                context.ExecuteCommandBuffer(cameraBuffer);
                cameraBuffer.Clear();

                postProcessPipeline.Render(cameraBuffer, camera, cameraColorTextureId, cameraDepthTextureId);
                
                //postProcessBuffer.Blit(cameraColorTextureId, BuiltinRenderTextureType.CameraTarget);

                context.ExecuteCommandBuffer(cameraBuffer);
                cameraBuffer.Clear();

                cameraBuffer.ReleaseTemporaryRT(cameraColorTextureId);
                cameraBuffer.ReleaseTemporaryRT(cameraDepthTextureId);
                
            }
        }

        cameraBuffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(cameraBuffer);
        
        cameraBuffer.Clear();

        context.Submit();
    }

    private int opaquePassColorId = Shader.PropertyToID("_OpaquePassColorTex");

    private void ScreenOutputProtocol(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {
        DrawPass(SortingCriteria.CommonOpaque, context, camera, cullingResults, "Opaque");
        
        if (SpiritVision.Inactive)
            DrawPass(SortingCriteria.CommonOpaque, context, camera, cullingResults, "HumanOpaque");
        
        if (SpiritVision.Active)
            DrawPass(SortingCriteria.CommonOpaque, context, camera, cullingResults, "SpiritOpaque");
        
        if (SpiritVision.Transition)
            DrawPass(SortingCriteria.CommonOpaque, context, camera, cullingResults, "Transition");

        ClearAll();

        context.DrawSkybox(camera);
        
        if (SpiritVision.Inactive)
            DrawPass(SortingCriteria.CommonTransparent, context, camera, cullingResults, "HumanTransparent");
        
        if (SpiritVision.Active)
            DrawPass(SortingCriteria.CommonTransparent, context, camera, cullingResults, "SpiritTransparent", "Pearl");

        DrawPass(SortingCriteria.CommonTransparent, context, camera, cullingResults, "Transparent");

        GrabDistortPass(context, camera);
        DrawPass(SortingCriteria.CommonTransparent, context, camera, cullingResults, "Distort");

        if (SpiritVision.Active)
            DrawPass(SortingCriteria.CommonTransparent, context, camera, cullingResults, "DistortSpirit");

        RenderDefault(context, camera);

        DrawGizmos(context, camera);
        cameraBuffer.ReleaseTemporaryRT(grabTextureId);
    }

    private void GrabDistortPass(ScriptableRenderContext context, Camera camera)
    {
        cameraBuffer.GetTemporaryRT(grabTextureId, camera.pixelWidth, camera.pixelHeight, 0);
        cameraBuffer.Blit(BuiltinRenderTextureType.CurrentActive, grabTextureId);
        cameraBuffer.SetGlobalTexture(grabTextureId, grabTextureId);
        cameraBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    private void ShadowProtocol(ScriptableRenderContext context, Camera camera, CullingResults cullingResults)
    {
        //Shader.SetGlobalMatrix("unity_MatrixVP", ShadowRenderer.currentVP);

        DrawPass(SortingCriteria.CommonOpaque, context, camera, cullingResults, "Shadow");

        ClearAll();
    }

    private CullingResults CullRenderers(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
        {
            throw new System.Exception("[Render Pipeline] No culling possible");
        }
        CullingResults cullingResults = context.Cull(ref cullingParameters);
        return cullingResults;
    }

    private void SetupEditorScene(ScriptableRenderContext context, Camera camera)
    {
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            PipelineLighting.enabled = UnityEditor.SceneView.currentDrawingSceneView.sceneLighting;
        }
#endif
    }

    private void SetupShaderProperties(ScriptableRenderContext context, Camera camera)
    {
        context.SetupCameraProperties(camera);
        Shader.SetGlobalVector("_WorldSpaceCameraPos", camera.transform.position);
        Shader.SetGlobalVector("_CameraDirection", camera.transform.forward);
        Shader.SetGlobalVector("_ZBufferParams", camera.transform.forward);
        Shader.SetGlobalFloat("time", Time.time);
        SetupLighting();
        cameraBuffer.GetTemporaryRT(cameraColorTextureId, camera.pixelWidth, camera.pixelHeight, 0);
        cameraBuffer.GetTemporaryRT(cameraDepthTextureId, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point, RenderTextureFormat.Depth);
    }

    private void ClearAll()
    {
        cameraBuffer.ReleaseTemporaryRT(cameraColorTextureId);
        cameraBuffer.ReleaseTemporaryRT(cameraDepthTextureId);
    }

    private void SetupLighting()
    {
        //PipelineLighting.PopulateShaderVariables();
    }

    private void GrabBuffers(ScriptableRenderContext context)
    {
        cameraBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraColorTextureId);
        cameraBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cameraDepthTextureId, 24, 24);
        cameraBuffer.SetGlobalTexture(cameraColorTextureId, cameraColorTextureId);
        cameraBuffer.SetGlobalTexture(cameraDepthTextureId, cameraDepthTextureId);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    private void DrawPassByList(Renderer[] renderers, ScriptableRenderContext context, Camera camera, CullingResults culling, params string[] passes)
    {
        SortingSettings sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        DrawingSettings drawingSettings = new DrawingSettings() { sortingSettings = sortingSettings };
        for (int i = 0; i < passes.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, new ShaderTagId(passes[i]));
        }
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
        foreach (var i in renderers)
        {
            cameraBuffer.DrawRenderer(i, i.material);
        }
        //context.DrawRenderers(culling, ref drawingSettings, ref filteringSettings);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    private void DrawPass(SortingCriteria criteria, ScriptableRenderContext context, Camera camera, CullingResults culling, params string[] passes)
    {
        SortingSettings sortingSettings = new SortingSettings(camera);
        sortingSettings.criteria = criteria;
        DrawingSettings drawingSettings = new DrawingSettings() { sortingSettings = sortingSettings };
        for (int i = 0; i < passes.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, new ShaderTagId(passes[i]));
        }
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
        context.DrawRenderers(culling, ref drawingSettings, ref filteringSettings);
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    private void DrawGizmos(ScriptableRenderContext context, Camera camera)
    {
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            if (UnityEditor.SceneView.currentDrawingSceneView.drawGizmos)
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
#endif
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    private void RenderDefault(ScriptableRenderContext context, Camera camera)
    {
        if (errorMaterial == null)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
        {
            return;
        }
        CullingResults cullingResults = context.Cull(ref cullingParameters);

        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), sortingSettings);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

        drawingSettings.SetShaderPassName(0, new ShaderTagId("ForwardBase"));
        drawingSettings.SetShaderPassName(1, new ShaderTagId("Always"));
        drawingSettings.SetShaderPassName(2, new ShaderTagId("Vertex"));
        drawingSettings.SetShaderPassName(3, new ShaderTagId("VertexLMRGBM"));
        drawingSettings.SetShaderPassName(4, new ShaderTagId("VertexLM"));
        drawingSettings.SetShaderPassName(5, new ShaderTagId("PrepassBase"));

        drawingSettings.overrideMaterial = errorMaterial;
        drawingSettings.overrideMaterialPassIndex = 0;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
