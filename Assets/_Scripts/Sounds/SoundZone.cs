using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundZone : MonoBehaviour
{
    private static List<SoundZone> all = new List<SoundZone>();

    public string soundName;
    public float fadeDuration = 3;
    [Range(0,1)]
    public float spatialBlend = 1;

    protected bool triggered = false;

    private void Awake()
    {
        all.Add(this);
    }

    private void OnDestroy()
    {
        if (all.Contains(this)) all.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.GetComponent<Player>() != null)
        {
            Play();
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!triggered) return;
        if (other.GetComponent<Player>() != null)
        {
            Stop();
        }
    }

    protected virtual void Play()
    {
        if (SoundsLibrary.LoopIsPlaying(soundName)) return;
        triggered = true;
        SoundsLibrary.FadeIn(soundName, fadeDuration);
        SoundsLibrary.AttachInWorld(soundName, transform, spatialBlend);
    }

    protected virtual void Stop()
    {
        triggered = false;
        SoundsLibrary.FadeOut(soundName, fadeDuration);
    }

    public static void ResetAll()
    {
        foreach (var i in all)
        {
            if (i != null)
            {
                i.Stop();
            }
        }
    }
}
