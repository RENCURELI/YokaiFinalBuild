using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnMessage : MonoBehaviour
{
    public string messageToListenTo;
    public GameObject linkedObject;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener(messageToListenTo, OnMessageReceived);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMessageReceived()
    {
        Destroy(linkedObject);
    }
}
