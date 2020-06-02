using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process
{
    /// <summary>
    /// Base class for all callbacks in an event
    /// </summary>
    public class ProcessCallback
    {
        /// <summary>
        /// A method with no arguments and no return type
        /// </summary>
        public delegate void VoidCallback();

        /// <summary>
        /// The method bound to this callback
        /// </summary>
        public VoidCallback callback;
        /// <summary>
        /// The id of the event this callback is bound to
        /// </summary>
        public int eventId;
        /// <summary>
        /// The execution order of this callback
        /// Smaller order numbers are invoked first
        /// </summary>
        public int order;

        public ProcessCallback(int eventId, int order, VoidCallback callback)
        {
            this.eventId = eventId;
            this.order = order;
            this.callback = callback;
        }

        /// <summary>
        /// The position of this process in the event's list of callbacks.
        /// </summary>
        public int ProcessId
        {
            get
            {
                return GameProcess.events[eventId].callbacks.IndexOf(this);
            }
        }

        /// <summary>
        /// Used to determine wether this callback is broken or not.
        /// </summary>
        public virtual bool Verify()
        {
            if (callback.Target == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Invokes the function bound to this callback, if any.
        /// </summary>
        public virtual void Invoke()
        {
            callback?.Invoke();
        }
    }
}