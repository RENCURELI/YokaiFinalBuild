using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveOnMessage : MonoBehaviour
{
    public string messageToListenTo;
    public GameObject linkedObject;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener<bool>(messageToListenTo, OnMessageReceived);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMessageReceived(bool status)
    {
        linkedObject.SetActive(status);
    }
}
