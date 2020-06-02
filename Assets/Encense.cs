using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Game.Utility;

public class Encense : MonoBehaviour
{
    public static Encense main;
    public static List<Encense> all = new List<Encense>();
    
    public static int CurrentRendererId
    {
        get
        {
            float timeLeft = GameManager.main.getCurrentTimeLeft();
            return Mathf.CeilToInt(timeLeft / 60.0f) - 1;
        }
    }

    public Renderer[] stickRenderers;
    public Renderer mainRenderer;
    public Renderer smokeRenderer;
    //public Renderer sparkles;

    public static float BurnFactor
    {
        get
        {
            float timeLeft = GameManager.main.getCurrentTimeLeft();
            float currentMinute = Mathf.Ceil(timeLeft / 60.0f);
            float currentSeconds = currentMinute * 60 - timeLeft;
            return currentSeconds / 60.0f;
        }
    }

    private static MaterialPropertyBlock mpb;
    private static Material mainMaterial;
    private static Material smokeMaterial;
    private static Material sparklesMaterial;
    private static int burntFactorPropId = Shader.PropertyToID("_BurntFactor");
    private static int smokeColorPropId = Shader.PropertyToID("_SpiritColor");
    private static int emitFactorPropId = Shader.PropertyToID("_EmitIntensity");
    private static int sparklesFactorPropId = Shader.PropertyToID("_Intensity");
    private static float emitFactor = 0;

    private void Awake()
    {
        main = null;
    }

    void Start()
    {
        if (!main)
        {
            main = this;
            mpb = new MaterialPropertyBlock();
            smokeMaterial = smokeRenderer.sharedMaterial;
            sparklesMaterial = gameObject.FindChild("Sparkles").GetComponent<Renderer>().sharedMaterial;
            mainMaterial = stickRenderers[0].sharedMaterial;
            emitFactor = 0;
            mainProcess = mainProcess.GetNew();
            WaitForStartSignal(mainProcess);
        }
        all.Add(this);
    }

    private void OnDestroy()
    {
        mainProcess.Cancel();
        all.Clear();
    }

    public static void ApplyMaterial()
    {
        if (CurrentRendererId < 0) return;
        foreach (var i in all)
        {
            if (i != null)
            {
                i.stickRenderers[CurrentRendererId].SetPropertyBlock(mpb);
            }
        }
    }

    public static void ApplyAllMaterial()
    {
        if (CurrentRendererId < 0) return;
        foreach (var i in all)
        {
            if (i != null)
            {
                foreach (var r in i.stickRenderers)
                {
                    r.SetPropertyBlock(mpb);
                }
            }
        }
    }

    public static void ApplyEmitFactor()
    {
        mainMaterial.SetFloat(emitFactorPropId, emitFactor);
        sparklesMaterial.SetFloat(sparklesFactorPropId, emitFactor);
        //ApplyMaterial();
        mpb.SetFloat(burntFactorPropId, 1);
        ApplyAllMaterial();
    }

    public static void ApplySmokeOpacity(float amount)
    {
        smokeMaterial.SetColor(smokeColorPropId, Color.white * amount);
    }

    #region ASYNC
    private static AsyncProcessId mainProcess = new AsyncProcessId();

    /// <summary>
    /// Attend que le Game Manager commence son compte a rebour, puis allume les encens.
    /// </summary>
    private static async void WaitForStartSignal(AsyncProcessId process)
    {
        const float delatTime = 0.2f;

        await Task.Delay(System.TimeSpan.FromSeconds(1));

        ApplySmokeOpacity(0);
        emitFactor = 0;
        ApplyEmitFactor();

        Debug.Log("Encens is waiting for signal...");

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(delatTime));
            if (GameManager.main.timerActive)
            {
                LightAll(process);
                break;
            }
            if (process.Canceled) break;
        }
        Debug.Log("Encens got signal to start or got canceled");
    }

    /// <summary>
    /// Allume tous les batons, puis commence a les consommer un par un.
    /// </summary>
    private static async void LightAll(AsyncProcessId process)
    {
        const float deltaTime = 0.02f;
        const float duration = 0.5f;
        const float speed = 1.0f / duration;

        Debug.Log("Lighting encens...");

        float timer = duration;
        while (timer > 0)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            timer -= deltaTime;
            emitFactor += speed * deltaTime;
            ApplyEmitFactor();
            ApplySmokeOpacity(emitFactor);
            if (process.Canceled) break;
        }
        emitFactor = 1;
        ApplyEmitFactor();
        ApplySmokeOpacity(emitFactor);
        if (process.valid) BurnStick(process);
    }

    /// <summary>
    /// Allume tous les batons, puis commence a les consommer un par un.
    /// Une fois que tous sont consummes, eteint l'encens.
    /// </summary>
    private static async void BurnStick(AsyncProcessId process)
    {
        const float deltaTime = 0.03f;

        Debug.Log("Burning encens...");

        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            if (CurrentRendererId < 0 /*|| GameManager.main.showedNoTimeLeft*/)
            {
                DimAll(process);
                break;
            } else
            {
                mpb.SetFloat(burntFactorPropId, 1 - BurnFactor);
                ApplyMaterial();
            }
        }
        
    }

    /// <summary>
    /// Eteint tous les batons et arrete le processus.
    /// </summary>
    private static async void DimAll(AsyncProcessId process)
    {
        const float deltaTime = 0.02f;
        const float duration = 1.0f;
        const float speed = 1.0f / duration;

        Debug.Log("Dimming encens...");

        emitFactor = 1;

        float timer = duration;
        while (timer > 0)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
            timer -= deltaTime;
            emitFactor -= speed * deltaTime;
            ApplyEmitFactor();
            ApplySmokeOpacity(emitFactor);
            if (process.Canceled) break;
        }

        emitFactor = 0;
        ApplyEmitFactor();
        ApplySmokeOpacity(emitFactor);
        process.Cancel();
        Debug.Log("Encens dimmed...");
    }
    
    public static void PrisonBreak()
    {
        foreach (var i in all)
        {
            Animator animator = i.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Break");
            }
        }
    }

    #endregion
}
