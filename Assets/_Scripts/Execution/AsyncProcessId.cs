using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AsyncProcessId
{
    public bool valid = true;

    public bool Canceled
    {
        get
        {
            return !valid;
        }
    }

    public AsyncProcessId GetNew()
    {
        Cancel();
        AsyncProcessId newProcess = new AsyncProcessId();
        newProcess.valid = true;
        return newProcess;
    }

    public void Cancel()
    {
        valid = false;
    }

    public static async Task Wait(float seconds)
    {
        await Task.Delay(System.TimeSpan.FromSeconds(seconds));
        return;
    }
}
