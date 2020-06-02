using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TrailMaterialProperty : MonoBehaviour
{
    private new Renderer renderer;

    public const int trailJointCount = 3;
    private Vector4[] trailJoints;
    private const float trailSpeed = 2;

    private static int shaderPropId = Shader.PropertyToID("_Trail");
    private MaterialPropertyBlock mpb;

    private AsyncProcessId mainProcess = new AsyncProcessId();

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        mainProcess = mainProcess.GetNew();
        trailJoints = new Vector4[trailJointCount];
        for (int i = 0; i < trailJoints.Length; i++)
        {
            trailJoints[i] = transform.position;
        }
        Register();
    }

    private void Register()
    {
        SetTrailLimit();
        MainTask(mainProcess);
    }

    private int trailLimit = 1;

    private async void SetTrailLimit()
    {
        trailLimit = 3;
        return;
        const float deltaTime = 0.7f;
        trailJoints[0] = transform.position;
        trailJoints[1] = transform.position;
        trailJoints[2] = transform.position;
        await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        trailJoints[1] = trailJoints[0];
        trailLimit = 2;
        await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        trailJoints[2] = trailJoints[1];
        trailLimit = 3;
    }

    private async void MainTask(AsyncProcessId process)
    {
        const float deltaTime = 0.05f;

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));

            Vector3 target = transform.position;
            for (int i = 0; i < trailLimit; i++)
            {
                Vector3 position = trailJoints[i];
                Vector3 toTarget = target - position;
                trailJoints[i] = Vector3.Lerp(position, target, trailSpeed * deltaTime);
                target = position;
            }
            mpb.SetVectorArray(shaderPropId, trailJoints);
            renderer.SetPropertyBlock(mpb);
        }
    }

    private void Unregister()
    {
        mainProcess.Cancel();
    }

    private void OnDestroy()
    {
        Unregister();
    }
    /*
    private void OnDisable()
    {
        Unregister();
    }
    */
    private void OnDrawGizmosSelected()
    {
        Vector3 target = transform.position;
        for (int i = 0; i < trailJoints.Length; i++)
        {
            Vector3 joint = trailJoints[i];
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target, joint);
            target = joint;
        }
    }
}
