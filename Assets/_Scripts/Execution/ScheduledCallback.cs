using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process
{
    public class ScheduledCallback : ProcessCallback
    {
        public float timer;

        public ScheduledCallback(int eventId, int order, float time, VoidCallback callback) : base(eventId, order, callback)
        {
            timer = time;
        }

        public virtual ProcessCallback GetAfterProcess()
        {
            return new ProcessCallback(eventId, order, callback);
        }

        public override void Invoke()
        {
            timer -= Time.deltaTime;
            if (timer > 0) return;
            int id = ProcessId;
            ProcessCallback process = GetAfterProcess();
            GameProcess.events[eventId].callbacks[id] = process;
            process.Invoke();
        }
    }

    public class ScheduledUniqueCallback : ScheduledCallback
    {
        public ScheduledUniqueCallback(int eventId, int order, float time, VoidCallback callback) : base(eventId, order, time, callback)
        {
        }

        public override ProcessCallback GetAfterProcess()
        {
            return new UniqueCallback(eventId, order, callback);
        }
    }
}