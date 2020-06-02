using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RandomSoundZone : SoundZone
{
    private bool playing = false;
    private AsyncProcessId mainProcess = new AsyncProcessId();

    public string[] names;
    public float waitTime = 5;

    private void OnDestroy()
    {
        playing = false;
        mainProcess.Cancel();
    }

    private async void PlayRandom(AsyncProcessId process)
    {
        int random = Random.Range(0, names.Length);
        while (process.valid)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(waitTime));
            if (process.Canceled) return;
            var audio = SoundsLibrary.PlaySoundEffect(names[random]);
            audio.transform.position = transform.position;
            audio.spatialBlend = spatialBlend;
            int other = random;
            while (other == random)
            {
                other = Random.Range(0, names.Length);
                await Task.Delay(System.TimeSpan.FromSeconds(0.01f));
                if (process.Canceled) return;
            }
            random = other;
        }
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    protected override void Play()
    {
        triggered = false;
        if (playing) return;
        playing = true;
        mainProcess = mainProcess.GetNew();
        PlayRandom(mainProcess);
    }

    protected override void Stop()
    {
        triggered = false;
        mainProcess.Cancel();
        playing = false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        Stop();
    }
}
