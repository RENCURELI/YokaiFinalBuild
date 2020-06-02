using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMessageTeleportPlayer : MonoBehaviour
{
    public string messageToListenTo;
    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener(messageToListenTo, TeleportPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TeleportPlayer()
    {
        GameObject.FindGameObjectWithTag("Player").transform.position = transform.position;
        GameObject.FindGameObjectWithTag("Player").transform.rotation = transform.rotation;
    }
}
