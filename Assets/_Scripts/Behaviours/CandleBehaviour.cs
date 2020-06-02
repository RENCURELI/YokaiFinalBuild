using Game.RenderPipelines;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CandleBehaviour : MonoBehaviour
{
    private static CandleBehaviour main;
    private static List<CandleBehaviour> all = new List<CandleBehaviour>();

    public ComputeShader behaviourShader;
    private CandleSensor candleSensor;
    public Renderer[] flameRenderers;
    public Renderer[] burst;

    private AsyncProcessId mainProcess = new AsyncProcessId();
    public bool canUpdate = false;

    private static int shaderFlameFactorId = Shader.PropertyToID("_FadeFactor");
    private static int shaderTimeFactorId = Shader.PropertyToID("_Lifetime");

    private void Start()
    {
        candleSensor = GetComponent<CandleSensor>();
        if (!candleSensor) gameObject.AddComponent<CandleSensor>();

        mainProcess = mainProcess.GetNew();

        foreach (var r in flameRenderers)
        {
            r.transform.rotation = Quaternion.identity;
        }

        //burst = GetComponentInChildren<Animator>();

        Dim(mainProcess);

        all.Add(this);

        canChange = true;

        if (!main)
        {
            main = this;
            DelayedStart(mainProcess);
        }
    }

    private void OnDestroy()
    {
        mainProcess.Cancel();
    }

    private struct CandleInput
    {
        public Vector3 position;
        public float range;
        public static int size
        {
            get
            {
                return sizeof(float) * 4;
            }
        }
    }

    private struct CandleOutput
    {
        public int flag;
        public static int size
        {
            get
            {
                return sizeof(int);
            }
        }
    }

    private struct EnemyInput
    {
        public Vector3 position;
        public static int size
        {
            get
            {
                return sizeof(float) * 3;
            }
        }
    }

    private int kernelId;

    private async void DelayedStart(AsyncProcessId process)
    {
        await Task.Delay(System.TimeSpan.FromSeconds(2));
        if (process.Canceled) return;
        
        UpdateAll(process);
    }

    private async void UpdateAll(AsyncProcessId process)
    {
        all.Clear();
        all.AddRange(FindObjectsOfType<CandleBehaviour>());
        EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>();

        Debug.Log("Candles count: " + all.Count);

        foreach (var o in all)
        {
            o.lit = true;
        }

        const float deltaTime = 0.3f;

        Player player = FindObjectOfType<Player>();

        behaviourShader = Resources.Load<ComputeShader>("BehaviourShader");

        kernelId = behaviourShader.FindKernel("CSCandleUpdate");
        ComputeBuffer inputBuffer = new ComputeBuffer(all.Count, CandleInput.size);
        CandleInput[] inputs = new CandleInput[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            inputs[i] = new CandleInput() { position = all[i].transform.position, range = all[i].candleSensor.radius };
        }
        inputBuffer.SetData(inputs);
        behaviourShader.SetBuffer(kernelId, "candleInputs", inputBuffer);
        behaviourShader.SetInt("candleCount", all.Count);
        ComputeBuffer outputBuffer = new ComputeBuffer(all.Count, CandleOutput.size);
        CandleOutput[] outputs = new CandleOutput[all.Count];
        for (int i = 0; i < all.Count; i++)
        {
            outputs[i] = new CandleOutput() {flag = 0};
        }
        outputBuffer.SetData(outputs);
        behaviourShader.SetBuffer(kernelId, "candleOutputs", outputBuffer);
        ComputeBuffer enemiesBuffer = new ComputeBuffer(enemies.Length, EnemyInput.size);
        EnemyInput[] enemiesArray = new EnemyInput[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            enemiesArray[i] = new EnemyInput() { position = enemies[i].transform.position };
        }
        enemiesBuffer.SetData(enemiesArray);
        behaviourShader.SetBuffer(kernelId, "enemyInputs", enemiesBuffer);
        behaviourShader.SetInt("enemyCount", enemies.Length);

        int playerPropId = Shader.PropertyToID("playerPosition");
        int outBufferPropId = Shader.PropertyToID("candleOutputs");
        int enemiesBufferPropId = Shader.PropertyToID("enemyInputs");

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) break;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue;
                enemiesArray[i] = new EnemyInput() { position = enemies[i].transform.position };
            }
            enemiesBuffer.SetData(enemiesArray);
            //behaviourShader.SetBuffer(kernelId, outBufferPropId, outputBuffer);
            behaviourShader.SetBuffer(kernelId, enemiesBufferPropId, enemiesBuffer);
            behaviourShader.SetVector(playerPropId, player.transform.position);
            behaviourShader.Dispatch(kernelId, all.Count, 1, 1);

            CandleOutput[] dispatchedOutput = new CandleOutput[all.Count];
            outputBuffer.GetData(dispatchedOutput);

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null) continue;
                //Debug.Log("Candle Flag: " + dispatchedOutput[i].flag);
                all[i].canChange = true;
                if (dispatchedOutput[i].flag == 1)
                {
                    if (!all[i].lit)
                    {
                        all[i].Light(all[i].mainProcess);
                        Debug.Log("Lit candle!!");
                    }
                }
                else
                {
                    if (all[i].lit)
                    {
                        all[i].Dim(all[i].mainProcess);
                        Debug.Log("Dimmed candle!!");
                    }
                }
            }
        }

        Debug.Log("Process cancelled");

        inputBuffer.Dispose();
        outputBuffer.Dispose();
        enemiesBuffer.Dispose();
    }

    private void UpdateSingle()
    {
        if (!canChange) return;
        bool triggered = false;
        foreach (var i in CandleTrigger.all)
        {
            float dist = Vector3.Distance(i.transform.position, transform.position);
            if (dist <= candleSensor.radius)
            {
                triggered = true;
                break;
            }
        }
        if (triggered && !candleSensor.triggered)
        {
            Light(mainProcess);
        }
        else if (!triggered && candleSensor.triggered)
        {
            Dim(mainProcess);
        }
        candleSensor.triggered = triggered;
    }

    private bool canChange = true;

    public bool lit = false;

    private void SetMPB(MaterialPropertyBlock mpb)
    {
        foreach (var r in flameRenderers)
        {
            r.SetPropertyBlock(mpb);
        }
    }

    private async void Light(AsyncProcessId process)
    {
        if (lit || !canChange) return;

        lit = true;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat(shaderFlameFactorId, 1);
        SetMPB(mpb);

        var audio = SoundsLibrary.PlaySoundEffect("CandleLight");
        audio.transform.position = transform.position;
        audio.spatialBlend = 1;
        audio.volume = 0.3f;
        audio.pitch = Mathf.Lerp(0.8f, 1.2f, Random.value);
        //burst.SetTrigger("Burst");

        const float duration = 0.5f;
        float timer = duration;
        float deltaTime = 0.02f;

        canChange = false;

        while (timer > 0)
        {
            timer -= deltaTime;
            float t = timer / duration;
            mpb.SetFloat(shaderFlameFactorId, t);
            SetMPB(mpb);
            mpb.SetFloat(shaderTimeFactorId, 1-t);
            foreach (var r in burst)
            {
                r.SetPropertyBlock(mpb);
            }
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) break;
        }

        canChange = true;
    }

    private async void Dim(AsyncProcessId process)
    {
        if (!lit || !canChange) return;

        lit = false;
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mpb.SetFloat(shaderFlameFactorId, 0);
        SetMPB(mpb);

        const float duration = 0.5f;
        float timer = duration;
        float deltaTime = 0.02f;

        canChange = false;

        while (timer > 0)
        {
            timer -= deltaTime;
            float t = 1 - timer / duration;
            mpb.SetFloat(shaderFlameFactorId, t);
            SetMPB(mpb);
            //mpb.SetFloat(shaderTimeFactorId, 1 - t);
            //burst.SetPropertyBlock(mpb);
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (process.Canceled) break;
        }

        canChange = true;
    }
}
