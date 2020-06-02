using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimOnMessage : MonoBehaviour
{
    public string messageToListenTo;

    // Start is called before the first frame update
    void Start()
    {
        Messenger.AddListener(messageToListenTo, ActivateAnimator);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ActivateAnimator()
    {
        GetComponent<Animator>().enabled = true;
    }
}
