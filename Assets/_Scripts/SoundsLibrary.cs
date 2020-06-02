using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sounds Library", fileName = "MainSoundsLibrary")]
public class SoundsLibrary : ScriptableObject
{
    private const string resourcePath = "MainSoundsLibrary";

    private static SoundsLibrary main;
    public static SoundsLibrary Main
    {
        get
        {
            if (!main) main = Resources.Load<SoundsLibrary>(resourcePath);
            return main;
        }
    }

    [System.Serializable]
    public struct SoundDescriptor
    {
        public string name;
        public AudioClip clip;
        public float volumeScale;
    }

    public SoundDescriptor[] sounds;

    public SoundDescriptor GetSound(string name)
    {
        foreach (var i in sounds)
        {
            if (i.name == name) return i;
        }
        throw new System.Exception("Sound Does not exist in library: " + name);
    }

    public AudioSource GetLoopedSound(string name)
    {
        if (runtimeLoopedDict.ContainsKey(name))
        {
            return runtimeLoopedDict[name];
        }
        return null;
    }

    public static bool LoopIsPlaying(string name)
    {
        return Main.GetLoopedSound(name) != null;
    }

    public static AudioSource PlaySoundEffect(string name)
    {
        var descriptor = Main.GetSound(name);
        GameObject obj = new GameObject("Sound - " + name);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = descriptor.clip;
        source.volume = descriptor.volumeScale;
        source.Play();
        Destroy(obj, descriptor.clip.length + 1);
        obj.hideFlags = HideFlags.HideAndDontSave;
        return source;
    }

    private static Dictionary<string, AudioSource> runtimeLoopedDict = new Dictionary<string, AudioSource>();

    public static AudioSource PlayLooped(string name)
    {
        var descriptor = Main.GetSound(name);
        GameObject obj = new GameObject("Sound (looped) - " + name);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = descriptor.clip;
        source.volume = descriptor.volumeScale;
        source.loop = true;
        source.Play();
        obj.hideFlags = HideFlags.HideAndDontSave;
        runtimeLoopedDict.Add(name, source);
        return source;
    }

    public static void StopLooped(string name)
    {
        if (runtimeLoopedDict.ContainsKey(name))
        {
            if (runtimeLoopedDict[name] != null)
            {
                runtimeLoopedDict[name].Stop();
                Destroy(runtimeLoopedDict[name], 1);
            }
            runtimeLoopedDict.Remove(name);
        }
    }

    public static void StopAllLooped()
    {
        foreach (var i in runtimeLoopedDict.Keys)
        {
            if (runtimeLoopedDict[i] != null)
            {
                runtimeLoopedDict[i].Stop();
                Destroy(runtimeLoopedDict[i], 1);
            }
        }
        runtimeLoopedDict.Clear();
    }

    public static async void PlaySoundEffectDelayed(string name, float delay)
    {
        await Task.Delay(System.TimeSpan.FromSeconds(delay));
        PlaySoundEffect(name);
    }

    public static async void Fade(string name, float startVolume, float endVolume, float duration, bool destroyOnEnd)
    {
        AudioSource audio;

        if (runtimeLoopedDict.ContainsKey(name))
            audio = runtimeLoopedDict[name];
        else
            audio = PlayLooped(name);

        float timer = duration;

        float deltaTime = 0.05f;

        while(timer > 0)
        {
            timer -= deltaTime;
            float t = timer / duration;
            float volume = Mathf.Lerp(endVolume, startVolume, t);
            if (audio == null) return;
            audio.volume = volume;
            await Task.Delay(System.TimeSpan.FromSeconds(deltaTime));
        }

        if (audio == null) return;

        audio.volume = endVolume;

        if (destroyOnEnd)
        {
            runtimeLoopedDict.Remove(name);
            GameObject.Destroy(audio.gameObject);
        }
    }

    public static void FadeIn(string name, float duration)
    {
        StopLooped(name);
        Fade(name, 0, Main.GetSound(name).volumeScale, duration, false);
    }

    public static void FadeOut(string name, float duration)
    {
        Fade(name, Main.GetSound(name).volumeScale, 0, duration, true);
    }

    public static void AttachInWorld(string name, Vector3 position, float blend)
    {
        AudioSource source = runtimeLoopedDict[name];
        source.transform.position = position;
        source.spatialBlend = blend;
    }

    public static void AttachInWorld(string name, Transform parent, float blend)
    {
        AudioSource source = runtimeLoopedDict[name];
        source.transform.parent = parent;
        source.transform.localPosition = Vector3.zero;
        source.spatialBlend = blend;
    }

    private static List<string> oneShotNoOverridesPlaying = new List<string>();
    public static async void PlayOneShotNoOverride(string name, Transform parent=null, float blend=1)
    {
        if (oneShotNoOverridesPlaying.Contains(name)) return;
        oneShotNoOverridesPlaying.Add(name);
        AudioSource source = PlaySoundEffect(name);
        if (parent)
        {
            source.transform.parent = parent;
            source.transform.localPosition = source.transform.localEulerAngles = Vector3.zero;
            source.spatialBlend = blend;
        }
        await Task.Delay(System.TimeSpan.FromSeconds(source.clip.length + 0.1f));
        if (source) Destroy(source.gameObject);
        oneShotNoOverridesPlaying.Remove(name);
    }
}
