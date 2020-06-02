using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public partial class StateMachine
{
    [System.Serializable]
    public class State
    {
        public delegate void VoidCallback(StateMachine stateMachine, State state, float deltaTime);

        public string name;
        public bool active = false;
        public event VoidCallback onActive;

        // Editor
        public Vector2 position;

        public void Bind(VoidCallback callback)
        {
            onActive += callback;
        }

        public void Unbind(VoidCallback callback)
        {
            onActive -= callback;
        }

        public void Update(StateMachine stateMachine, float deltaTime)
        {
            onActive?.Invoke(stateMachine, this, deltaTime);
        }

        public void Dispose()
        {
            onActive = null;
        }

        public System.Reflection.MethodInfo[] GetDelegates()
        {
            List<System.Reflection.MethodInfo> methods = new List<System.Reflection.MethodInfo>();
            var eventArgs = System.EventArgs.Empty;
            var eventInfo = typeof(State).GetEvent("onActive", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var eventDelegate = (System.MulticastDelegate)typeof(State).GetField("onActive", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    methods.Add(handler.Method);
                }
            }
            return methods.ToArray();
        }

        public virtual void Activate()
        {
            active = true;
        }

        public virtual void Deactivate()
        {
            active = false;
        }

        public virtual void OnGUI()
        {
            GUILayout.Label("State");
            GUILayout.Space(5);
            GUILayout.Label("Name: " + name);
            GUILayout.Space(5);
            GUILayout.Label("Bindings: ");
            foreach (var i in GetDelegates())
            {
                GUILayout.Label(i.Name);
            }
        }
    }

    [System.Serializable]
    public class Transition
    {
        public delegate bool ConditionCallback(State a, State b);

        public string fromName;
        public string toName;
        public string Name
        {
            get { return fromName + " > " + toName; }
        }

        public ConditionCallback condition;

        public static bool Always(State a, State b)
        {
            return true;
        }

        public virtual void OnGUI()
        {
            GUILayout.Label("Transition");
            GUILayout.Space(5);
            GUILayout.Label("From: " + fromName);
            GUILayout.Label("To: " + toName);
            GUILayout.Space(5);
            GUILayout.Label("Condition: ");
            GUILayout.Label(condition.Method.Name);
        }
    }

    public string name;
    public float updateTime = 1f;
    public List<State> states = new List<State>();
    public List<Transition> transitions = new List<Transition>();
    private bool built = false;
    private System.Action<StateMachine> buildFunction;

    public StateMachine(string name, float updateTime)
    {
        this.name = name;
        this.updateTime = updateTime;
        states = new List<State>();
        transitions = new List<Transition>();

        AddState("Start");
        GetState("Start").Activate();
        
    }

    public void AddState(string name)
    {
        foreach (var i in states)
        {
            if (i.name == name) throw new System.Exception("State " + name + " already exists.");
        }
        states.Add(new State() { name = name, position = Vector2.right * states.Count * 300});
    }

    public State GetState(string name)
    {
        foreach (var i in states)
        {
            if (i.name == name) return i;
        }
        throw new System.Exception("State " + name + " does not exist.");
    }

    public Transition GetTransition(string from, string to)
    {
        foreach (var i in transitions)
        {
            if (i.fromName == from && i.toName == to) return i;
        }
        throw new System.Exception("Transition from: " + from + " to: " + to + " does not exist.");
    }

    public void BindState(string name, State.VoidCallback callback)
    {
        GetState(name).Bind(callback);
    }

    public void UnbindState(string name, State.VoidCallback callback)
    {
        GetState(name).Unbind(callback);
    }

    public void Connect(string from, string to, Transition.ConditionCallback condition)
    {
        foreach (var i in transitions)
        {
            if (i.fromName == from && i.toName == to) { i.condition = condition; return; }
        }
        Transition t = new Transition() { fromName = from, toName = to, condition = condition };
        transitions.Add(t);
    }

    public void Disconnect(string from, string to)
    {
        for (int i = 0; i < transitions.Count; i++)
        {
            if ((transitions[i].fromName == from || transitions[i].fromName == "ANY") && (transitions[i].toName == to || transitions[i].toName == "ANY"))
            {
                transitions.RemoveAt(i);
                i--;
                continue;
            }
        }
    }

    private void UpdateStates()
    {
        foreach (var i in states)
        {
            if (i.active)
            {
                i.Update(this, updateTime);
            }
        }
    }
    [System.NonSerialized] public List<Transition> validTransition = new List<Transition>();
    private void UpdateTransitions()
    {
        validTransition.Clear();
        List<State> statesToActivate = new List<State>();
        List<State> statesToDeactivate = new List<State>();
        foreach (var from in states)
        {
            foreach (var t in transitions)
            {
                if (from.active)
                {
                    if (t.fromName == from.name && t.fromName != "ANY")
                    {
                        State to = GetState(t.toName);
                        if (t.condition(from, to))
                        {
                            validTransition.Add(t);
                            statesToActivate.Add(to);
                            statesToDeactivate.Add(from);
                        }
                    }
                }
                else if (t.fromName == "ANY")
                {
                    State to = GetState(t.toName);
                    if (t.condition(from, to))
                    {
                        validTransition.Clear();
                        validTransition.Add(t);
                        statesToActivate.Clear();
                        statesToDeactivate.Clear();
                        foreach (var any in states)
                        {
                            statesToDeactivate.Add(any);
                        }
                        statesToActivate.Add(to);
                    }
                }
            }
        }
        foreach (var i in statesToDeactivate)
        {
            i.Deactivate();
        }
        foreach (var i in statesToActivate)
        {
            i.Activate();
        }
    }

    private float cancelUpdateToken;
    public async void Update()
    {
        float cancel = cancelUpdateToken = UnityEngine.Random.value;
        while (cancel == cancelUpdateToken)
        {
            await Task.Delay(System.TimeSpan.FromSeconds(updateTime));
            if (cancel != cancelUpdateToken) return;
            UpdateStates();
            if (cancel != cancelUpdateToken) return;
            UpdateTransitions();
        }
        Debug.Log("State Machine canceled");
    }

    public void Build()
    {
        buildFunction?.Invoke(this);
        built = true;
    }

    public void Start()
    {
        if (!built)
        {
            Build();
        }
        Update();
    }

    public void Stop()
    {
        cancelUpdateToken = -1;
    }

    public void Dispose()
    {
        Stop();
        foreach (var i in states)
        {
            i.Dispose();
        }
        foreach (var i in transitions)
        {
            i.condition = null;
        }
        Debug.Log("State Machine Disposed");
    }
}

/*
// Sample State Machine
#region STATE_MACHINE

#region MACHINE

public StateMachine stateMachine;
public Game.Utility.InputListener useInputListener = new Game.Utility.InputListener(Game.Controller.InputType.VALID, Game.Utility.InputListener.EventType.DOWN);

private void UseInputs()
{
    useInputListener.Use();
}

private void InitializeStateMachine()
{
    stateMachine = new StateMachine("Sample", 0.01f);
    // States
    stateMachine.AddState("End");
    // Inputs
    Game.Controller.AddListener(useInputListener);
    // Bindings
    stateMachine.BindState("Start", State_Start);
    stateMachine.BindState("End", State_End);
    // Transitions
    stateMachine.Connect("Start", "End", Transition_Use);
    //
    stateMachine.Start();
}

#endregion MACHINE

#region STATES

private void State_Start(StateMachine stateMachine, StateMachine.State state, float deltaTime)
{
}

private void State_End(StateMachine stateMachine, StateMachine.State state, float deltaTime)
{

}
        
#endregion STATES

#region TRANSITIONS

private bool Transition_Always(StateMachine.State a, StateMachine.State b)
{
    return true;
}

private bool Transition_Use(StateMachine.State a, StateMachine.State b)
{
    if (useInputListener.Get())
    {
        UseInputs();
        return true;
    }
    return false;
}
        
#endregion TRANSITIONS

#endregion STATE_MACHINE
*/
