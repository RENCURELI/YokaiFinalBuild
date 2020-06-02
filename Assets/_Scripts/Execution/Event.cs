using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process.Events
{
    /// <summary>
    /// A list of organized callbacks, which will automatically clear all broken callbacks.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The organized list of all callbacks of this event.
        /// </summary>
        public List<ProcessCallback> callbacks = new List<ProcessCallback>();

        /// <summary>
        /// Removes all broken callbacks.
        /// </summary>
        public void ClearGarbage()
        {
            int i = 0;
            while (true)
            {
                if (i >= callbacks.Count) break;
                if (callbacks[i].Verify())
                {
                    i++;
                } else
                {
                    callbacks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Invokes all callback in their order number.
        /// </summary>
        public void Invoke()
        {
            ClearGarbage();
            for (int i = 0; i < callbacks.Count; i++)
            {
                callbacks[i].Invoke();
            }
        }
    }
}