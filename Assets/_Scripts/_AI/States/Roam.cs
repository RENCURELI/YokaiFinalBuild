using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts._AI.States
{
    //[CreateAssetMenu(fileName = "RoamState", menuName = "ProjetEscape/States/Roam", order = 4)]
    public class Roam : AbstractFSMClass
    {
        /// <summary>
        /// Last known player position
        /// </summary>
        Vector3 lastPCPos;

        /// <summary>
        /// Randomly selected roaming destination
        /// </summary>
        Vector3 roamDest;

        /// <summary>
        /// Current destination of the navmesh agent
        /// </summary>
        Vector3 _currentDest;

        /// <summary>
        /// Roaming distance constraints
        /// </summary>
        int min_x = -5, max_x = 5, min_z = -5, max_z = 5;

        float elapsedTime = 0.0f;

        [SerializeField]
        float roamDuration = 8.0f;

        public override void OnEnable()
        {
            base.OnEnable();
            FSMStateType = FSMStateType.ROAM;
        }

        public override bool EnterState()
        {
            EnteredState = false;
            if (base.EnterState())
            {
                elapsedTime = 0.0f;
                lastPCPos = GameObject.FindGameObjectWithTag("Player").transform.position;
                _currentDest = lastPCPos;
                _navMeshAgent.SetDestination(_currentDest);
                EnteredState = true;
            }
            Debug.Log("ENTERRED ROAM STATE");
            return EnteredState;
        }

        public override void UpdateState()
        {
            if (EnteredState)
            {
                elapsedTime += Time.deltaTime;
                if(Vector3.Distance(_navMeshAgent.transform.position, _currentDest) <= 1.5f)
                {
                    roamDest = new Vector3(UnityEngine.Random.Range(min_x, max_x), 0.5f, UnityEngine.Random.Range(min_z, max_z));
                    _currentDest = roamDest;
                    _navMeshAgent.SetDestination(_currentDest);
                }

                if (elapsedTime >= roamDuration)
                    _fsm.EnterState(FSMStateType.PATROL);
                
                Debug.Log("UPDATING ROAM STATE");
                //Debug.Log("Roaming Time = " + elapsedTime);
            }
        }

        public override bool ExitState()
        {
            base.ExitState();
            Debug.Log("EXITED ROAM STATE");
            return true;
        }

    }
}
