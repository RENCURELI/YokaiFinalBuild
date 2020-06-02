using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts._AI.States
{
    //[CreateAssetMenu(fileName = "AttackState", menuName = "ProjetEscape/States/Attack", order = 3)]
    public class AttackState : AbstractFSMClass
    {
        Transform player;


        public override void OnEnable()
        {
            base.OnEnable();
            FSMStateType = FSMStateType.ATTACK;
        }

        public override bool EnterState()
        {
            EnteredState = false;
            if (base.EnterState())
            {
                player = FindObjectOfType<Player>().transform;
                Vector3 targetVector = player.transform.position;
                _navMeshAgent.SetDestination(targetVector);
                Debug.Log("ENTERRED ATTACK STATE");
                EnteredState = true;
            }
            
            return EnteredState;
        }

        //TO DO
        public override void UpdateState()
        {
            if (EnteredState)
            {
                Debug.Log("UPDATING ATTACK STATE");
            }
        }

        public override bool ExitState()
        {
            base.ExitState();
            Debug.Log("EXITED ATTACK STATE");
            return true;
        }

        private void SetDestination(PatrolPoints destination)
        {
            if (_navMeshAgent != null && destination != null)
            {
                _navMeshAgent.SetDestination(destination.transform.position);
            }

        }

        public void LostPlayer()
        {
            _fsm.EnterState(FSMStateType.ROAM);
        }
    }
}
