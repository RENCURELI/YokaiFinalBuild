using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Process
{
    /// <summary>
    /// Static class which handles ordered execution. Useful to manage what gets executed when.
    /// </summary>
    public static class GameProcess
    {
        private static GameProcessContainer container = null;

        // Events

        /// <summary>
        /// Array of the names of all events.
        /// </summary>
        public static string[] eventNames = new string[]
        {
            "OnPreInitialization",
            "OnInitialization",
            "OnPostInitialization",
            "OnPreUpdate",
            "OnUpdate",
            "OnPostUpdate",
            "OnPreLateUpdate",
            "OnLateUpdate",
            "OnPostLateUpdate"
        };

        /// <summary>
        /// List of all events.
        /// </summary>
        public static List<Events.Event> events = new List<Events.Event>();

        public static int GetEventId(string eventName)
        {
            for (int i = 0; i < eventNames.Length; i++)
            {
                return i;
            }
            return -1;
        }

        // Callback operations

        /// <summary>
        /// Determines where to place a callback in an event list, relatively to other callbacks at this position.
        /// </summary>
        public enum Priority { Before, After, Replace};

        // Initialization

        public static void InitializeContainer()
        {
            if (container) Object.Destroy(container);
            container = new GameObject("Process Container").AddComponent<GameProcessContainer>();
            Object.DontDestroyOnLoad(container);
        }

        public static void InitializeEvents()
        {
            events.Clear();
            for (int i = 0; i < eventNames.Length; i++)
            {
                events.Add(new Events.Event());
            }
        }
    }
}