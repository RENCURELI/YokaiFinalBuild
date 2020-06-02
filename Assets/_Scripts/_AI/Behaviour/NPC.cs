using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent), typeof(FiniteStateMachine))]
public class NPC : MonoBehaviour
{

    NavMeshAgent _navMeshAgent;
    FiniteStateMachine _finiteStateMachine;

    [SerializeField]
    ConnectedWayPoints[] _patrolPoints;

    
    [SerializeField]
    public bool g_roundPatrol;

    public void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _finiteStateMachine = new FiniteStateMachine();
    }
    

    public ConnectedWayPoints[] PatrolPoint
    {
        get
        {
            return _patrolPoints;
        }
    }
}
