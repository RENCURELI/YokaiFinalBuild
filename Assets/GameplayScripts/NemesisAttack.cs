using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NemesisAttack : MonoBehaviour
{
    NavMeshAgent navAgent;
    GameObject playerRef;
    bool startAttacking = false;
    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        playerRef = GameObject.FindGameObjectWithTag("Player");
        Invoke("StartNemesissAttack", 4);
    }

    // Update is called once per frame
    void Update()
    {
        if (startAttacking)
        {
            navAgent.SetDestination(playerRef.transform.position);
        }   
    }

    void StartNemesissAttack()
    {
        startAttacking = true;
    }
}
