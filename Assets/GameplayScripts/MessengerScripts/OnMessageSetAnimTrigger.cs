using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMessageSetAnimTrigger : MonoBehaviour
{
    public string TriggerToPlay;
    public string MessageToListen;

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener(MessageToListen, ActivateTrigger);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ActivateTrigger()
    {
        GetComponent<Animator>().SetTrigger(TriggerToPlay);
    }
}
