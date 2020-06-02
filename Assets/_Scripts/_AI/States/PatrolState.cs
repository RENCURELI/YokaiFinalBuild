using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "PatrolState", menuName = "ProjetEscape/States/Patrol", order = 2)]
public class PatrolState : AbstractFSMClass
{
    
    ConnectedWayPoints[] _patrolPoints;

    int _patrolPointIndex = 0;
    int _prevWaypointIndex = 0;
    private const float DIST_TO_POINT = 1.5f;
    


    ConnectedWayPoints _currentWaypoint;
    ConnectedWayPoints _previousWaypoint;

    int visitedWaypoint;

    public override void OnEnable()
    {
        base.OnEnable();
        _patrolPointIndex = 0;
        _prevWaypointIndex = _patrolPointIndex - 1;
        FSMStateType = FSMStateType.PATROL;
    }

    public override bool EnterState()
    {
        EnteredState = false;
        if (base.EnterState())
        {
            //Grab and store Patrol Points
            _patrolPoints = _npc.PatrolPoint;

            Debug.Log(_npc.g_roundPatrol);
            Debug.Log(_patrolPoints.Length);

            if (_currentWaypoint == null)
            {
                _currentWaypoint = _patrolPoints[0];
                
            }

            if (visitedWaypoint > 0)
            {
                if (_npc.g_roundPatrol == false && _patrolPointIndex >= (_patrolPoints.Length - 1))
                {
                    _previousWaypoint = _currentWaypoint;
                    _prevWaypointIndex = _patrolPointIndex;
                    --_patrolPointIndex;
                    _currentWaypoint = _patrolPoints[_patrolPointIndex];
                    _fsm.g_hasTurned = true;
                }
                else if (_patrolPointIndex < (_patrolPoints.Length - 1) && _fsm.g_hasTurned == false)
                {
                    _previousWaypoint = _currentWaypoint;
                    _prevWaypointIndex = _patrolPointIndex;
                    ++_patrolPointIndex;
                    _currentWaypoint = _patrolPoints[_patrolPointIndex];
                }
                else if (_fsm.g_hasTurned == true && _patrolPointIndex > 0)
                {
                    _previousWaypoint = _currentWaypoint;
                    _prevWaypointIndex = _patrolPointIndex;
                    --_patrolPointIndex;
                    _currentWaypoint = _patrolPoints[_patrolPointIndex];
                }
                else
                {
                    _previousWaypoint = _currentWaypoint;
                    _patrolPointIndex = 0;
                    _prevWaypointIndex = _patrolPointIndex - 1;
                    _currentWaypoint = _patrolPoints[_patrolPointIndex];
                    _fsm.g_hasTurned = false;
                }
                
            }
            
                EnteredState = true;
        }
        return EnteredState;
    }

    public override void UpdateState()
    {
        //TODO : Check successfull state entry
        if (EnteredState)
        {
            
            Vector3 targetVector = _currentWaypoint.transform.position;
            _navMeshAgent.SetDestination(targetVector);
            if (Vector3.Distance(_navMeshAgent.transform.position, _currentWaypoint.transform.position) <= DIST_TO_POINT)
            {
                visitedWaypoint++;
                _fsm.EnterState(FSMStateType.IDLE);
            }
        }
    }

    public override bool ExitState()
    {
        base.ExitState();
        Debug.Log("EXITED PATROL STATE");
        return true;
    }

    private void SetDestination(PatrolPoints destination)
    {
        if (_navMeshAgent != null && destination != null)
        {
            _navMeshAgent.SetDestination(destination.transform.position);
        }

    }

    public override void DetectedPlayer()
    {
        _fsm.EnterState(FSMStateType.ATTACK);
    }
}
