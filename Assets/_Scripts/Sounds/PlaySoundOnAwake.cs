using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnAwake : MonoBehaviour
{
    public string sound;
    [Range(0,1)]
    public float blend;
    
    void Start()
    {
        var audio = SoundsLibrary.PlaySoundEffect(sound);
        audio.transform.position = transform.position;
        audio.spatialBlend = blend;
        Destroy(audio, audio.clip.length + 1);
    }
}
