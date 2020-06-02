using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process
{

    public class UniqueCallback : ProcessCallback
    {
        public bool done;

        public UniqueCallback(int eventId, int order, VoidCallback callback) : base(eventId, order, callback)
        {
            done = false;
        }

        public override bool Verify()
        {
            if (done) return false;
            return base.Verify();
        }

        public override void Invoke()
        {
            base.Invoke();
            done = true;
        }
    }
}