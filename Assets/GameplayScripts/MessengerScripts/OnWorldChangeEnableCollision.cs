using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnWorldChangeEnableCollision : MonoBehaviour
{

    public GamePhase linkedWorld;

    // Start is called before the first frame update
    void Start()
    {
        if (linkedWorld == GamePhase.Real)
        {
            Messenger.AddListener("EnteredSpiritWorld", DisableCollisionTrigger);
            Messenger.AddListener("EnteredRealWorld", EnableCollisionTrigger);
            EnableCollisionTrigger();
        } else
        {
            Messenger.AddListener("EnteredSpiritWorld", EnableCollisionTrigger);
            Messenger.AddListener("EnteredRealWorld", DisableCollisionTrigger);
            DisableCollisionTrigger();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EnableCollisionTrigger()
    {
        if (GetComponent<BoxCollider>() != null)
        {
            GetComponent<BoxCollider>().enabled = true;
        }
        else if (GetComponent<MeshCollider>() != null)
        {
            GetComponent<MeshCollider>().enabled = true;
        }
        else if (GetComponent<SphereCollider>() != null)
        {
            GetComponent<SphereCollider>().enabled = true;
        }
    }

    void DisableCollisionTrigger()
    {
        if (GetComponent<BoxCollider>() != null)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
        else if (GetComponent<MeshCollider>() != null)
        {
            GetComponent<MeshCollider>().enabled = false;
        }
        else if (GetComponent<SphereCollider>() != null)
        {
            GetComponent<SphereCollider>().enabled = false;
        }
    }
}
