using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FiniteStateMachine : MonoBehaviour
{

    [SerializeField]
    AbstractFSMClass _startingState;
    AbstractFSMClass _currentState;

    [SerializeField]
    List<AbstractFSMClass> _validStates;

    [HideInInspector]
    public bool g_hasTurned = false;

    Dictionary<FSMStateType, AbstractFSMClass> _fsmStates;


    public void Awake()
    {
        _currentState = null;
        _fsmStates = new Dictionary<FSMStateType, AbstractFSMClass>();

        NavMeshAgent navMeshAgent = this.GetComponent<NavMeshAgent>();
        NPC npc = this.GetComponent<NPC>();

        foreach (AbstractFSMClass state in _validStates)
        {
            
            state.SetExecutingFSM(this.gameObject.GetComponent<FiniteStateMachine>()); //Must find solution to make instances
            state.SetExecutingNPC(npc);
            state.SetNavMeshAgent(navMeshAgent);
            if(!_fsmStates.ContainsKey(0))
                _fsmStates.Add(state.FSMStateType, state);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        if (_startingState != null)
        {
            EnterState(_startingState);
        }
    }

    public void Update()
    {
        if (_currentState != null)
        {
            _currentState.UpdateState();
        }
    }

    #region STATE MANAGEMENT

    public void EnterState(AbstractFSMClass nextState)
    {
        if (_startingState == null)
        {
            return;
        }

        if (_currentState != null)
        {
            _currentState.ExitState();
        }

        _currentState = nextState;
        _currentState.EnterState();
    }

    public void EnterState(FSMStateType Type)
    {
        if (_fsmStates.ContainsKey(Type))
        {
            AbstractFSMClass nextstate = _fsmStates[Type];

            EnterState(nextstate);
        }
    }
    

    #endregion
}
