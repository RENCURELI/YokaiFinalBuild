using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StateMachine
{
    [System.Serializable]
    public class ActionState : State
    {
        public bool done = false;

        public override void OnGUI()
        {
            GUILayout.Label("Action State");
            GUILayout.Space(5);
            GUILayout.Label("Name: " + name);
            GUILayout.Label("Done: " + done);
            GUILayout.Space(5);
            GUILayout.Label("Bindings: ");
            foreach (var i in GetDelegates())
            {
                GUILayout.Label(i.Name);
            }
        }

        public override void Activate()
        {
            base.Activate();
            done = false;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            done = false;
        }
    }

    public void AddActionState(string name)
    {
        states.Add(new ActionState() { name = name, position = Vector2.right * states.Count * 300 });
    }
}