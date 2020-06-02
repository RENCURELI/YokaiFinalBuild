using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessProxy : MonoBehaviour
{
    public PostProcessEffect bloom;
    [Range(0,1)]
    public float bloomBlend;

    public PostProcessEffect radialBlur;
    [Range(0, 1)]
    public float radialBlurBlend;

    public PostProcessEffect farClipWave;
    [Range(0, 1)]
    public float farClipWaveBlend;


    void Start()
    {
        
    }
    
    void Update()
    {
        farClipWaveBlend = radialBlurBlend;

        bloom.blend = bloomBlend;
        radialBlur.blend = radialBlurBlend;
        farClipWave.blend = farClipWaveBlend;
    }
}
