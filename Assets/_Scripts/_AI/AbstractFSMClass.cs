using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ExecutionState
{
    NONE,
    ACTIVE,
    COMPLETED,
    TERMINATED,
};

public enum FSMStateType
{
    NONE,
    IDLE,
    PATROL,
    ATTACK,
    ROAM,
};

public abstract class AbstractFSMClass : MonoBehaviour
{
    protected NavMeshAgent _navMeshAgent;
    protected NPC _npc = new NPC();
    protected FiniteStateMachine _fsm = new FiniteStateMachine();
    

    public ExecutionState ExecutionState { get; protected set; }
    public bool EnteredState { get; protected set; }
    public FSMStateType FSMStateType { get; protected set; }
    

    /// <summary>
    /// Default setup : None active
    /// </summary>
    public virtual void OnEnable()
    {
        ExecutionState = ExecutionState.NONE;
    }

    /// <summary>
    /// Check if state is entered without issue
    /// </summary>
    /// <returns></returns>
    public virtual bool EnterState()
    {
        bool successNavMesh = true;
        bool successNPC = true;
        ExecutionState = ExecutionState.ACTIVE;

        //Does navmesh agent exist
        successNavMesh = (_navMeshAgent != null);

        //Does executing agent exist
        successNPC = (_npc != null);

        return successNavMesh & successNPC;
    }


    /// <summary>
    /// Tells FSM to update current state
    /// </summary>
    public abstract void UpdateState();

    /// <summary>
    /// Check if state is exited without issue
    /// </summary>
    /// <returns></returns>
    public virtual bool ExitState()
    {
        ExecutionState = ExecutionState.COMPLETED;
        return true;
    }

    public virtual void DetectedPlayer()
    {
        
    }

    public virtual void SetNavMeshAgent(NavMeshAgent navMeshAgent)
    {
        if (navMeshAgent != null)
        {
            _navMeshAgent = navMeshAgent;
        }
    }

    public virtual void SetExecutingFSM(FiniteStateMachine fsm)
    {
        if (fsm != null)
        {
            _fsm = fsm;
        }
    }

    public virtual void SetExecutingNPC(NPC npc)
    {
        if (npc != null)
        {
            _npc = npc;
        }
    }
}
