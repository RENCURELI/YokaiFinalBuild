using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ParticleBurst : MonoBehaviour
{
    public Renderer[] renderers;
    public float duration;
    public bool editorPreview;

    [System.NonSerialized]
    public AsyncProcessId process = new AsyncProcessId();

    public void Awake()
    {
        process = process.GetNew();
        UpdateMaterials(process);
    }

    private void OnDestroy()
    {
        process.Cancel();
    }

    private async void UpdateMaterials(AsyncProcessId id)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        float timer = duration;

        const float deltaTime = 0.01f;

        int propId = Shader.PropertyToID("_Lifetime");

        while (timer > 0)
        {
            float t = 1 - timer / duration;
            mpb.SetFloat(propId, t);
            foreach (var r in renderers)
            {
                if (r != null) r.SetPropertyBlock(mpb);
            }
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (id.Canceled) return;
            timer -= deltaTime;
        }

        if (Application.isEditor && !Application.isPlaying)
        {
            id.Cancel();
        } else
        {
            Destroy(gameObject);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ParticleBurst))]
public class ParticleBurstEditor : UnityEditor.Editor
{
    private bool updating = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ParticleBurst obj = (ParticleBurst)target;
        if (!obj.process.valid || !updating)
        {
            obj.Awake();
            updating = true;
        }
    }
    public void OnSceneGUI()
    {
        UnityEditor.SceneView.RepaintAll();
    }
    public void OnDisable()
    {
        ParticleBurst obj = (ParticleBurst)target;
        obj.process.Cancel();
        updating = false;
    }
}

#endif