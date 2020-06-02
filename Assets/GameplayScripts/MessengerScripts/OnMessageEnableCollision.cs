using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMessageEnableCollision : MonoBehaviour
{
    public string messageToListenTo;
    public bool disable = false;

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener(messageToListenTo, EnableCollisionTrigger);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EnableCollisionTrigger()
    {
        if (GetComponent<BoxCollider>() != null)
        {
            GetComponent<BoxCollider>().enabled = !disable;
        }
        else if (GetComponent<MeshCollider>() != null)
        {
            GetComponent<MeshCollider>().enabled = !disable;
        }
        else if (GetComponent<SphereCollider>() != null)
        {
            GetComponent<SphereCollider>().enabled = !disable;
        }

    }
}
