using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Post-Processing Stack")]
public class PostProcessPipeline : ScriptableObject
{
    public static int mainTexId = Shader.PropertyToID("_MainTex");

    static Mesh fullScreenTriangle;

    static Material material;
    static Material bloom;

    public PostProcessEffect[] passes;

    static void InitializeStatic()
    {
        if (fullScreenTriangle)
        {
            return;
        }
        fullScreenTriangle = new Mesh
        {
            name = "My Post-Processing Stack Full-Screen Triangle",
            vertices = new Vector3[] {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f,  3f, 0f),
                new Vector3( 3f, -1f, 0f)
            },
            triangles = new int[] { 0, 1, 2 },
        };
        fullScreenTriangle.UploadMeshData(true);
        material =
            new Material(Shader.Find("VFX/RenderPipeline/CopyData"))
            {
                name = "PostProcess: CopyData",
                hideFlags = HideFlags.HideAndDontSave
            };
        
    }

    private bool initialized = false;
    private void Initialize()
    {
        if (initialized) return;
        foreach (var i in passes)
        {
            i.Initialize();
        }
        initialized = true;
    }

    private static int tempRT = Shader.PropertyToID("_TempDest");

    public void Render(CommandBuffer cb, Camera camera, int cameraColorId, int cameraDepthId)
    {
        InitializeStatic();
        //Initialize();

        cb.SetGlobalTexture(mainTexId, cameraColorId);
        cb.SetGlobalTexture(cameraDepthId, cameraDepthId);
        cb.GetTemporaryRT(tempRT, camera.pixelWidth, camera.pixelHeight, 16, FilterMode.Point, RenderTextureFormat.ARGB32);
        cb.Blit(cameraColorId, tempRT);
        cb.SetGlobalTexture(mainTexId, cameraColorId);

        for (int i = 0; i < passes.Length; i++)
        {
            cb.SetGlobalTexture(mainTexId, cameraColorId);

            //cb.SetRenderTarget(tempRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            //cb.ClearRenderTarget(true, false, Color.clear);
            //cb.SetGlobalTexture(mainTexId, cameraColorId);
            //cb.Blit(cameraColorId, tempRT, material);
            //DrawTriangle(cb, material);
            passes[i].Render(cb, camera, cameraColorId, cameraDepthId, tempRT);
            cb.Blit(tempRT, cameraColorId);
            //cb.SetRenderTarget(cameraColorId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
            //DrawTriangle(cb, material);
        }

        /*
        cb.SetGlobalTexture(mainTexId, cameraColorId);
        cb.SetRenderTarget( BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
        //cb.DrawMesh(fullScreenTriangle, Matrix4x4.identity, material);
        DrawTriangle(cb, material);
        */
        cb.Blit(tempRT, cameraColorId);
        cb.Blit(cameraColorId, BuiltinRenderTextureType.CameraTarget);
        cb.ReleaseTemporaryRT(tempRT);
    }

    public static void DrawTriangle(CommandBuffer cb, Material mat)
    {
        InitializeStatic();
        cb.DrawMesh(fullScreenTriangle, Matrix4x4.identity, mat);
    }
}
