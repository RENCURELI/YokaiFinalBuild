using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process {

    public class Process : System.Attribute
    {
        public int eventId;
        public int order;
        public GameProcess.Priority priority;

        public Process(string eventName, int order, GameProcess.Priority priority)
        {
            eventId = GameProcess.GetEventId(eventName);
            this.order = order;
            this.priority = priority;
        }

        public Process(string eventName, int order)
        {
            eventId = GameProcess.GetEventId(eventName);
            this.order = order;
            priority = GameProcess.Priority.Before;
        }

        public Process(string eventName)
        {
            eventId = GameProcess.GetEventId(eventName);
            order = 0;
            priority = GameProcess.Priority.Before;
        }
    }
}